using Platform.Helper;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace Platform.Views
{
    /// <summary>
    /// LoginWin.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWin : Window
    {
        public LoginWin()
        {
            InitializeComponent();
            ActionHelper.Register<object>("Register", new Func<object, bool>(param =>
            {
                var win = new RegisterWin();
                // 设置所有者，让注册窗口居中于登录窗口
                var loginWin = Application.Current.Windows.OfType<LoginWin>().FirstOrDefault();
                if (loginWin != null)
                {
                    win.Owner = loginWin;
                    win.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                }
                return win.ShowDialog() == true;
            }));
        }

        private bool ShowRegisterWin(object obj)
        {
            return ShowDialog(new RegisterWin());
        }

        private bool ShowDialog(Window dialog)
        {
            bool showdialog = (dialog.ShowDialog()==true);
            return showdialog;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }
    }
}
