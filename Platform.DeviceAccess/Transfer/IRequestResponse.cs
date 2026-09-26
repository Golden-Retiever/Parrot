using Platform.DeviceAccess.Base;
using System;
using System.Collections.Generic;

namespace Platform.DeviceAccess.Transfer
{
    /// <summary>
    /// 问答式（请求-应答）传输能力。Modbus、S7 等主动问、被动答的协议实现此接口。
    /// </summary>
    internal interface IRequestResponse
    {
        /// <summary>
        /// 发送一帧并同步等待一帧响应
        /// </summary>
        /// <param name="req">发送报文</param>
        /// <param name="len1">正常响应总字节数（串口用；TCP 按 MBAP 长度字段收帧，可传 0）</param>
        /// <param name="len2">异常响应总字节数（串口用）</param>
        /// <param name="timeout">超时时间（毫秒）</param>
        /// <param name="calcLen">预留委托参数</param>
        ResultData<List<byte>> SendAndReceived(List<byte> req, int len1 = 0, int len2 = 0, int timeout = 5000, Func<byte[], int> calcLen = null);
    }
}
