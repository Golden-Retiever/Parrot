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
    /// 七日能耗-耗电量
    /// </summary>
    [Table("SevenPowers")]
    public class SevenPowerEntity
    {
        [Key]
        public int PowerId { get; set; }

        /// <summary>
        /// 日  （1日、2日等）
        /// </summary>
        public string Day { get; set; }

        /// <summary>
        /// 耗电量
        /// </summary>
        public decimal Power { get; set; }
    }
}
