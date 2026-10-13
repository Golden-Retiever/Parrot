using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Platform.Helper
{
    /// <summary>
    /// 必填星标记:模板红星按此显隐,默认显示(必填);选填字段标 RequiredMark.Star="Collapsed"
    /// </summary>
    public class RequiredMark
    {
        public static Visibility GetStar(DependencyObject obj)
        {
            return (Visibility)obj.GetValue(StarProperty);
        }

        public static void SetStar(DependencyObject obj, Visibility value)
        {
            obj.SetValue(StarProperty, value);
        }

        // Using a DependencyProperty as the backing store for Star.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StarProperty =
            DependencyProperty.RegisterAttached("Star", typeof(Visibility), typeof(RequiredMark), new PropertyMetadata(Visibility.Visible));
    }
}
