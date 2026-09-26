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
    /// 设备属性表
    /// </summary>
    [Table("DeviceProps")]
    public class DevicePropEntity
    {
        /// <summary>
        /// 设备属性ID
        /// </summary>
        [Key]
        public int DevicePropId { get; set; }

        /// <summary>
        /// 设备编号
        /// </summary>
        public string DeviceNum { get; set; }

        /// <summary>
        /// 属性名称
        /// </summary>
        public string PropName { get; set; }

        /// <summary>
        /// 属性值
        /// </summary>
        public string PropValue { get; set; }
    }
}
