using Platform.DeviceAccess.Base;
using System;

namespace Platform.DeviceAccess.Transfer
{
    /// <summary>
    /// 订阅式（发布-订阅）传输能力。MQTT 等数据方主动推、订阅方被动收的协议实现此接口。
    /// </summary>
    public interface IPushSubscribe
    {
        /// <summary>
        /// 订阅主题
        /// </summary>
        Result Subscribe(string topic);

        /// <summary>
        /// 向主题发布一条消息（写命令）
        /// </summary>
        Result Publish(string topic, string payload);

        /// <summary>
        /// 收到订阅消息
        /// </summary>
        event Action<string, string> MessageReceived;

        /// <summary>
        /// 读取某主题的最新缓存载荷
        /// </summary>
        bool TryGetCached(string topic, out string payload);
    }
}
