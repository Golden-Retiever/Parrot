using Platform.Components;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Platform.Converter
{
    /// <summary>
    /// 设备转换器
    /// </summary>
    public class DeviceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            #region 水平/垂直对齐线
            if (value.ToString() == "HL")//水平线
            {
                return new Line
                {
                    X1 = 0,//起点坐标
                    Y1 = 0,

                    X2 = 2000,//终点坐标
                    Y2 = 0,

                    Stroke = Brushes.Red,
                    StrokeThickness = 1,//粗细

                    StrokeDashArray = new DoubleCollection { 3, 3 },//虚线
                    ClipToBounds = true,

                };
            }

            if (value.ToString() == "VL")//垂直线
            {
                return new Line
                {
                    X1 = 0,//起点坐标
                    Y1 = 0,

                    X2 = 0,//终点坐标
                    Y2 = 2000,

                    Stroke = Brushes.Red,
                    StrokeThickness = 1,//粗细

                    StrokeDashArray = new DoubleCollection { 3, 3 },//虚线
                    ClipToBounds = true,

                };
            }
            #endregion

            var assembly = Assembly.Load("Platform.Components");
            Type t = assembly.GetType("Platform.Components." + value.ToString());
            var obj = Activator.CreateInstance(t);

            #region 宽/高 对齐线
            if (new string[] { "HeightRule", "WidthRule" }.Contains(value.ToString()))
            {
                return obj;
            }
            #endregion

            var c = (ComponentBase)obj;

            #region 删除
            //绑定
            //<DeleteCommand={bing DeleteModelCommand}> DeleteParameterProperty={binding}

            //绑定命令
            Binding binding = new Binding();
            binding.Path = new System.Windows.PropertyPath("DeleteCommand");//业务命令
            c.SetBinding(ComponentBase.DeleteCommandProperty, binding);

            //绑定参数
            binding = new Binding();
            c.SetBinding(ComponentBase.DeleteParameterProperty, binding);
            #endregion

            #region 是否选中
            //<IsSelected={bing IsSelected}>
            binding = new Binding();
            binding.Path = new System.Windows.PropertyPath("IsSelected");//业务属性
            c.SetBinding(ComponentBase.IsSelectedProperty, binding);
            #endregion

            #region 处理组件尺寸缩放命令逻辑绑定

            binding = new Binding();
            binding.Path = new System.Windows.PropertyPath("ResizeDownCommand");//业务里的
            c.SetBinding(ComponentBase.ResizeDownCommandProperty, binding);

            binding = new Binding();
            binding.Path = new System.Windows.PropertyPath("ResizeMoveCommand");
            c.SetBinding(ComponentBase.ResizeMoveCommandProperty, binding);

            binding = new Binding();
            binding.Path = new System.Windows.PropertyPath("ResizeUpCommand");
            c.SetBinding(ComponentBase.ResizeUpCommandProperty, binding);

            #endregion

            #region 流向
            binding = new Binding();
            binding.Path = new System.Windows.PropertyPath("FlowDirection");//业务属性名称
            c.SetBinding(ComponentBase.FlowDirectionProperty, binding);
            #endregion

            #region 旋转

            binding = new Binding();
            binding.Path = new System.Windows.PropertyPath("Rotate");//业务属性名称
            c.SetBinding(ComponentBase.RotateAngleProperty, binding);

            #endregion

            #region 变量监控

            //是否监控
            binding = new Binding();
            binding.Path = new System.Windows.PropertyPath("IsMonitor");// Model中的属性
            c.SetBinding(ComponentBase.IsMonitorProperty, binding);// 组件中的依赖属性

            //变量列表
            binding = new Binding();
            binding.Path = new System.Windows.PropertyPath("DeviceVarList");// Model中的属性
            c.SetBinding(ComponentBase.VarListProperty, binding);// 组件中的依赖属性

            #endregion

            #region 报警提醒

            binding = new Binding();
            binding.Path = new System.Windows.PropertyPath("IsWarning");// Model中的属性
            c.SetBinding(ComponentBase.IsWarningProperty, binding);// 组件中的依赖属性

            binding = new Binding();
            binding.Path = new System.Windows.PropertyPath("WarningMsg");// Model中的属性
            c.SetBinding(ComponentBase.WarningMessageProperty, binding);// 组件中的依赖属性

            binding = new Binding();
            binding.Path = new System.Windows.PropertyPath("DataContext.AlarmDetailCommand");// 业务中，该命令在监控视图模型里，不在设备model里
            binding.RelativeSource = new RelativeSource { AncestorType = typeof(Window) };
            c.SetBinding(ComponentBase.AlarmDetailCommandProperty, binding);// 组件中的依赖属性

            #endregion

            #region 手动控制
            binding = new Binding();
            binding.Path = new System.Windows.PropertyPath("ManualControlList");// Model中的属性
            c.SetBinding(ComponentBase.ManualControlListProperty, binding);// 组件中的依赖属性

            binding = new Binding();
            binding.Path = new System.Windows.PropertyPath("ManualControlCommand");// Model中的属性
            c.SetBinding(ComponentBase.ManualControlCommandProperty, binding);// 组件中的依赖属性

            #endregion

            return c;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
