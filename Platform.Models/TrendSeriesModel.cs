using LiveCharts;
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
    /// 图表的某一条序列
    /// </summary>
    public class TrendSeriesModel
    {
        #region 基本属性

        /// <summary>
        /// 设备编号
        /// </summary>
        public string DeviceNum { get; set; }

        /// <summary>
        /// 变量编号
        /// </summary>
        public string VarNum { get; set; }


        private string _title;

        /// <summary>
        /// 图表标题
        /// </summary>
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                Series.Title = value;
            }
        }

        private string _color;

        /// <summary>
        /// 颜色
        /// </summary>
        public string Color
        {
            get { return _color; }
            set
            {
                _color = value;
                Series.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(value));
            }
        }

        private string _ANum;

        /// <summary>
        /// 纵轴编号
        /// </summary>
        public string ANum
        {
            get { return _ANum; }
            set
            {
                _ANum = value;
                var index = AxisIndexFunc?.Invoke(value);
                Series.ScalesYAt = (index == null ? 0 : (int)index);//纵轴索引位置
            }
        }

        #endregion

        #region 显示序列

        public Func<string, int> AxisIndexFunc { get; set; }//获取序列索引

        /// <summary>
        /// 显示的一个序列(livecharts)
        /// </summary>
        public LineSeries Series { get; set; } = new LineSeries()
        {
            Values = new ChartValues<double>(),
            Fill = Brushes.Transparent,
            StrokeThickness = 2
        };

        #endregion
    }
}
