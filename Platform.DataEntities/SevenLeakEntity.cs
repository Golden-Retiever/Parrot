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
    /// 七日能耗-泄露量
    /// </summary>
    [Table("SevenLeaks")]
    public class SevenLeakEntity
    {
        [Key]
        public int LeakId { get; set; }

        /// <summary>
        /// 日
        /// </summary>
        public string Day { get; set; }

        /// <summary>
        /// 泄漏量
        /// </summary>
        public decimal Leak { get; set; }
    }
}
