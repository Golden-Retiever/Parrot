using Platform.DataEntities;
using Platform.DeviceAccess.Base;

namespace Platform.DeviceAccess.Transfer
{
    /// <summary>
    /// 传输对象。和PLC发送/接收报文
    /// </summary>
    public abstract class TransferObject
    {
        //匹配条件
        //["COM1"]
        //["127.0.0.1","502"]
        internal List<string> Condition = new List<string>();

        /// <summary>
        /// 连接
        /// </summary>
        /// <param name="trycount">重试次数</param>
        /// <returns></returns>
        internal virtual Result Connect(int trycount = 30)
        {
            return new Result();
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        internal virtual void DisConnect() { }

        /// <summary>
        /// 属性配置
        /// </summary>
        /// <param name="props">设备属性</param>
        /// <returns></returns>
        internal virtual Result Config(List<DevicePropEntity> props)
        {
            return new Result();
        }
    }
}
