using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Models
{
    /// <summary>
    /// 用气量排行
    /// </summary>
    public class AirRankingModel
    {
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
