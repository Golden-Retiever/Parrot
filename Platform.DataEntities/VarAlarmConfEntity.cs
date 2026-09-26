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
    /// 变量报警配置表
    /// </summary>
    [Table("VarAlarmConfs")]
    public class VarAlarmConfEntity
    {
        /// <summary>
        /// 配置编号
        /// </summary>
        [Key]
        public string ConfNum { get; set; }

        /// <summary>
        /// 变量编号
        /// </summary>
        public string VarNum { get; set; }

        /// <summary>
        /// 操作符
        /// </summary>
        public string Operator { get; set; }

        /// <summary>
        /// 比较值
        /// </summary>
        public string CompareValue { get; set; }

        /// <summary>
        /// 报警提示内容
        /// </summary>
        public string AlarmContent { get; set; }
    }
}
