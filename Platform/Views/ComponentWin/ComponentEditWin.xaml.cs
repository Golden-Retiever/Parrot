using Platform.Helper;
using Platform.Views.DialogWin;
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

namespace Platform.Views.ComponentWin
{
    /// <summary>
    /// ComponentEditWin.xaml 的交互逻辑
    /// </summary>
    public partial class ComponentEditWin : Window
    {
        public ComponentEditWin()
        {
            InitializeComponent();

            //注册打开变量报警配置界面
            ActionHelper.Register<object>("AlarmConf", new Action<object>(obj =>
            {
                new VariableAlarmConfWin() { Owner = this, DataContext = obj }.ShowDialog();
            }));
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            ActionHelper.Unregister("AlarmConf");
        }
    }
}
