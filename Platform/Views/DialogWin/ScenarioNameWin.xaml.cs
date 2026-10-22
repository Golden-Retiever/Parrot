using Platform.Models;
using System.Windows;
using System.Windows.Input;

namespace Platform.Views.DialogWin
{
    /// <summary>
    /// ScenarioNameWin.xaml 的交互逻辑
    /// 新建场景/重命名场景共用的命名弹窗：
    /// DataContext = ScenarioNamePayload（Title/InitialName 传入，确认后 ResultName 带出）
    /// </summary>
    public partial class ScenarioNameWin : Window
    {
        public ScenarioNameWin()
        {
            InitializeComponent();
            Loaded += ScenarioNameWin_Loaded;
        }

        /// <summary>
        /// 打开时预填旧名并全选（重命名时直接输入即覆盖）
        /// </summary>
        private void ScenarioNameWin_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ScenarioNamePayload payload)
            {
                txtName.Text = payload.InitialName;
            }
            txtName.Focus();
            txtName.SelectAll();
        }

        /// <summary>
        /// 回车=确定
        /// </summary>
        private void TxtName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnOk_Click(sender, e);
            }
        }

        /// <summary>
        /// 确定：回写结果名并返回 true
        /// </summary>
        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            string name = txtName.Text?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("场景名不能为空", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtName.Focus();
                txtName.SelectAll();
                return;
            }

            if (DataContext is ScenarioNamePayload payload)
            {
                payload.ResultName = name;
            }
            DialogResult = true;
        }

        /// <summary>
        /// 取消/关闭：返回 false
        /// </summary>
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
