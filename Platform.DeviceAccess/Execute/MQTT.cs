using Platform.DataEntities;
using Platform.DeviceAccess.Base;
using Platform.DeviceAccess.Transfer;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;

namespace Platform.DeviceAccess.Execute
{
    /// <summary>MQTT 执行对象。订阅式：主题即地址，读=取缓存，写=发布。</summary>
    internal class MQTT : ExecuteObject
    {
        /// <summary>订阅入口。MQTT 是订阅式协议，匹配到的传输对象必然实现了 IPushSubscribe</summary>
        private IPushSubscribe Push => (IPushSubscribe)TransferObject;

        #region 匹配
        internal override Result Match(List<DevicePropEntity> devicePropList, List<TransferObject> transferObjList)
        {
            PropList = devicePropList;
            Result result = new Result();

            var ps = new List<string> { GetProp("IP"), GetProp("Port") };

            var transferObject = transferObjList.FirstOrDefault(t => t.GetType().Name == "TransferMqtt" && ps.All(p => t.Condition.Any(c => c == p)));
            if (transferObject == null)
            {
                TransferObject = new TransferMqtt();
                TransferObject.Condition = ps;
                TransferObject.Config(PropList);
                transferObjList.Add(TransferObject);
            }
            else
            {
                TransferObject = transferObject;
            }

            result.Msg = "没有错误";
            result.Status = true;
            return result;
        }
        #endregion

        #region 分组
        /// <summary>MQTT 分组：同一主题的变量并为一组（主题即变量地址）</summary>
        public override ResultData<List<GroupAddress>> GroupAddress(List<VariableProp> varList)
        {
            var result = new ResultData<List<GroupAddress>> { Data = new List<GroupAddress>() };
            try
            {
                var groups = new List<MqttGroupAddress>();
                foreach (var varProp in varList)
                {
                    string topic = varProp.VarAddr;
                    var existing = groups.FirstOrDefault(g => g.Topic == topic);
                    if (existing == null)
                    {
                        existing = new MqttGroupAddress { Topic = topic };
                        groups.Add(existing);
                    }
                    existing.VarPropList.Add(varProp);
                }
                foreach (var g in groups) result.Data.Add(g);
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
        #endregion

        #region 读
        public override Result Read(List<GroupAddress> groupAddrList)
        {
            Result result = new Result { Status = true, Msg = "读数据成功" };
            try
            {
                // 确保连接
                Result conn = TransferObject.Connect();
                if (!conn.Status) { result.Status = false; result.Msg = conn.Msg; return result; }

                foreach (var ga in groupAddrList)
                {
                    if (ga is not MqttGroupAddress group) continue;

                    // 订阅主题（幂等）
                    Result sub = Push.Subscribe(group.Topic);
                    if (!sub.Status) { result.Status = false; result.Msg = sub.Msg; return result; }

                    // 读缓存并解析
                    if (Push.TryGetCached(group.Topic, out string payload))
                    {
                        foreach (var varProp in group.VarPropList)
                            varProp.ReadValue = ParsePayload(payload, varProp.ValueType, varProp.Count);
                    }
                }
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.Msg = ex.Message;
            }
            return result;
        }
        #endregion

        #region 写
        public override Result Write(WriteDataInfo writeDataInfo)
        {
            Result result = new Result();
            try
            {
                string topic = writeDataInfo.StartAddr;
                string payload = BytesToPayload(writeDataInfo.WriteBytes, writeDataInfo.ValueType, writeDataInfo.Count);
                result = Push.Publish(topic, payload);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.Msg = ex.Message;
            }
            return result;
        }
        #endregion

        #region 载荷解析
        /// <summary>payload 字符串 → 值。count≤1 返回单值，count>1 返回 object[]</summary>
        private object ParsePayload(string payload, Type type, int count)
        {
            if (string.IsNullOrEmpty(payload)) return null;
            string[] parts = payload.Split(new[] { ',', '，', ' ', ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (count <= 1)
                return ConvertOne(parts.Length > 0 ? parts[0] : payload, type);

            var arr = new object[Math.Min(count, parts.Length)];
            for (int i = 0; i < arr.Length; i++)
                arr[i] = ConvertOne(parts[i], type);
            return arr;
        }

        private object ConvertOne(string s, Type type)
        {
            if (type == typeof(bool)) return s == "1" || s.Equals("true", StringComparison.OrdinalIgnoreCase);
            if (type == typeof(short)) return short.Parse(s, CultureInfo.InvariantCulture);
            if (type == typeof(ushort)) return ushort.Parse(s, CultureInfo.InvariantCulture);
            if (type == typeof(int)) return int.Parse(s, CultureInfo.InvariantCulture);
            if (type == typeof(uint)) return uint.Parse(s, CultureInfo.InvariantCulture);
            if (type == typeof(long)) return long.Parse(s, CultureInfo.InvariantCulture);
            if (type == typeof(ulong)) return ulong.Parse(s, CultureInfo.InvariantCulture);
            if (type == typeof(float)) return float.Parse(s, CultureInfo.InvariantCulture);
            if (type == typeof(double)) return double.Parse(s, CultureInfo.InvariantCulture);
            if (type == typeof(string)) return s;
            return s;
        }

        /// <summary>写入字节 → 逗号分隔字符串 payload（数值用不变文化格式）</summary>
        private string BytesToPayload(byte[] bytes, Type type, int count)
        {
            int perValue = type == typeof(bool) ? 1 : Marshal.SizeOf(type);
            int n = count <= 0 ? Math.Max(1, bytes.Length / perValue) : count;
            var values = new List<string>();
            for (int i = 0; i < n; i++)
            {
                int offset = i * perValue;
                if (offset + perValue > bytes.Length) break;
                object val = ByteConvert.ToValue(bytes.Skip(offset).Take(perValue).ToArray(), type);
                values.Add(Convert.ToString(val, CultureInfo.InvariantCulture));
            }
            return values.Count == 1 ? values[0] : string.Join(",", values);
        }
        #endregion

        #region 辅助
        private string GetProp(string name)
            => PropList?.FirstOrDefault(p => p.PropName == name)?.PropValue;

        public override void DisConnect() => TransferObject?.DisConnect();
        #endregion
    }
}
