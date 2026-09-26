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
    /// 监控数据表
    /// </summary>
    [Table("MonitorRecords")]
    public class MonitorRecordEntity
    {
        [Key]
        public int RecordId { get; set; }

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
        /// 监控记录值(只有数据类型的才记录)
        /// </summary>
        public decimal RecordValue { get; set; }

        /// <summary>
        /// 报警编号，没有报警不记
        /// </summary>
        public string? AlarmNum { get; set; }

        /// <summary>
        /// 记录时间
        /// </summary>
        public DateTime RecordTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 记录人账号
        /// </summary>
        public string Account { get; set; }
    }
}
