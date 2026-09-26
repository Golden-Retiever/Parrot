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
    /// 趋势表
    /// </summary>
    [Table("Trends")]
    public class TrendEntity
    {
        /// <summary>
        /// 趋势编号
        /// </summary>
        [Key]
        public string TrendNum { get; set; }

        /// <summary>
        /// 趋势名称
        /// </summary>
        public string TrendName { get; set; }

        /// <summary>
        /// 是否显示图例
        /// </summary>
        public bool IsShowLegend { get; set; }
    }
}
