namespace Platform.DeviceAccess.Base
{
    /// <summary>
    /// 分组后的 MQTT 地址（一个主题一组）
    /// </summary>
    internal class MqttGroupAddress : GroupAddress
    {
        /// <summary>
        /// 主题（变量地址即主题）
        /// </summary>
        public string Topic { get; set; }
    }
}
