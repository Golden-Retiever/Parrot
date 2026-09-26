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
    /// 七日能耗-用气量
    /// </summary>
    [Table("SevenAirs")]
    public class SevenAirEntity
    {
        [Key]
        public int AirId { get; set; }

        /// <summary>
        /// 日
        /// </summary>
        public string Day { get; set; }

        /// <summary>
        /// 用气量
        /// </summary>
        public decimal Air { get; set; }
    }
}
