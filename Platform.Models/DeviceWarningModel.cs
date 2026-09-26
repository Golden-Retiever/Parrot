using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Models
{
    /// <summary>
    /// 设备提醒
    /// </summary>
    public class DeviceWarningModel
    {
        /// <summary>
        /// 提醒信息内容
        /// </summary>

        public string Message { get; set; }

        /// <summary>
        /// 提醒时间
        /// </summary>
        public DateTime DateTime { get; set; }
    }
}
