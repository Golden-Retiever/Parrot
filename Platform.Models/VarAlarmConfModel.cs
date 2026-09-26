using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Models
{
    /// <summary>
    /// 变量报警配置信息
    /// </summary>
    public class VarAlarmConfModel
    {
        /// <summary>
        /// 配置编号。作用后续报警重复提示的区分
        /// </summary>
        public string ConfNum { get; set; }

        /// <summary>
        /// 运算符
        /// </summary>
        public string Operator { get; set; }

        /// <summary>
        /// 比较值
        /// </summary>
        public string CompareValue { get; set; }

        /// <summary>
        /// 提醒内容
        /// </summary>
        public string AlarmContent { get; set; }
    }
}
