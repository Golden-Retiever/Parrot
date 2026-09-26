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
    /// 设备提醒表
    /// </summary>
    [Table("DeviceWarnings")]
    public class DeviceWarningEntity
    {
        [Key]
        public int WarningId { get; set; }
        public string Message { get; set; }
        public DateTime DateTime { get; set; }
    }
}
