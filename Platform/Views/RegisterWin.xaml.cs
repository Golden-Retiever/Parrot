using Platform.Models;
using System.Windows;

namespace Platform.Views
{
    /// <summary>
    /// RegisterWin.xaml 的交互逻辑
    /// </summary>
    public partial class RegisterWin : Window
    {
        public static SysUserModel RegisteredUser { get; set; } = null;

        public RegisterWin()
        {
            InitializeComponent();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
