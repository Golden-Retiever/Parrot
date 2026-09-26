using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Platform.Models
{
    /// <summary>
    /// 预警模型
    /// </summary>
    public class TrendSectionModel : BindableBase
    {
        #region 基本属性

        /// <summary>
        /// 趋势-预警  接收方法。属性值发生改变执行
        /// </summary>
        public Action ValueChanged { get; set; }

        private double _value = 0;

        /// <summary>
        /// 预警值
        /// </summary>
        public double Value
        {
            get { return _value; }
            set
            {
                _value = value;
                Section.Value = value;//发生变化，显示也要跟着变化
                ValueChanged?.Invoke();
            }
        }

        private string _color = "Red";

        /// <summary>
        /// 预警线颜色
        /// </summary>
        public string Color
        {
            get { return _color; }
            set
            {
                _color = value;

                //发生变化，显示也要跟着变化

                Section.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(value));

                ValueChanged?.Invoke();
            }
        }

        #endregion

        #region 显示

        /// <summary>
        /// 图表对象  来自在于LiveCharts  体现在界面上
        /// </summary>
        public AxisSection Section { get; set; } = new AxisSection()
        {
            Value = 0,
            Stroke = Brushes.Red,
            StrokeThickness = 1,
            StrokeDashArray = new DoubleCollection { 5, 5 }
        };
        #endregion
    }
}
