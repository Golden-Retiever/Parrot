using Platform.Helper;
using Platform.Models;
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
