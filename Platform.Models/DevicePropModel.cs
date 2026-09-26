using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Models
{
    /// <summary>
    /// 设备属性
    /// </summary>
    public class DevicePropModel
    {
        // 属性的名称 (如，Protocol)
        public string PropName { get; set; }

        /// <summary>
        /// 属性值 (如,ModbusRTU)
        /// </summary>

        public string PropValue { get; set; }
    }
}
