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
    /// 监控配置表
    /// </summary>
    [Table("MonitorSettings")]
    public class MonitorSettingEntity
    {
        /// <summary>
        /// 配置编号
        /// </summary>
        [Key]
        public MonitorSettingNumEnum SettingNum { get; set; }

        /// <summary>
        /// 配套标题/头
        /// </summary>
        public string Header { get; set; }

        /// <summary>
        /// 配置描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 设备编号
        /// </summary>
        public string DeviceNum { get; set; }

        /// <summary>
        /// 变量编号
        /// </summary>
        public string VarNum { get; set; }
    }
}
