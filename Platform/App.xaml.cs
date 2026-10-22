using System.Windows;
using Microsoft.EntityFrameworkCore;
using Platform.DBAccess;
using Platform.Logger;
using Platform.ViewModels;
using Platform.Views;


namespace Platform
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // ==== 场景拆分/恢复先行 ====
            try
            {
                DBAccess.ScenarioManager.EnsureFirstRunSplit();
            }
            catch (Exception ex)
            {
                MessageBox.Show("场景初始化失败：" + ex.Message, "启动错误", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            // ==== 主库结构升级 ====
            try
            {
                DBAccess.DbUpgrader.UpgradeMainDbIfNeeded();
            }
            catch (Exception ex)
            {
                MessageBox.Show("主库结构升级失败：" + ex.Message, "启动错误", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            // ==== 主库建库/应用迁移 ====
            try
            {
                using (var ctx = new DBAccess.AppContext())
                {
                    ctx.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("数据库初始化失败：" + ex.Message, "启动错误", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            // ==== 全部场景库结构升级 ====
            try
            {
                DBAccess.DbUpgrader.UpgradeAllScenarioDbs();
            }
            catch (Exception ex)
            {
                MessageBox.Show("场景库结构升级失败：" + ex.Message, "启动错误", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            base.OnStartup(e);
        }


        protected override Window CreateShell()
        {
            //启动登录界面
            return Container.Resolve<LoginWin>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<MainUCViewModel>();//将MainUCViewModel注册成单例

            //注册数据访问
            containerRegistry.Register<IDataAccess,DataAccess>();

            //注册主窗体对话框
            containerRegistry.RegisterDialog<MainUC>();

            containerRegistry.RegisterDialogWindow<DialogOuterWin>();

            // 注册泛型日志服务：ILoggerService<T> 关联到具体类 T
            containerRegistry.RegisterScoped(
                typeof(ILoggerService<>),   // 泛型接口
                typeof(NLogLoggerService<>) // 泛型实现
            );

            //containerRegistry.Register<ILogger>();
        }

        /// <summary>
        /// 重写退出
        /// </summary>
        /// <param name="e"></param>
        protected override void OnExit(ExitEventArgs e)
        {
            //取出一个实例对象
            var dataAccess=Container.Resolve<IDataAccess>();

            MonitorRecordOperation.SaveRecords(dataAccess,null);//记录设备监控数据

            dataAccess.DeleteDealAlarm(1);

            //场景状态自动落盘：显示名/标签列表/当前标签写入 scenarios.json（无弹窗）
            try
            {
                DBAccess.ScenarioManager.SaveStateJson();
            }
            catch { }
        }
    }

}
