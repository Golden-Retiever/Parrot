using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.DataEntities
{
    /// <summary>
    /// 报警消息表
    /// </summary>
    [Table("DeviceAlarms")]
    public class DeviceAlarmEntity
    {
        [Key]
        public string AlarmNum { get; set; }// 报警消息编号
        public string? ConfNum { get; set; }// 报警配置编号
        public string DeviceNum { get; set; }// 报警设备编号

        /// <summary>
        /// 报警设备名称
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// 报警变量编号
        /// </summary>
        public string? VarNum { get; set; }// 变量编号

        /// <summary>
        /// 报警变量名称
        /// </summary>
        public string? VarName { get; set; }// 变量名称

        /// <summary>
        /// 报警值
        /// </summary>
        public string? AlarmValue { get; set; }

        /// <summary>
        /// 报警信息
        /// </summary>
        public string AlarmContent { get; set; }

        /// <summary>
        /// 报警记录时间
        /// </summary>
        public DateTime RecordTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 报警记录账号
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// 解决时间
        /// </summary>
        public DateTime? SolveTime { get; set; }

        /// <summary>
        /// 处理状态。0-未解决，1-已解决
        /// </summary>
        public int State { get; set; } = 0;
    }
}
