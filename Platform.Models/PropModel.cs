using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Models
{
    /// <summary>
    /// 属性信息
    /// </summary>
    public class PropModel
    {
        /// <summary>
        /// 属性ID
        /// </summary>
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

        /// <summary>
        /// 选项值列表
        /// </summary>
        public List<string> ValueOptions { get; set; }

        /// <summary>
        /// 属性默认选择索引
        /// </summary>
        public int OptionsSelectedIndex { get; set; }
    }
}
