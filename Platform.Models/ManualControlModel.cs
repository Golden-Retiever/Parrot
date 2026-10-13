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
    public class ManualControlModel : BindableBase
    {
        /// <summary>
        /// 控制名称
        /// </summary>
        public string ControlName { get; set; }

        /// <summary>
        /// 控制地址
        /// </summary>
        public string ControlAddress { get; set; }

        private bool _isRawFrame;

        /// <summary>
        /// 是否裸帧(勾选=地址格填完整请求帧文本原样发送,写入数据列不参与;不勾=按结构化地址解析)
        /// </summary>
        public bool IsRawFrame
        {
            get { return _isRawFrame; }
            set { SetProperty(ref _isRawFrame, value); }
        }

        /// <summary>
        /// 控制的值
        /// </summary>
        public string ControlValue { get; set; }

        /// <summary>
        /// 数据类型(默认Int16,三点按钮切换)
        /// </summary>
        public string ValueType { get; set; } = "Int16";

        private bool _isLoop;

        /// <summary>
        /// 是否循环执行(勾选=随监控自动循环写,点击无效;不勾=点击写一次);
        /// 带通知:勾选瞬间联动延时框的置灰/解灰
        /// </summary>
        public bool IsLoop
        {
            get { return _isLoop; }
            set { SetProperty(ref _isLoop, value); }
        }

        /// <summary>
        /// 循环行间隔(毫秒):每轮写完本行后等待的时长,默认500;非循环行不适用(置灰)
        /// </summary>
        public string ControlDelay { get; set; } = "500";
    }
}
