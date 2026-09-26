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
    /// 属性表
    /// </summary>
    [Table("Properties")]
    public class PropEntity
    {
        /// <summary>
        /// 属性ID
        /// </summary>
        [Key]
        public int PropId { get; set; }

        /// <summary>
        /// 属性标题(比如 通信协议、串口名称等)
        /// </summary>
        public string PropTitle { get; set; }

        /// <summary>
        /// 属性名称(比如 PortName、Protocol等)
        /// </summary>
        public string PropName { get; set; }

        /// <summary>
        /// 属性类型(0-文本，1-下拉)
        /// </summary>
        public int PropType { get; set; }
    }
}
