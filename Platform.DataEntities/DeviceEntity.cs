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
    /// 设备表
    /// </summary>
    [Table("Devices")]
    public class DeviceEntity
    {
        /// <summary>
        /// 设备编号
        /// </summary>
        [Key]
        public string DeviceNum { get; set; }

        /// <summary>
        /// 所属组件Id
        /// </summary>
        public int ComponentId { get; set; }

        //[ForeignKey("ComponentId")]
        //public ComponentEntity? ComponentEntity { get; set; }

        /// <summary>
        /// 设备名称(空压机、冷冻式干燥机等)
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// 设备组件类型名称(AirCompressor等，与程序中控件名一致)
        /// </summary>
        public string ComponentType { get; set; }

        /// <summary>
        /// 坐标X
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// 坐标Y
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// 坐标Z （zindex）
        /// </summary>
        public double Z { get; set; }

        /// <summary>
        /// 宽
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// 高
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// 流动方向(左右、上下)
        /// </summary>
        public int FlowDirection { get; set; } = 0;

        /// <summary>
        /// 旋转角度
        /// </summary>
        public int Rotate { get; set; } = 0;
    }
}
