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
    /// 趋势图序表
    /// </summary>
    [Table("TrendSerieses")]
    public class TrendSeriesEntity
    {
        /// <summary>
        /// 显示Id
        /// </summary>
        [Key]
        public int SeriesId { get; set; }

        /// <summary>
        /// 设备编号
        /// </summary>
        public string DeviceNum { get; set; }


        /// <summary>
        /// 变量编号
        /// </summary>
        public string VarNum { get; set; }


        /// <summary>
        /// 趋势编号
        /// </summary>
        public string TrendNum { get; set; }

        /// <summary>
        /// 纵轴编号
        /// </summary>
        public string ANum { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 颜色
        /// </summary>
        public string Color { get; set; }
    }
}
