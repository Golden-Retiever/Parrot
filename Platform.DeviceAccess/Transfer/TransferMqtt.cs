using Platform.DataEntities;
using Platform.DeviceAccess.Base;
using MQTTnet;
using MQTTnet.Protocol;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Platform.DeviceAccess.Transfer
{
    /// <summary>
    /// MQTT 传输对象。订阅式：连 Broker、订阅主题、收推送、发消息。
    /// </summary>
    internal class TransferMqtt : TransferObject, IPushSubscribe
    {
        private readonly object _lock = new object();
        private IMqttClient _client;

        private string _broker = "broker.emqx.io";
        private int _port = 1883;
        private string _clientId = "";
        private string _username = "";
        private string _password = "";

        /// <summary>主题 -> 最新载荷缓存</summary>
        private readonly Dictionary<string, string> _cache = new Dictionary<string, string>();

        /// <summary>已订阅主题（幂等去重）</summary>
        private readonly HashSet<string> _subscribed = new HashSet<string>();

        public event Action<string, string> MessageReceived;

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
                        case "IP": _broker = item.PropValue.Trim(); break;
                        case "Port": _port = int.Parse(item.PropValue.Trim()); break;
                        case "ClientId": _clientId = item.PropValue.Trim(); break;
                        case "Username": _username = item.PropValue.Trim(); break;
                        case "Password": _password = item.PropValue.Trim(); break;
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
            lock (_lock)
            {
                Result result = new Result { Status = true, Msg = "连接成功" };
                try
                {
                    if (_client != null && _client.IsConnected) return result;

                    _client = new MqttClientFactory().CreateMqttClient();
                    _client.ApplicationMessageReceivedAsync += OnMessageReceived;

                    var builder = new MqttClientOptionsBuilder()
                        .WithTcpServer(_broker, _port)
                        .WithClientId(string.IsNullOrEmpty(_clientId) ? Guid.NewGuid().ToString("N") : _clientId);

                    if (!string.IsNullOrEmpty(_username))
                        builder.WithCredentials(_username, _password);

                    _client.ConnectAsync(builder.Build(), CancellationToken.None).GetAwaiter().GetResult();
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
            lock (_lock)
            {
                try { _client?.DisconnectAsync().GetAwaiter().GetResult(); } catch { }
                finally
                {
                    _client?.Dispose();
                    _client = null;
                    _subscribed.Clear();
                }
            }
        }
        #endregion

        #region 订阅
        public Result Subscribe(string topic)
        {
            Result result = new Result { Status = true, Msg = "订阅成功" };
            lock (_lock)
            {
                try
                {
                    if (_client == null || !_client.IsConnected)
                    {
                        result.Status = false;
                        result.Msg = "未连接 Broker";
                        return result;
                    }
                    if (_subscribed.Contains(topic)) return result;

                    var sub = new MqttClientSubscribeOptionsBuilder()
                        .WithTopicFilter(topic, MqttQualityOfServiceLevel.AtMostOnce)
                        .Build();
                    _client.SubscribeAsync(sub, CancellationToken.None).GetAwaiter().GetResult();
                    _subscribed.Add(topic);
                }
                catch (Exception ex)
                {
                    result.Status = false;
                    result.Msg = ex.Message;
                }
            }
            return result;
        }
        #endregion

        #region 发布
        public Result Publish(string topic, string payload)
        {
            Result result = new Result();
            lock (_lock)
            {
                try
                {
                    if (_client == null || !_client.IsConnected)
                    {
                        result.Status = false;
                        result.Msg = "未连接 Broker";
                        return result;
                    }
                    var msg = new MqttApplicationMessageBuilder()
                        .WithTopic(topic)
                        .WithPayload(Encoding.UTF8.GetBytes(payload ?? ""))
                        .Build();
                    _client.PublishAsync(msg, CancellationToken.None).GetAwaiter().GetResult();
                    result.Status = true;
                    result.Msg = "发布成功";
                }
                catch (Exception ex)
                {
                    result.Status = false;
                    result.Msg = ex.Message;
                }
            }
            return result;
        }
        #endregion

        #region 读缓存
        public bool TryGetCached(string topic, out string payload)
        {
            lock (_lock)
                return _cache.TryGetValue(topic, out payload);
        }
        #endregion

        #region 收到消息回调
        private Task OnMessageReceived(MqttApplicationMessageReceivedEventArgs e)
        {
            string topic = e.ApplicationMessage.Topic;
            string payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload.ToArray());

            lock (_lock)
                _cache[topic] = payload;

            MessageReceived?.Invoke(topic, payload);
            return Task.CompletedTask;
        }
        #endregion
    }
}
