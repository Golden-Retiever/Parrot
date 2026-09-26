using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Models
{
    /// <summary>
    /// 手动控制
    /// </summary>
    public class ManualControlModel
    {
        /// <summary>
        /// 控制名称
        /// </summary>
        public string ControlName { get; set; }

        /// <summary>
        /// 控制地址
        /// </summary>
        public string ControlAddress { get; set; }

        /// <summary>
        /// 控制的值
        /// </summary>
        public string ControlValue { get; set; }
    }
}
