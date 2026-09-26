using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.DataEntities
{
    /// <summary>
    /// 监控配置编号枚举
    /// </summary>
    public enum MonitorSettingNumEnum
    {
        /// <summary>
        /// 温度
        /// </summary>
        TemperatureVar,

        /// <summary>
        /// 湿度
        /// </summary>
        HumidityVar,

        /// <summary>
        /// PM2.5
        /// </summary>
        PM25Var,

        /// <summary>
        /// 压力
        /// </summary>
        PressureVar,

        /// <summary>
        /// 瞬时流量
        /// </summary>
        FlowRateVar
    }
}
