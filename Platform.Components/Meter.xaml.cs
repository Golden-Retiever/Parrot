using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Platform.Components
{
    /// <summary>
    /// Meter.xaml 的交互逻辑
    /// </summary>
    public partial class Meter : UserControl
    {
        public Meter()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 头
        /// </summary>
        public string Header { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        public object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(object), typeof(Meter),  new PropertyMetadata(0.0, new PropertyChangedCallback(OnValueChanged)));

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            string sData = "M0,160A160 160 0 0 1 {0} {1}";//从0,160  A 圆弧 横轴半径160 数轴半径160 旋转角度(0无需旋转) 0小弧(<180)  1逆时针  终点坐标
            sData = string.Format(sData,
                160 - 160 * Math.Cos(double.Parse(e.NewValue.ToString()) * 2.7 * Math.PI / 180),//横坐标
                160 - 160 * Math.Sin(double.Parse(e.NewValue.ToString()) * 2.7 * Math.PI / 180));//纵坐标

            var convter = TypeDescriptor.GetConverter(typeof(Geometry));
            (d as Meter).path.Data = (Geometry)convter.ConvertFrom(sData);
        }
    }
}
