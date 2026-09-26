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
    /// 趋势预警线段表
    /// </summary>
    [Table("TrendSections")]
    public class TrendSectionEntity
    {
        [Key]
        public int SectionId { get; set; }

        /// <summary>
        /// 纵轴编号
        /// </summary>
        public string AxisNum { get; set; }

        /// <summary>
        /// 预警值
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// 预警线颜色
        /// </summary>
        public string Color { get; set; } = "Red";
    }
}
