using Platform.DataEntities;
using Platform.DeviceAccess.Base;
using Platform.DeviceAccess.Transfer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Platform.DeviceAccess.Execute
{
    /// <summary>ModbusRTU 执行对象</summary>
    internal class ModbusRTU : ExecuteObject
    {
        #region 匹配
        internal override Result Match(List<DevicePropEntity> propList, List<TransferObject> transferObjList)
        {
            PropList = propList;
            Result result = new Result();

            var ps = propList.Where(p => p.PropName == "PortName").Select(p => p.PropValue).ToList();

            var transferObj = transferObjList.FirstOrDefault(to => to.GetType().Name == "TransferModbusRTU" && ps.All(s => to.Condition.Any(c => c == s)));

            if (transferObj == null)
            {
                TransferObject = new TransferModbusRTU();
                TransferObject.Condition = ps;
                TransferObject.Config(PropList);
                transferObjList.Add(TransferObject);
            }
            else
            {
                TransferObject = transferObj;
            }

            result.Status = true;
            result.Msg = "没有错误";
            return result;
        }
        #endregion

        #region 读，最后转
        public override Result Read(List<GroupAddress> groupAddrList)
        {
            Result result = new Result { Status = true, Msg = "读数据成功" };
            try
            {
                #region 前提准备
                // 从站地址校验
                var slaveResult = CheckSlaveId();
                if (!slaveResult.Status) { result.Status = false; result.Msg = slaveResult.Msg; return result; }
                byte slaveId = slaveResult.Data;

                // 读设备端配置（只读一次）
                bool deviceLittleEndian = GetProp("DeviceEndian") == "Little";
                bool lowWordFirst = GetProp("WordOrder") == "LowFirst";
                #endregion

                #region 协议实现
                // 遍历分组
                foreach (var ga in groupAddrList)
                {
                    if (ga is not ModbusGroupAddress group) continue;

                    //获取组装报文的纯数据部分，读方法
                    List<byte> byteList = ReadGroupBytes(slaveId, group);

                    bool isCoil = group.FuncCode == FC_ReadCoils || group.FuncCode == FC_ReadDiscreteInputs;
                    bool isRegister = group.FuncCode == FC_ReadHoldingRegisters || group.FuncCode == FC_ReadInputRegisters;

                    // 线圈转字节
                    List<byte> bitList = null;
                    if (isCoil)
                    {
                        bitList = new List<byte>(byteList.Count * 8);
                        foreach (byte b in byteList)
                            for (int i = 0; i < 8; i++)
                                bitList.Add((byte)((b >> i) & 1));
                    }

                    // 每个变量按偏移切片
                    foreach (var varProp in group.VarPropList)
                    {
                        int relativeAddr = int.Parse(varProp.VarAddr.Substring(1)) - 1 - group.StartAddress;
                        int count = varProp.Count <= 0 ? 1 : varProp.Count;

                        if (isCoil)
                        {
                            // 线圈：取 count 个
                            varProp.ReadBytes = bitList.GetRange(relativeAddr, count).ToArray();
                            varProp.ReadValue = ByteConvert.ToValues(varProp.ReadBytes, varProp.ValueType, count);
                        }
                        else if (isRegister)
                        {
                            // 寄存器：切 ReadByteCount 字节（已是总数）
                            int startIndex = relativeAddr * 2;
                            byte[] raw = byteList.GetRange(startIndex, varProp.ReadByteCount).ToArray();
                            varProp.ReadBytes = ConvertEndian(raw, varProp.ValueType, deviceLittleEndian, lowWordFirst);
                            varProp.ReadValue = ByteConvert.ToValues(varProp.ReadBytes, varProp.ValueType, count);
                        }
                        else
                        {
                            throw new Exception($"不支持的功能码: 0x{group.FuncCode:X2}");
                        }
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.Msg = ex.Message;
            }
            return result;
        }

        /// <summary>读整组数据部分字节</summary>
        private List<byte> ReadGroupBytes(byte slaveId, ModbusGroupAddress group)
        {
            List<byte> pdu = new List<byte>()//拼装PDU
            {
                slaveId,
                (byte)group.FuncCode,
                (byte)(group.StartAddress / 256), (byte)(group.StartAddress % 256),
                (byte)(group.Count / 256),        (byte)(group.Count % 256)
            };
            int respDataLen = (group.FuncCode == FC_ReadCoils || group.FuncCode == FC_ReadDiscreteInputs)//要读字节数
                ? (group.Count + 7) / 8 : group.Count * 2;
            return SendAndReceive(pdu, respDataLen);
        }
        #endregion

        #region 写
        public override Result Write(WriteDataInfo writeDataInfo)
        {
            Result result = new Result { Status = true, Msg = "写成功" };
            try
            {
                var slaveResult = CheckSlaveId();
                if (!slaveResult.Status) { result.Status = false; result.Msg = slaveResult.Msg; return result; }
                byte slaveId = slaveResult.Data;

                bool deviceLittleEndian = GetProp("DeviceEndian") == "Little";
                bool lowWordFirst = GetProp("WordOrder") == "LowFirst";

                // 值个数:优先用前端明确传入的 Count;未传(<=0)才按字节数反推
                int perValueBytes = writeDataInfo.ValueType == typeof(bool) ? 1 : Marshal.SizeOf(writeDataInfo.ValueType);//类型占字节数
                int writeCount = writeDataInfo.Count > 0
                    ? writeDataInfo.Count
                    : Math.Max(1, writeDataInfo.WriteBytes.Length / perValueBytes);

                var addrResult = AnalysisAddress(new VariableProp
                {
                    VarAddr = writeDataInfo.StartAddr,
                    ValueType = writeDataInfo.ValueType,
                    Count = writeCount
                }, isWrite: true);

                if (!addrResult.Status) { result.Status = false; result.Msg = addrResult.Msg; return result; }
                var ma = addrResult.Data;

                // 主机字节 → 设备字节
                byte[] deviceBytes = ConvertEndian(writeDataInfo.WriteBytes, writeDataInfo.ValueType, deviceLittleEndian, lowWordFirst);

                switch (ma.FuncCode)
                {
                    case FC_WriteSingleCoil:
                        WriteSingleCoilInternal(slaveId, (ushort)ma.StartAddress, deviceBytes[0] != 0);
                        break;
                    case FC_WriteSingleRegister:
                        WriteSingleRegInternal(slaveId, (ushort)ma.StartAddress, deviceBytes);
                        break;
                    case FC_WriteMultipleCoils:
                        WriteMultipleCoilsInternal(slaveId, (ushort)ma.StartAddress, deviceBytes);
                        break;
                    case FC_WriteMultipleRegisters:
                        WriteMultipleRegsInternal(slaveId, (ushort)ma.StartAddress, deviceBytes);
                        break;
                    default:
                        throw new Exception($"不支持的功能码: 0x{ma.FuncCode:X2}");
                }
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.Msg = ex.Message;
            }
            return result;
        }

        #region 报文组装
        private void WriteSingleCoilInternal(byte slaveId, ushort startAddr, bool value)
        {
            byte[] v = value ? new byte[] { 0xFF, 0x00 } : new byte[] { 0x00, 0x00 };
            var pdu = new List<byte>
            {
                slaveId,
                FC_WriteSingleCoil,
                (byte)(startAddr / 256), (byte)(startAddr % 256),
                v[0], v[1]
            };
            SendAndReceive(pdu, 0);
        }

        private void WriteSingleRegInternal(byte slaveId, ushort startAddr, byte[] value)
        {
            var pdu = new List<byte>
            {
                slaveId,
                FC_WriteSingleRegister,
                (byte)(startAddr / 256), (byte)(startAddr % 256),
                value[0], value[1]
            };
            SendAndReceive(pdu, 0);
        }

        private void WriteMultipleCoilsInternal(byte slaveId, ushort startAddr, byte[] coilBytes)
        {
            // 入参约定:每字节一个线圈(0/1),内部打包成位(LSB 在前)
            int coilCount = coilBytes.Length;
            byte[] packed = new byte[(coilCount + 7) / 8];
            for (int i = 0; i < coilCount; i++)
                if (coilBytes[i] != 0)
                    packed[i / 8] |= (byte)(1 << (i % 8));

            var pdu = new List<byte>
            {
                slaveId,
                FC_WriteMultipleCoils,
                (byte)(startAddr / 256), (byte)(startAddr % 256),
                (byte)(coilCount / 256), (byte)(coilCount % 256),
                (byte)packed.Length
            };
            pdu.AddRange(packed);
            SendAndReceive(pdu, 0);
        }

        private void WriteMultipleRegsInternal(byte slaveId, ushort startAddr, byte[] regBytes)
        {
            int regCount = regBytes.Length / 2;
            var pdu = new List<byte>
            {
                slaveId,
                FC_WriteMultipleRegisters,
                (byte)(startAddr / 256), (byte)(startAddr % 256),
                (byte)(regCount / 256), (byte)(regCount % 256),
                (byte)regBytes.Length
            };
            pdu.AddRange(regBytes);
            SendAndReceive(pdu, 0);
        }
        #endregion
        #endregion

        #region 分组
        /// <summary>
        ///
        /// </summary>
        /// <param name="varList">通信变量集合</param>
        /// <returns></returns>
        public override ResultData<List<GroupAddress>> GroupAddress(List<VariableProp> varList)
        {
            var result = new ResultData<List<GroupAddress>> { Data = new List<GroupAddress>() };
            try
            {
                var groupList = new List<ModbusGroupAddress>();

                foreach (var varProp in varList)
                {
                    var addrResult = AnalysisAddress(varProp);//解析单个变量的地址
                    if (!addrResult.Status) throw new Exception(addrResult.Msg);

                    var current = addrResult.Data;

                    // 同功能码 + 地址邻近
                    var existing = groupList.FirstOrDefault(g => g.FuncCode == current.FuncCode && IsAdjacent(g, current));

                    if (existing == null)
                    {
                        current.VarPropList.Add(varProp);
                        groupList.Add(current);
                    }
                    else
                    {
                        existing.VarPropList.Add(varProp);
                        MergeRange(existing, current);
                    }
                }

                foreach (var g in groupList) result.Data.Add(g);
                result.Status = true;
                result.Msg = "分组成功";
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.Msg = ex.Message;
            }
            return result;
        }

        //地址邻近判断
        private bool IsAdjacent(ModbusGroupAddress a, ModbusGroupAddress b)
        {
            int aEnd = a.StartAddress + a.Count;
            int bEnd = b.StartAddress + b.Count;
            return b.StartAddress <= aEnd + AdjacentThreshold && a.StartAddress <= bEnd + AdjacentThreshold;
        }

        //合并范围
        private void MergeRange(ModbusGroupAddress target, ModbusGroupAddress other)
        {
            int newStart = Math.Min(target.StartAddress, other.StartAddress);
            int newEnd = Math.Max(target.StartAddress + target.Count, other.StartAddress + other.Count);
            target.StartAddress = newStart;
            target.Count = newEnd - newStart;
        }
        #endregion

        #region 地址分析
        /// <summary>
        ///  解析单个变量的地址配置，生成用于协议通信的组地址对象
        /// </summary>
        /// <param name="variable"></param>
        /// <param name="isWrite"></param>
        /// <returns></returns>
        private ResultData<ModbusGroupAddress> AnalysisAddress(VariableProp variable, bool isWrite = false)
        {
            var result = new ResultData<ModbusGroupAddress>();
            try
            {
                var ma = new ModbusGroupAddress();

                // bool 特例：Marshal.SizeOf(bool) == 4
                int perValueBytes = variable.ValueType == typeof(bool) ? 1 : Marshal.SizeOf(variable.ValueType);

                // 值个数
                int count = variable.Count <= 0 ? 1 : variable.Count;

                // 总字节数 / 总寄存器数
                variable.ReadByteCount = perValueBytes * count;
                ma.Count = Math.Max(perValueBytes / 2, 1) * count;

                string addr = variable.VarAddr;
                if (addr.StartsWith("0"))
                {
                    ma.FuncCode = isWrite ? FC_WriteMultipleCoils : FC_ReadCoils;
                    variable.ReadByteCount = count;
                    ma.Count = count;
                }
                else if (addr.StartsWith("1"))
                {
                    ma.FuncCode = FC_ReadDiscreteInputs;
                    variable.ReadByteCount = count;
                    ma.Count = count;
                }
                else if (addr.StartsWith("3"))
                {
                    ma.FuncCode = FC_ReadInputRegisters;
                }
                else if (addr.StartsWith("4"))
                {
                    ma.FuncCode = isWrite ? FC_WriteMultipleRegisters : FC_ReadHoldingRegisters;
                }
                else
                {
                    throw new Exception($"不支持的地址格式：{addr}");
                }

                ma.StartAddress = int.Parse(addr.Substring(1)) - 1;

                result.Status = true;
                result.Msg = "没有错误";
                result.Data = ma;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.Msg = ex.Message;
            }
            return result;
        }
        #endregion

        #region 数据验证
        private ResultData<byte> CheckSlaveId()
        {
            var result = new ResultData<byte>();
            var prop = PropList?.FirstOrDefault(p => p.PropName == "SlaveId");

            if (prop == null) { result.Status = false; result.Msg = "未配置从站地址"; return result; }
            if (!byte.TryParse(prop.PropValue, out byte slaveId))
            { result.Status = false; result.Msg = "从站地址格式错误"; return result; }
            if (slaveId < 1 || slaveId > 247)
            { result.Status = false; result.Msg = $"从站地址 {slaveId} 超出范围（1~247）"; return result; }

            result.Status = true;
            result.Msg = "没有错误";
            result.Data = slaveId;
            return result;
        }
        #endregion

        #region 转端
        /// <summary>设备字节 ↔ 主机字节（主机固定小端）。按单值分段处理，不整段反转</summary>
        private byte[] ConvertEndian(byte[] deviceBytes, Type valueType, bool deviceLittleEndian, bool lowWordFirst)
        {
            if (deviceBytes == null || deviceBytes.Length == 0)
                return deviceBytes;

            byte[] result = (byte[])deviceBytes.Clone();
            int unit = valueType == typeof(bool) ? 1 : Marshal.SizeOf(valueType);   // 单值字节数

            for (int s = 0; s + unit <= result.Length; s += unit)
            {
                //处理端间（字序） —— 交换两个寄存器的位置
                if (lowWordFirst && unit > 2)
                {
                    int regCount = unit / 2;
                    for (int i = 0; i < regCount / 2; i++)
                    {
                        int a = s + i * 2;
                        int b = s + (regCount - 1 - i) * 2;

                        (result[a], result[b]) = (result[b], result[a]);
                        (result[a + 1], result[b + 1]) = (result[b + 1], result[a + 1]);
                    }
                }

                //处理端内（字节序） —— 翻转单个寄存器内部的字节
                if (!deviceLittleEndian)
                    Array.Reverse(result, s, unit);
            }

            return result;
        }
        #endregion

        #region 通用收发
        // 发 PDU + 收响应 + 校验。respDataLen = 0 表示写操作（响应固定 8 字节）
        private List<byte> SendAndReceive(List<byte> pdu, int respDataLen, int? timeoutOverride = null)
        {
            pdu.AddRange(ComputeCrc(pdu));

            #region 时限、次数检验
            Result conn = TransferObject.Connect(GetPropInt("TryCount", 30));
            if (!conn.Status) throw new Exception(conn.Msg);

            int timeout = GetPropInt("Timeout", 5000);
            #endregion

            #region 从传输层获取字节
            int receiveLen = respDataLen > 0 ? respDataLen + 5 : 8;
            var resp = ((IRequestResponse)TransferObject).SendAndReceived(pdu, receiveLen, 5, timeout);
            #endregion

            #region 报文检验
            if (!resp.Status) throw new Exception(resp.Msg);
            if (resp.Data.Count < 5) throw new Exception("响应报文长度不足");

            // CRC 校验
            var checkBytes = resp.Data.GetRange(0, resp.Data.Count - 2);
            checkBytes.AddRange(ComputeCrc(checkBytes));
            if (!checkBytes.SequenceEqual(resp.Data)) throw new Exception("CRC 校验失败");

            // 异常响应
            if (resp.Data[1] > 0x80)
            {
                byte errCode = resp.Data[2];
                string msg = Errors.ContainsKey(errCode) ? Errors[errCode] : $"未知异常 0x{errCode:X2}";
                throw new Exception($"Modbus 异常：{msg}");
            }
            #endregion

            return respDataLen > 0 ? resp.Data.GetRange(3, respDataLen) : new List<byte>();
        }
        #endregion

        #region CRC
        private static byte[] ComputeCrc(List<byte> data)
        {
            ushort crc = 0xFFFF;//CRC计算的初始值
            for (int i = 0; i < data.Count; i++)
            {
                crc ^= data[i];
                for (int j = 0; j < 8; j++)
                    crc = (crc & 1) != 0 ? (ushort)((crc >> 1) ^ 0xA001) : (ushort)(crc >> 1);
            }
            return new byte[] { (byte)(crc & 0xFF), (byte)((crc >> 8) & 0xFF) };
        }
        #endregion

        #region 辅助
        private string GetProp(string name)
            => PropList?.FirstOrDefault(p => p.PropName == name)?.PropValue;

        private int GetPropInt(string name, int defaultValue)
        {
            var s = GetProp(name);
            return int.TryParse(s, out int v) ? v : defaultValue;
        }

        public override void DisConnect() => TransferObject?.DisConnect();
        #endregion

        #region 常量 / 错误码
        private const byte FC_ReadCoils = 0x01;
        private const byte FC_ReadDiscreteInputs = 0x02;
        private const byte FC_ReadHoldingRegisters = 0x03;
        private const byte FC_ReadInputRegisters = 0x04;
        private const byte FC_WriteSingleCoil = 0x05;
        private const byte FC_WriteSingleRegister = 0x06;
        private const byte FC_WriteMultipleCoils = 0x0F;
        private const byte FC_WriteMultipleRegisters = 0x10;

        /// <summary>分组允许的地址间隔阈值</summary>
        private const int AdjacentThreshold = 8;

        private static readonly Dictionary<byte, string> Errors = new Dictionary<byte, string>
        {
            { 0x01, "非法功能码" },
            { 0x02, "非法数据地址" },
            { 0x03, "非法数据值" },
            { 0x04, "从站设备故障" },
            { 0x05, "确认，从站需要一个耗时操作" },
            { 0x06, "从站忙" },
        };
        #endregion
    }
}
