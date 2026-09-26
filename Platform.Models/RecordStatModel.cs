using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Models
{
    /// <summary>
    /// 监控记录统计模型
    /// </summary>
    public class RecordStatModel
    {
        /// <summary>
        /// 没有意义 不想显示红色
        /// </summary>
        public int State => 1;

        /// <summary>
        /// 设备编号
        /// </summary>
        public string DeviceNum { get; set; }

        /// <summary>
        /// 设备名称
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// 变量编号
        /// </summary>
        public string VarNum { get; set; }

        /// <summary>
        /// 变量名称
        /// </summary>
        public string VarName { get; set; }

        /// <summary>
        /// 平均值
        /// </summary>
        public string AvgValue { get; set; }// 平均值

        /// <summary>
        /// 最大值
        /// </summary>
        public decimal MaxValue { get; set; }// 记录中的最大值

        /// <summary>
        /// 最小值
        /// </summary>
        public decimal MinValue { get; set; }// 记录中的最小值

        /// <summary>
        /// 报警次数
        /// </summary>
        public int AlarmCount { get; set; }// 报警触发次数

        /// <summary>
        /// 最新记录时间
        /// </summary>
        public string LastTime { get; set; }// 最新记录时间

        /// <summary>
        /// 总记录数
        /// </summary>
        public int RecordCount { get; set; }// 总记录数
    }
}
