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
    /// 用气排行表
    /// </summary>
    [Table("AirRankings")]
    public class AirRankingEntity
    {
        [Key]
        public int RankingId { get; set; }

        /// <summary>
        /// 车间名称
        /// </summary>
        public string WorkshopName { get; set; }

        /// <summary>
        /// 计划用气量
        /// </summary>
        public double PlanValue { get; set; }

        /// <summary>
        /// 已用气量
        /// </summary>
        public double FinishedValue { get; set; }
    }
}
