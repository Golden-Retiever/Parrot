using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Models
{
    /// <summary>
    /// 趋势-设备模型
    /// </summary>
    public class TrendDeviceModel
    {
        /// <summary>
        /// 设备名称
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// 设备变量模型
        /// </summary>
        public List<TrendDeviceVarModel> VarList { get; set; }
    }
}
