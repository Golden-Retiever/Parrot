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
        /// 是否裸帧(勾选=地址格填完整请求帧文本原样发送,写入数据列不参与)
        /// </summary>
        public bool IsRawFrame { get; set; }

        /// <summary>
        /// 手动控制值(勾帧行不参与发送,可为null)
        /// </summary>
        public string? ControlValue { get; set; }

        /// <summary>
        /// 数据类型(默认Int16)
        /// </summary>
        public string ValueType { get; set; } = "Int16";

        /// <summary>
        /// 是否循环执行(勾选后每2秒重复写,取消勾选停止)
        /// </summary>
        public bool IsLoop { get; set; }

        /// <summary>
        /// 单写延时(毫秒):点击后先等再写,0=立即写
        /// </summary>
        public string ControlDelay { get; set; } = "0";
    }
}
