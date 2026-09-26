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
    /// 纵轴表
    /// </summary>
    [Table("TrendAxises")]
    public class TrendAxisEntity
    {
        /// <summary>
        /// 纵轴编号
        /// </summary>
        [Key]
        public string AxisNum { get; set; }

        /// <summary>
        /// 纵轴所在趋势编号
        /// </summary>
        public string TrendNum { get; set; }

        /// <summary>
        /// 纵轴标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 是否显示标题
        /// </summary>
        public bool IsShowTitle { get; set; }

        /// <summary>
        /// 最小值
        /// </summary>
        public double Minimum { get; set; }

        /// <summary>
        /// 最大值
        /// </summary>
        public double Maximum { get; set; } = 100;

        /// <summary>
        /// 是否显示分割线
        /// </summary>
        public bool IsShowSeperator { get; set; }

        /// <summary>
        /// 标签格式
        /// </summary>
        public string LabelFormater { get; set; } = "00";

        /// <summary>
        /// 位置(左/右)
        /// </summary>
        public string Position { get; set; }
    }
}
