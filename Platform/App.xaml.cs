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
            // 首次运行自动建库/升级（EF Core 应用所有迁移）
            // 已存在 platform.db 时为 no-op，不丢数据
            try
            {
                using (var ctx = new DBAccess.AppContext())
                {
                    ctx.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                // 库初始化失败不致命崩，记日志后继续（由登录页/数据访问层提示）
                // 需 _loggerService 但此处容器未就绪，暂用 MessageBox
                MessageBox.Show("数据库初始化失败：" + ex.Message, "启动错误", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                typeof(ILoggerService<>), // 泛型接口（需新增）
                typeof(NLogLoggerService<>) // 泛型实现（需新增）
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
        }
    }

}
