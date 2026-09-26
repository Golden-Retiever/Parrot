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
    /// 手动控制表
    /// </summary>
    [Table("ManualControls")]
    public class ManualControlEntity
    {
        /// <summary>
        /// 手动控制ID
        /// </summary>
        [Key]
        public int ControlId { get; set; }

        /// <summary>
        /// 设备编号
        /// </summary>
        public string DeviceNum { get; set; }


        /// <summary>
        /// 手动控制名称
        /// </summary>
        public string ControlName { get; set; }

        /// <summary>
        /// 手动控制地址
        /// </summary>
        public string ControlAddress { get; set; }

        /// <summary>
        /// 手动控制值
        /// </summary>
        public string ControlValue { get; set; }
    }
}
