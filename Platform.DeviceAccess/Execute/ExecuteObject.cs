using Platform.DataEntities;
using Platform.DeviceAccess.Base;
using Platform.DeviceAccess.Transfer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.DeviceAccess.Execute
{
    /// <summary>
    /// 通信执行对象。实现连接、读、写等
    /// </summary>
    public abstract class ExecuteObject
    {
        /// <summary>
        /// 该执行对象使用的传输对象
        /// </summary>
        public TransferObject TransferObject { get; set; }

        /// <summary>
        /// 设备属性
        /// </summary>
        internal List<DevicePropEntity> PropList { get; set; }

        /// <summary>
        /// 匹配
        /// </summary>
        /// <param name="devicePropList">通信配置属性</param>
        /// <param name="transferObjList">匹配结果。匹配到的传输对象集合</param>
        internal virtual Result Match(List<DevicePropEntity> devicePropList, List<TransferObject> transferObjList)
        {
            return new Result();
        }

        /// <summary>
        /// 读
        /// </summary>
        /// <param name="groupAddrList">分组后的地址</param>
        /// <returns></returns>
        public virtual Result Read(List<GroupAddress> groupAddrList)
        {
            return new Result();
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public virtual void DisConnect() { }

        /// <summary>
        /// 分组
        /// </summary>
        /// <param name="varList">变量集合</param>
        /// <returns>分组后的地址</returns>
        public virtual ResultData<List<GroupAddress>> GroupAddress(List<VariableProp> varList)
        {
            return new ResultData<List<GroupAddress>>();
        }

        /// <summary>
        /// 写
        /// </summary>
        public virtual Result Write(WriteDataInfo writeDataInfo)
        {
            return new Result();
        }
    }
}
