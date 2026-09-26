using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Platform.Converter
{
    /// <summary>
    /// 趋势图 切换 时候 需要 光标
    /// </summary>
    public class FocuseToSelectedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            (value as TextBox).GotFocus -= FocuseToSelectedConverter_GotFocus;
            (value as TextBox).GotFocus += FocuseToSelectedConverter_GotFocus;


            return Visibility.Visible;
        }

        private void FocuseToSelectedConverter_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            ((sender as TextBox).Tag as ListBoxItem).IsSelected = true;
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }

    }
}
