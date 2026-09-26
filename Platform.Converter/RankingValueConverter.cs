using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Platform.Converter
{
    /// <summary>
    /// 用气排行转换器
    /// </summary>
    public class RankingValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var value = double.Parse(values[0].ToString());//数字(计划/已完成)
            var width = double.Parse(values[1].ToString());//border真是宽度
            return value / 240 * width;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
