using System.Windows;
using System.Windows.Media;

namespace Platform.Helper
{
    public class ThemeHelper
    {
        /// <summary>
        /// 全局主题切换  改 App 资源里的笔刷，界面用 DynamicResource 的控件立即变色
        /// </summary>
        /// <param name="theme"></param>
        public static string CurrentTheme { get; set; } = ""; /* 静态变量，存在进程级内存*/
        public static void Apply(string? theme)
        {
            
            CurrentTheme = theme ?? ""; /* 注入当前主题 */
            bool dark = string.Equals(theme, "Dark", StringComparison.OrdinalIgnoreCase); /* 忽略大小写判断主题是否为Dark */

            /* 当前整个程序的全局对象 */
            var app = Application.Current;
            app.Resources["App.Background"] = dark ? Brushes.Black : Brushes.White;
            app.Resources["App.Foreground"] = dark ? Brushes.White : Brushes.Black;
            app.Resources["App.ForegroundSecondary"] = dark ? new SolidColorBrush(Color.FromRgb(0xAA, 0xAA, 0xAA)) : new SolidColorBrush(Color.FromRgb(0x77, 0x77, 0x77));
            app.Resources["App.Border"] = dark ? new SolidColorBrush(Color.FromRgb(0x3F, 0x3F, 0x3F)) : new SolidColorBrush(Color.FromRgb(0xDD, 0xDD, 0xDD));
            app.Resources["App.ControlBackground"] = dark ? new SolidColorBrush(Color.FromRgb(0x2D, 0x2D, 0x2D)) : new SolidColorBrush(Color.FromRgb(0xF7, 0xF9, 0xFA));
            app.Resources["App.RowAlt"] = dark ? new SolidColorBrush(Color.FromArgb(0x22, 0x00, 0x00, 0x00)) : new SolidColorBrush(Color.FromArgb(0x33, 0xFF, 0xFF, 0xFF));
            app.Resources["App.ToggleIcon"] = dark ? Brushes.White : new SolidColorBrush(Color.FromArgb(0x66, 0x00, 0x00, 0x00));
            app.Resources["App.ToggleWrite"] = dark ? Brushes.White : new SolidColorBrush(Color.FromArgb(0x99, 0x00, 0x00, 0x00));
        }
    }
}
