using System.Windows;

namespace Platform.Views
{
    /// <summary>
    /// 管理员口令输入框:二级管理员升级时弹出对口令
    /// </summary>
    public partial class AdminCmdInputWin : Window
    {
        private string _input = null;//确定=输入内容;取消/关闭=null

        public AdminCmdInputWin()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 弹出口令输入框,返回输入内容;取消返回null
        /// </summary>
        public static string Input()
        {
            var win = new AdminCmdInputWin();
            win.ShowDialog();
            return win._input;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            _input = pwdBox.Text;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();//_input保持null
        }
    }
}
