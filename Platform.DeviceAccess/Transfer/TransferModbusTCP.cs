using Platform.DataEntities;
using Platform.DeviceAccess.Base;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Platform.DeviceAccess.Transfer
{
    /// <summary>
    /// TCP 传输对象
    /// </summary>
    internal class TransferModbusTCP : TransferObject, IRequestResponse
    {
        private static readonly object transLock = new object();
        private TcpClient tcpClient;
        private NetworkStream stream;

        private string ipAddress = "127.0.0.1";
        private int port = 502;
        private int cfgTimeout = 500;

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
                    switch (item.PropName.Trim())
                    {
                        case "IP": ipAddress = item.PropValue.Trim(); break;
                        case "Port": port = int.Parse(item.PropValue.Trim()); break;
                        case "Timeout": cfgTimeout = int.Parse(item.PropValue.Trim()); break;
                    }
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
                Result result = new Result { Status = true, Msg = "连接成功" };
                try
                {
                    if (tcpClient != null && tcpClient.Connected) { ConnectState = true; return result; }

                    int count = 0;
                    while (count < trycount)
                    {
                        try
                        {
                            tcpClient = new TcpClient();
                            tcpClient.Connect(ipAddress, port);
                            stream = tcpClient.GetStream();
                            break;
                        }
                        catch (SocketException)
                        {
                            count++;
                            System.Threading.Thread.Sleep(100);
                        }
                    }
                    if (tcpClient == null || !tcpClient.Connected) throw new Exception("TCP 连接失败");
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
            //加锁:与Connect/SendAndReceived同锁,避免连接在别的线程Receive时被关闭引发error
            lock (transLock)
            {
                try { stream?.Close(); tcpClient?.Close(); } catch { }
                stream = null;
                tcpClient = null;
                ConnectState = false;
            }
        }
        #endregion

        #region 收发
        /// <summary>
        /// 发送报文 + 接收完整响应。TCP 按 MBAP 头的长度字段收帧(前 6 字节含长度),len1/len2 不用
        /// </summary>
        public ResultData<List<byte>> SendAndReceived(List<byte> req, int len1 = 0, int len2 = 0, int timeout = 5000, Func<byte[], int> calcLen = null)
        {
            lock (transLock)
            {
                ResultData<List<byte>> result = new ResultData<List<byte>>();
                List<byte> respBytes = new List<byte>();

                try
                {
                    stream.ReadTimeout = cfgTimeout > 0 ? cfgTimeout : timeout;
                    stream.WriteTimeout = cfgTimeout > 0 ? cfgTimeout : timeout;

                    // 发送
                    stream.Write(req.ToArray(), 0, req.Count);

                    // 先读 MBAP 前 6 字节(事务2 + 协议2 + 长度2)
                    byte[] header = ReadExact(6);

                    // 长度字段 = 单元标识(1) + PDU,总帧长 = 6 + 长度
                    int totalLen = 6 + (header[4] << 8 | header[5]);

                    respBytes.AddRange(header);
                    respBytes.AddRange(ReadExact(totalLen - 6));

                    result.Status = true;
                    result.Msg = "读取成功";
                }
                catch (System.IO.IOException)
                {
                    // 断线/超时都标记断开,下次 Connect 重连
                    ConnectState = false;
                    result.Status = false;
                    result.Msg = "接收报文超时或连接中断";
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

        /// <summary>网络流是流式的,循环读满 n 字节</summary>
        private byte[] ReadExact(int n)
        {
            byte[] buf = new byte[n];
            int got = 0;
            while (got < n)
            {
                int r = stream.Read(buf, got, n - got);
                if (r <= 0) throw new System.IO.IOException("连接已断开");
                got += r;
            }
            return buf;
        }
        #endregion
    }
}
