
using System.Globalization;
using System.Windows.Data;

namespace Platform.Converter
{
    public class UserTypeConverter : IValueConverter
    {
        /// <summary>
        /// 将接收的值转成ui显示的
        /// </summary>
        /// <param name="value">接收的值</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
           int role=int.Parse(value.ToString());
                        return role switch { 2 => "一级管理员", 1 => "二级管理员", _ => "非管理员" };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

}
