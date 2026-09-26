using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Platform.Components.Converter
{
    /// <summary>
    /// 多重转换器(显示变量的时候需要，两个条件，1、是否监控 2、变量个数必须大于0)
    /// </summary>
    public class InfoButtonVisibilityValueConverter : IMultiValueConverter
    {
        /// <summary>
        /// 多值转换。
        /// </summary>
        /// <param name="values">值数组</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                bool stateBool = bool.TryParse(values[0].ToString(), out bool s1);//stateBool是否为bool  s1转换结果
                bool stateInt = int.TryParse(values[1].ToString(), out int s2);//stateInt是否为int s2转换结果
                if (stateBool && s1 && stateInt && s2 > 0)
                {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
            catch { return Visibility.Collapsed; }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
