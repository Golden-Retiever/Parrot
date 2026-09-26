using Platform.DataEntities;
using Platform.DeviceAccess.Base;
using Platform.DeviceAccess.Execute;
using Platform.DeviceAccess.Transfer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Platform.DeviceAccess
{
    /// <summary>通信类（单例），提供通信执行对象</summary>
    public class Communication
    {
        #region 单例
        private static readonly Communication _instance = new Communication();

        /// <summary>获取单例</summary>
        public static Communication CreateInstance() => _instance;
        #endregion

        #region 传输对象池
        //传输对象池（多个设备可共享同一个串口/同一个TCP连接）
        private readonly List<TransferObject> TransferObjects = new List<TransferObject>();
        #endregion

        #region 获取执行对象
        /// <summary>
        /// 根据设备属性获取通信执行对象
        /// </summary>
        /// <param name="devicePropList">设备属性配置</param>
        public ResultData<ExecuteObject> GetExecuteObject(List<DevicePropEntity> devicePropList)
        {
            var result = new ResultData<ExecuteObject>();

            // 校验协议配置
            var protocol = devicePropList?.FirstOrDefault(p => p.PropName == "Protocol");
            if (protocol == null)
            {
                result.Status = false;
                result.Msg = "没有配置协议";
                return result;
            }

            // 反射找到执行对象类型
            string typeName = $"Platform.DeviceAccess.Execute.{protocol.PropValue}";
            Type type = typeof(Communication).Assembly.GetType(typeName);
            if (type == null)
            {
                result.Status = false;
                result.Msg = $"没有找到执行对象：{typeName}";
                return result;
            }

            // 创建实例
            ExecuteObject eo = Activator.CreateInstance(type) as ExecuteObject;
            if (eo == null)
            {
                result.Status = false;
                result.Msg = $"{protocol.PropValue} 不是 ExecuteObject";
                return result;
            }

            // 匹配传输对象
            Result matchResult = eo.Match(devicePropList, TransferObjects);//执行层
            if (!matchResult.Status)
            {
                result.Status = false;
                result.Msg = matchResult.Msg;
                return result;
            }

            result.Status = true;
            result.Msg = "没有错误";
            result.Data = eo;
            return result;
        }
        #endregion
    }
}
