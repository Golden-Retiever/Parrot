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
        #region 使用属性
        // 属性的名称 (如，Protocol)
        public string PropName { get; set; }

        /// <summary>
        /// 属性值 (如,ModbusRTU)
        /// </summary>
        public string PropValue { get; set; }
        #endregion


        #region 设置属性
        /// <summary>
        /// 设置属性名称
        /// </summary>
        public string PropTitle {  get; set; }

        /// <summary>
        /// 设置属性类型，0-文本/1-下拉
        /// </summary>
        public int PropType { get; set; }

        /// <summary>
        /// 下拉选项
        /// </summary>
        public List<string> ValueOptions {  get; set; }
        #endregion
    }
}
