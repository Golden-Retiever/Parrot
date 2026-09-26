using Platform.Helper;
using Platform.Views.ComponentWin;
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
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Platform.Views
{
    /// <summary>
    /// MainUC.xaml 的交互逻辑
    /// </summary>
    public partial class MainUC : UserControl
    {
        public MainUC()
        {
            InitializeComponent();

            //注册显示权限提示界面
            ActionHelper.Register<object>("ShowRight", ShowRightView);

            //打开链路编辑界面
            ActionHelper.Register<object>("ComponentsEdit", ShowComponentsEditView);

            //打开纵轴编辑界面
            ActionHelper.Register<object>("ShowTrendAxisEdit",new Func<object, bool>(ShowTrendAxisEdit));

            //打开趋势选择设备变量界面
            ActionHelper.Register<object>("ShowTrendVars",new Func<object, bool>(ShowTrendDeviceVars));
        }

        /// <summary>
        /// 弹窗权限提示界面
        /// </summary>
        /// <param name="obj">一般是弹窗的DataContext</param>
        /// <returns>弹窗的DailogResult</returns>
        private bool ShowRightView(object obj)
        {
            //RightRemindWin rightRemindWin = new RightRemindWin();
            //this.Effect = new BlurEffect {  Radius=5};//界面模糊
            //bool dialogResult = (rightRemindWin.ShowDialog() == true);
            //this.Effect = null;//清晰
            //return dialogResult;
            return ShowDialog(new RightRemindWin());
        }

        /// <summary>
        /// 打开链路编辑界面
        /// </summary>
        /// <param name="obj">一般是弹窗的DataContext</param>
        /// <returns>弹窗的DailogResult</returns>
        private bool ShowComponentsEditView(object obj)
        {
            return ShowDialog(new ComponentEditWin());
        }

        /// <summary>
        /// 打开纵轴编辑
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool ShowTrendAxisEdit(object obj)
        {
            return ShowDialog(new TrendAxisEditWin() { DataContext = obj });
        }

        /// <summary>
        /// 打开趋势选择设备变量界面
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool ShowTrendDeviceVars(object obj)
        {
            return ShowDialog(new TrendDeviceChooseWin() { DataContext = obj });
        }

        /// <summary>
        /// 弹窗
        /// </summary>
        /// <param name="dialogWindow"></param>
        /// <returns></returns>
        private bool ShowDialog(Window dialogWindow)
        {
            this.Effect = new BlurEffect { Radius = 5 };//界面模糊
            bool dialogResult = (dialogWindow.ShowDialog() == true);
            this.Effect = null;//清晰
            return dialogResult;
        }

        /// <summary>
        /// 退出应用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }

        /// <summary>
        /// 窗体最小化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Min_Click(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);//找到当前所在是window
            window.WindowState = WindowState.Minimized;
        }
    }
}
