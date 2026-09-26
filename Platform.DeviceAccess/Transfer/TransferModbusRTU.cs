using Platform.DataEntities;
using Platform.DeviceAccess.Base;
using System.IO.Ports;
using System.Reflection;

namespace Platform.DeviceAccess.Transfer
{
    /// <summary>
    /// 串口传输对象
    /// </summary>
    internal class TransferModbusRTU : TransferObject, IRequestResponse
    {
        private static readonly object transLock = new object();
        private SerialPort serialPort;

        public TransferModbusRTU()
        {
            serialPort = new SerialPort();
        }

        /// <summary>是否已连接</summary>
        internal bool ConnectState { get; set; } = false;

        #region 配置
        internal override Result Config(List<DevicePropEntity> props)
        {
            Result result = new Result();
            try
            {
                foreach (var item in props)
                {
                    PropertyInfo pi = serialPort.GetType().GetProperty(item.PropName.Trim(), BindingFlags.Public | BindingFlags.Instance);
                    if (pi == null) continue;

                    Type propType = pi.PropertyType;
                    object v = propType.IsEnum ? Enum.Parse(propType, item.PropValue.Trim()) : Convert.ChangeType(item.PropValue.Trim(), propType);

                    pi.SetValue(serialPort, v);
                }
                result.Status = true;
                result.Msg = "配置成功";
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.Msg = ex.Message;
            }
            return result;
        }
        #endregion

        #region 连接
        internal override Result Connect(int trycount = 30)
        {
            lock (transLock)
            {
                Result result = new Result { Status = true, Msg = "打开成功" };
                try
                {
                    if (serialPort.IsOpen) { ConnectState = true; return result; }

                    int count = 0;
                    while (count < trycount)
                    {
                        try
                        {
                            serialPort.Open();
                            break;
                        }
                        catch (System.IO.IOException)
                        {
                            System.Threading.Thread.Sleep(1);
                            count++;
                        }
                    }
                    if (!serialPort.IsOpen) throw new Exception("串口打开失败");
                    ConnectState = true;
                }
                catch (Exception ex)
                {
                    result.Status = false;
                    result.Msg = ex.Message;
                }
                return result;
            }
        }
        #endregion

        #region 断开
        internal override void DisConnect()
        {
            //加锁:与Connect/SendAndReceived同锁,避免串口在别的线程ReadByte时被关闭引发error
            lock (transLock)
            {
                if (serialPort.IsOpen) serialPort.Close();
                ConnectState = false;
            }
        }
        #endregion

        #region 收发
        /// <summary>
        /// 发送 PDU + 接收完整响应报文
        /// </summary>
        /// <param name="req">请求报文</param>
        /// <param name="receiveLen">正常响应总字节数</param>
        /// <param name="errorLen">异常响应总字节数（Modbus RTU 固定 5）</param>
        /// <param name="timeout">超时（毫秒）</param>
        /// <param name="calcLen">预留（串口不用）</param>
        public ResultData<List<byte>> SendAndReceived(List<byte> req, int receiveLen, int errorLen, int timeout, Func<byte[], int> calcLen = null)
        {
            lock (transLock)
            {
                ResultData<List<byte>> result = new ResultData<List<byte>>();
                List<byte> respBytes = new List<byte>();

                try
                {
                    // 发送
                    serialPort.Write(req.ToArray(), 0, req.Count);

                    // 先读 2 字节：从机地址 + 功能码
                    respBytes.Add((byte)serialPort.ReadByte());
                    respBytes.Add((byte)serialPort.ReadByte());

                    // 按功能码决定剩余长度
                    // 功能码 > 0x80 表示异常响应（固定 errorLen 字节）
                    // 否则是正常响应（receiveLen 字节）
                    int target = respBytes[1] > 0x80 ? errorLen : receiveLen;
                    while (respBytes.Count < target)
                        respBytes.Add((byte)serialPort.ReadByte());

                    result.Status = true;
                    result.Msg = "读取成功";
                }
                catch (TimeoutException)
                {
                    result.Status = false;
                    result.Msg = "接收报文超时";
                }
                catch (Exception ex)
                {
                    result.Status = false;
                    result.Msg = ex.Message;
                }
                finally
                {
                    result.Data = respBytes;
                }
                return result;
            }
        }
        #endregion
    }
}
