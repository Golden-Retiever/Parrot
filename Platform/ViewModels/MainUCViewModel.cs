using LiveCharts;
using Microsoft.Win32;
using Platform.DataEntities;
using Platform.DBAccess;
using Platform.DeviceAccess;
using Platform.DeviceAccess.Base;
using Platform.DeviceAccess.Execute;
using Platform.DeviceAccess.Transfer;
using Platform.Helper;
using Platform.Logger;
using Platform.Models;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace Platform.ViewModels
{
    public class MainUCViewModel : BindableBase, IDialogAware
    {
        ILoggerService<MainUCViewModel> _loggerService;

        private IDataAccess _dataAccess; /* 数据访问 */

        /// <summary>
        /// 登录成功的用户信息
        /// </summary>
        public SysUserModel LoginUserModel { get; set; } = new SysUserModel();

        public MainUCViewModel(IDataAccess dataAccess, IEventAggregator eventAggregator, ILoggerService<MainUCViewModel> loggerService)
        {   
            //日志服务
            _loggerService = loggerService;
            
            //数据访问
            _dataAccess = dataAccess;
            
            //主题初始化
            ThemeHelper.Apply(_dataAccess.GetSetting()?.Theme ?? "");

            InitScenarioTabs();//恢复上次的场景标签列表(多场景:一个场景一个db文件)
            InitMenu();//左侧菜单初始化
            InitHadDevices();//初始化设备
            InitSevenPower();//初始化7日耗电量
            InitSevenAir();//初始化7日能耗 - 用气量
            InitSevenLeak();//初始化7日能耗 - 泄露量
            InitAirRanking();//初始化用气排行
            InitDeviceWarning();//初始化设备提醒
            InitMonitorSettingVar(); //初始化监控配置变量
            
            #region 监视正在报警的设备量
            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(2000);
                    AlarmDeviceCount = DeviceList.Where(d => d.IsWarning).Count();
                }
            });
            #endregion
            
            #region 订阅报警处理结果
            eventAggregator.GetEvent<HandleAlarmEvent>().Subscribe(HandleDeviceAlram);
            #endregion
        }

        #region 报警处理
        /// <summary>
        /// 报警处理
        /// </summary>
        /// <param name="deviceNum">设备编号</param>
        private void HandleDeviceAlram(string deviceNum)
        {
            var device = DeviceList.FirstOrDefault(t => t.DeviceNum == deviceNum);
            if (device != null)
            {
                if (device.IsWarning)
                {
                    device.IsWarning = false;
                }
            }
        }
        #endregion 

        #region IDialogAware的实现
        public string Title { get; set; } = "主界面";

        public DialogCloseListener RequestClose { get; set; }

        /// <summary>
        /// 是否可以关闭
        /// </summary>
        /// <returns></returns>
        public bool CanCloseDialog()
        {
            return true;
        }

        /// <summary>
        /// 关闭时
        /// </summary>
        public void OnDialogClosed()
        {

        }

        /// <summary>
        /// 打开时
        /// </summary>
        /// <param name="parameters"></param>
        public void OnDialogOpened(IDialogParameters parameters)
        {
            LoginUserModel = parameters.GetValue<SysUserModel>("LoginUser");

            //登录信息赋值后才启动监控:构造里启动会抢在赋值前写记录,Account=null炸库
            Monitor();
        }
        #endregion

        #region 左侧菜单
        private UserControl _viewFunc; /* 正在显示的功能页(UserControl 类型)， 244 行使用 */

        public UserControl ViewFunc
        {
            get { return _viewFunc; }
            set
            {
                SetProperty(ref _viewFunc, value);
            }
        }

        public List<MenuModel> MenuList { get; set; }

        /// <summary>
        /// 左侧菜单初始化
        /// </summary>
        private void InitMenu()
        {
            //_loggerService.Info("左侧初始化菜单");
            MenuList =
                [
                    new MenuModel{  IsSelected=true, MenuName="监控", MenuIcon="\ue639",TargetFunc="MonitorUC"},
                    new MenuModel{  MenuName="趋势", MenuIcon="\ue61a",TargetFunc="TrendUC"},
                    new MenuModel{  MenuName="报警", MenuIcon="\ue60b",TargetFunc="AlarmUC"},
                    new MenuModel{  MenuName="报表", MenuIcon="\ue703",TargetFunc="ReportUC"},
                    new MenuModel{  MenuName="配置", MenuIcon="\ue60f",TargetFunc="SettingsUC"},
                ];

            DoSwitchFunc(MenuList[0]); /* 默认第一个功能 */
        }

        //切换功能命令
        public DelegateCommand<MenuModel> SwitchFuncCommand => new DelegateCommand<MenuModel>(DoSwitchFunc);

        /// <summary>
        /// 切换功能
        /// </summary>
        /// <param name="menuModel"></param>
        private void DoSwitchFunc(MenuModel menuModel)
        {
            /* 非管理员(一级/二级)，且非监控功能 */
            if (LoginUserModel.Role == 0 && menuModel.TargetFunc != "MonitorUC")
            {
                MenuList[0].IsSelected = true; /* 没有权限，默认选择监控功能 */
                if (ActionHelper.ExecuteAndResult<object>("ShowRight", null))
                {
                    DoReLogin(); /* 重新登录 */
                }
            }
            else
            {
                if (ViewFunc != null && ViewFunc.GetType().Name == menuModel.TargetFunc) return; /* 和之前点击功能一样，不再执行 */
                Type type = Assembly.Load("Platform").GetType("Platform.Views.FunctionUC." + menuModel.TargetFunc)!;
                ViewFunc = Activator.CreateInstance(type) as UserControl;
            }
        }
        #endregion

        #region 登录业务
        /// <summary>
        /// 重新登录
        /// </summary>
        private void DoReLogin()
        {
            Process.Start("Platform.exe");
            Application.Current.Shutdown();
        }

        /// <summary>
        /// 退出登录命令
        /// </summary>
        public DelegateCommand LogoutCommand => new DelegateCommand(DoReLogin);

        /// <summary>
        /// 重置密码命令
        /// </summary>
        public DelegateCommand ResetPwdCommand => new DelegateCommand(() =>
        {
            if (MessageBox.Show("确定重置密码吗？", "温馨提示", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                string newPwd = Md5Hepler.ComputeMD5Hash("123456");//将密码重置为123456
                int result = _dataAccess.ResetPwd(LoginUserModel.UserId, newPwd);
                if (result == 1)
                {
                    MessageBox.Show("重置密码成功");
                }
                else
                {
                    MessageBox.Show("重置密码失败");
                }
            }
        });
        #endregion

        #region 模糊度
        private int _viewBlur = 0;//默认0，即清晰

        public int ViewBlur
        {
            get { return _viewBlur; }
            set { SetProperty(ref _viewBlur, value); }
        }
        #endregion

        #region 场景管理(多场景:一个场景一个db文件,顶栏标签页切换)
        /// <summary>
        /// 场景标签集合(顶栏标签条)
        /// </summary>
        public ObservableCollection<ScenarioTabModel> ScenarioTabs { get; set; } = new ObservableCollection<ScenarioTabModel>();

        /// <summary>
        /// 场景清单(侧栏"场景"菜单里的打开列表)
        /// </summary>
        public List<ScenarioInfoModel> AllScenarioList { get; set; } = new List<ScenarioInfoModel>();

        /// <summary>
        /// 全部标签关闭后显示引导页
        /// </summary>
        public bool ShowScenarioGuide => ScenarioTabs.Count == 0;

        /// <summary>
        /// 初始化场景标签:恢复上次的标签列表和当前场景
        /// </summary>
        private void InitScenarioTabs()
        {
            ScenarioTabs.Clear();
            foreach (var (fileName, displayName) in ScenarioManager.GetOpenTabs())
            {
                ScenarioTabs.Add(new ScenarioTabModel
                {
                    FileName = fileName,
                    DisplayName = displayName,
                    IsCurrent = fileName == ScenarioManager.CurrentTabFileName,
                });
            }
            RefreshAllScenarioList();
            RaisePropertyChanged(nameof(ShowScenarioGuide));
        }

        /// <summary>
        /// 刷新侧栏场景菜单的清单列表
        /// </summary>
        private void RefreshAllScenarioList()
        {
            AllScenarioList = ScenarioManager.GetScenarioList()
                .Select(s => new ScenarioInfoModel { FileName = s.FileName, DisplayName = s.DisplayName })
                .ToList();
            RaisePropertyChanged(nameof(AllScenarioList));
        }

        /// <summary>
        /// 刷新标签激活状态(代码切换当前场景后)
        /// </summary>
        private void RefreshTabCurrentState()
        {
            foreach (var tab in ScenarioTabs)
            {
                tab.IsCurrent = tab.FileName == ScenarioManager.CurrentTabFileName;
            }
            RaisePropertyChanged(nameof(ShowScenarioGuide));
        }

        /// <summary>
        /// 切换标签命令(点标签页)
        /// </summary>
        public DelegateCommand<ScenarioTabModel> SwitchTabCommand => new DelegateCommand<ScenarioTabModel>(DoSwitchTab);

        /// <summary>
        /// 关闭标签命令(点❌/右键关闭:仅关视图不删文件,无弹窗)
        /// </summary>
        public DelegateCommand<ScenarioTabModel> CloseTabCommand => new DelegateCommand<ScenarioTabModel>(DoCloseTab);

        /// <summary>
        /// 全部关闭命令(右键菜单)
        /// </summary>
        public DelegateCommand CloseAllTabsCommand => new DelegateCommand(DoCloseAllTabs);

        /// <summary>
        /// 重命名命令(右键改名,只改显示名)
        /// </summary>
        public DelegateCommand<ScenarioTabModel> RenameTabCommand => new DelegateCommand<ScenarioTabModel>(DoRenameTab);

        /// <summary>
        /// 删除场景命令(右键删除:从清单移除并删掉场景库文件)
        /// </summary>
        public DelegateCommand<ScenarioTabModel> DeleteTabCommand => new DelegateCommand<ScenarioTabModel>(DoDeleteTab);

        /// <summary>
        /// 新建场景命令(侧栏场景菜单/引导页)
        /// </summary>
        public DelegateCommand NewScenarioCommand => new DelegateCommand(DoNewScenario);

        /// <summary>
        /// 打开已有场景命令(侧栏清单项,参数=文件名)
        /// </summary>
        public DelegateCommand<string> OpenScenarioCommand => new DelegateCommand<string>(DoOpenScenario);

        /// <summary>
        /// 导入场景命令(选外部.db复制进Scenarios)
        /// </summary>
        public DelegateCommand ImportScenarioCommand => new DelegateCommand(DoImportScenario);

        /// <summary>
        /// 导出当前场景命令(另存到任意位置)
        /// </summary>
        public DelegateCommand ExportScenarioCommand => new DelegateCommand(DoExportScenario);

        /// <summary>
        /// 切换标签:先停旧场景监控(单连接模型),换库,重跑初始化
        /// 守卫不能用 tab.IsCurrent(点击时 TwoWay 绑定已把它置 true,命令后执行会误判),
        /// 要跟 ScenarioManager 里的当前标签比
        /// </summary>
        private void DoSwitchTab(ScenarioTabModel tab)
        {
            if (tab == null || tab.FileName == ScenarioManager.CurrentTabFileName) return;

            StopMonitor();
            ScenarioManager.SetCurrentTab(tab.FileName);
            RefreshTabCurrentState();
            ReloadScenarioData();
        }

        /// <summary>
        /// 删除标签(右键):从清单移除并删掉该场景的数据库文件(.db/-wal/-shm)。
        /// 删的是当前场景时自动退到剩余第一个标签(没有则引导页),全程无确认弹窗
        /// </summary>
        private void DoDeleteTab(ScenarioTabModel tab)
        {
            if (tab == null) return;

            bool wasCurrent = tab.FileName == ScenarioManager.CurrentTabFileName;
            if (wasCurrent) StopMonitor();//防止监控还在读写要删的库

            bool fileGone = ScenarioManager.DeleteScenario(tab.FileName);
            ScenarioTabs.Remove(tab);

            if (wasCurrent)
            {
                if (ScenarioManager.CurrentTabFileName != null)
                    ReloadScenarioData();
                else
                    ClearScenarioData();
            }

            RefreshTabCurrentState();
            RefreshAllScenarioList();

            if (!fileGone)
                MessageBox.Show("场景已从清单移除，但数据库文件被占用暂时无法删除，可重启程序后再删 Scenarios 目录下的该文件",
                    "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        /// <summary>
        /// 关闭标签:关的是当前标签时退到剩余第一个;全关完显示引导页。
        /// 命名/列表随关闭自动落盘,全程无确认弹窗
        /// </summary>
        private void DoCloseTab(ScenarioTabModel tab)
        {
            if (tab == null) return;

            bool wasCurrent = tab.IsCurrent;
            ScenarioTabs.Remove(tab);
            ScenarioManager.RemoveOpenTab(tab.FileName);

            if (wasCurrent)
            {
                StopMonitor();
                var next = ScenarioTabs.FirstOrDefault();
                if (next != null)
                {
                    ScenarioManager.SetCurrentTab(next.FileName);
                    ReloadScenarioData();
                }
                else
                {
                    ClearScenarioData();
                }
            }

            ScenarioManager.SaveStateJson();
            RefreshTabCurrentState();
            RefreshAllScenarioList();
        }

        /// <summary>
        /// 全部关闭:停监控+清数据+落盘
        /// </summary>
        private void DoCloseAllTabs()
        {
            if (ScenarioTabs.Count == 0) return;

            StopMonitor();
            ScenarioTabs.Clear();
            foreach (string fileName in ScenarioManager.GetOpenTabs().Select(t => t.FileName).ToList())
            {
                ScenarioManager.RemoveOpenTab(fileName);
            }
            ClearScenarioData();
            ScenarioManager.SaveStateJson();
            RefreshAllScenarioList();
        }

        /// <summary>
        /// 右键改名:弹命名窗(预填旧名),确认后改显示名并落盘
        /// </summary>
        private void DoRenameTab(ScenarioTabModel tab)
        {
            if (tab == null) return;

            var payload = new ScenarioNamePayload { Title = "场景重命名", InitialName = tab.DisplayName };
            if (!ActionHelper.ExecuteAndResult<object>("ScenarioName", payload)) return;//取消
            if (string.IsNullOrWhiteSpace(payload.ResultName)) return;

            ScenarioManager.RenameScenario(tab.FileName, payload.ResultName);
            tab.DisplayName = payload.ResultName;//标签立即显示新名
            ScenarioManager.SaveStateJson();
            RefreshAllScenarioList();
        }

        /// <summary>
        /// 新建场景:命名→EnsureCreated建库→入清单→开标签并切换(上限6)
        /// </summary>
        private void DoNewScenario()
        {
            if (ScenarioTabs.Count >= ScenarioManager.MaxTabCount)
            {
                MessageBox.Show($"同时最多打开 {ScenarioManager.MaxTabCount} 个标签页，请先关闭一个", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var payload = new ScenarioNamePayload { Title = "新建场景" };
            if (!ActionHelper.ExecuteAndResult<object>("ScenarioName", payload)) return;//取消
            if (string.IsNullOrWhiteSpace(payload.ResultName)) return;

            if (ScenarioManager.DisplayNameExists(payload.ResultName))
            {
                if (MessageBox.Show($"已存在名为“{payload.ResultName}”的场景，仍要继续创建吗？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                    return;
            }

            try
            {
                StopMonitor();
                string fileName = ScenarioManager.NewScenario(payload.ResultName);
                ScenarioTabs.Add(new ScenarioTabModel { FileName = fileName, DisplayName = payload.ResultName, IsCurrent = true });
                ReloadScenarioData();
            }
            catch (Exception ex)
            {
                _loggerService.Error($"新建场景失败:{ex.Message}", ex);
                MessageBox.Show("新建场景失败：" + ex.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// 打开清单里的已有场景(已打开的仅激活,未打开的检查上限后开标签)
        /// </summary>
        private void DoOpenScenario(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return;

            var existing = ScenarioTabs.FirstOrDefault(t => t.FileName == fileName);
            if (existing != null)
            {
                DoSwitchTab(existing);
                return;
            }

            if (ScenarioTabs.Count >= ScenarioManager.MaxTabCount)
            {
                MessageBox.Show($"同时最多打开 {ScenarioManager.MaxTabCount} 个标签页，请先关闭一个", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                StopMonitor();
                ScenarioManager.OpenScenario(fileName);
                //元组是值类型:FirstOrDefault找不到返回default(null,null),??兜底
                string displayName = ScenarioManager.GetScenarioList()
                    .FirstOrDefault(s => s.FileName == fileName).DisplayName ?? Path.GetFileNameWithoutExtension(fileName);
                ScenarioTabs.Add(new ScenarioTabModel { FileName = fileName, DisplayName = displayName, IsCurrent = true });
                ReloadScenarioData();
            }
            catch (Exception ex)
            {
                _loggerService.Error($"打开场景失败:{ex.Message}", ex);
                MessageBox.Show("打开场景失败：" + ex.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// 导入场景:选外部.db,checkpoint后复制进Scenarios(同名自动加后缀)
        /// </summary>
        private void DoImportScenario()
        {
            if (ScenarioTabs.Count >= ScenarioManager.MaxTabCount)
            {
                MessageBox.Show($"同时最多打开 {ScenarioManager.MaxTabCount} 个标签页，请先关闭一个", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dlg = new OpenFileDialog
            {
                Title = "导入场景",
                Filter = "场景文件|*.db|所有文件|*.*",
            };
            if (dlg.ShowDialog() != true) return;

            try
            {
                StopMonitor();
                var (fileName, renamed) = ScenarioManager.ImportScenario(dlg.FileName);
                if (renamed)
                {
                    MessageBox.Show($"已存在同名场景文件，导入后重命名为“{Path.GetFileNameWithoutExtension(fileName)}”", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                ScenarioTabs.Add(new ScenarioTabModel
                {
                    FileName = fileName,
                    DisplayName = Path.GetFileNameWithoutExtension(fileName),
                    IsCurrent = true,
                });
                ReloadScenarioData();
            }
            catch (Exception ex)
            {
                _loggerService.Error($"导入场景失败:{ex.Message}", ex);
                MessageBox.Show("导入场景失败：" + ex.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// 导出当前激活场景(另存到任意位置)
        /// </summary>
        private void DoExportScenario()
        {
            string? currentFile = ScenarioManager.CurrentTabFileName;
            if (string.IsNullOrEmpty(currentFile))
            {
                MessageBox.Show("当前没有打开的场景", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var currentTab = ScenarioTabs.FirstOrDefault(t => t.IsCurrent);
            string displayName = currentTab?.DisplayName ?? Path.GetFileNameWithoutExtension(currentFile);
            var dlg = new SaveFileDialog
            {
                Title = "导出场景",
                Filter = "场景文件|*.db",
                FileName = displayName + ".db",
            };
            if (dlg.ShowDialog() != true) return;

            try
            {
                ScenarioManager.ExportScenario(currentFile, dlg.FileName);
                MessageBox.Show("导出成功：" + dlg.FileName, "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                _loggerService.Error($"导出场景失败:{ex.Message}", ex);
                MessageBox.Show("导出场景失败：" + ex.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// 重载场景数据:切场景/新建/导入后调用——
        /// 清旧场景界面数据,重跑全部初始化(设备/能耗/提醒/监控配置),重建取消器并重启监控
        /// </summary>
        private void ReloadScenarioData()
        {
            //清空随场景走的数据(Init*是追加式,不清会叠加)
            ClearEnergyCharts();
            DeviceWarningList = new List<DeviceWarningModel>();
            AirRankingList = new List<AirRankingModel>();

            InitHadDevices();
            InitSevenPower();
            InitSevenAir();
            InitSevenLeak();
            InitAirRanking();
            InitDeviceWarning();
            InitMonitorSettingVar();
            RefreshTabCurrentState();
            RefreshAllScenarioList();

            cts = new CancellationTokenSource();
            Monitor();
        }

        /// <summary>
        /// 清空当前场景的界面数据(全部标签关闭时)
        /// </summary>
        private void ClearScenarioData()
        {
            DeviceList = new ObservableCollection<DeviceModel>();
            DeviceWarningList = new List<DeviceWarningModel>();
            AirRankingList = new List<AirRankingModel>();
            TemperatureVar = null;
            HumidityVar = null;
            PM25Var = null;
            PressureVar = null;
            FlowRateVar = null;
            ClearEnergyCharts();
        }

        /// <summary>
        /// 清空三张7日能耗图表数据(Init*是追加式,重跑前必须清)
        /// </summary>
        private void ClearEnergyCharts()
        {
            PowerLineLabels.Clear();
            PowerLineValues.Clear();
            AirLineLabels.Clear();
            AirLineValues.Clear();
            LeakLineLabels.Clear();
            LeakLineValues.Clear();
        }
        #endregion

        #region 打开编辑窗口
        /// <summary>
        /// 打开编辑命令
        /// </summary>
        public DelegateCommand OpenComponentsEditCommand => new DelegateCommand(DoOpenComponentsEdit);

        /// <summary>
        /// 打开编辑窗体
        /// </summary>
        private void DoOpenComponentsEdit()
        {
            if (LoginUserModel.Role != 0)
            {
                this.ViewBlur = 5;//默认模糊

                //====进编辑界面立即停(轮询+循环写全部取消,编辑期间不对设备读写)====//
                StopMonitor();

                //====打开编辑窗(返回true=保存了配置,false=取消;两种情况都重启监控)====//
                ActionHelper.ExecuteAndResult<object>("ComponentsEdit", null);

                //====出编辑界面重启(保存了用新配置,取消了用原配置,都重新来)====//
                RestartMonitor();

                this.ViewBlur = 0;//变清晰
            }
            else
            {
                //如果没有权限重新登录
                if (ActionHelper.ExecuteAndResult<object>("ShowRight", null))
                {
                    //重新登录
                    DoReLogin();
                }
            }
        }
        #endregion

        #region 监控启停(编辑窗/场景切换共用)
        /// <summary>
        /// 停止监控:取消轮询+循环写,摘除推送回调,清空运行连接
        /// (进编辑窗/切换场景/关闭标签页共用)
        /// </summary>
        private void StopMonitor()
        {
            cts.Cancel();
            //等待所有task退出(任务进入取消态属正常,不视为失败)
            try { Task.WaitAll(taskList.ToArray(), 3000); }
            catch (AggregateException) { }
            taskList.Clear();
            //订阅式:摘除推送回调
            foreach (var (push, handler) in _pushHandlers)
            {
                push.MessageReceived -= handler;
            }
            _pushHandlers.Clear();
            //运行连接一并清空:手动写在监控外不可用
            _runEos.Clear();
        }

        /// <summary>
        /// 重启监控:重建取消发生器+重载设备+连接监控(出编辑窗时用)
        /// </summary>
        private void RestartMonitor()
        {
            cts = new CancellationTokenSource();
            InitHadDevices();
            Monitor();
        }
        #endregion

        #region 初始化设备

        private ObservableCollection<DeviceModel> _deviceList;

        public ObservableCollection<DeviceModel> DeviceList
        {
            get { return _deviceList; }
            set
            {
                SetProperty(ref _deviceList, value);
            }
        }


        private void InitHadDevices()
        {

            var deviceEntityList = _dataAccess.GetDevices();//获取所有设备
            var devicePropEntityList = _dataAccess.GetDeviceProps();//获取所有设备属性
            var deviceVarEntityList = _dataAccess.GetDeviceVarList();//获取所有设备变量
            var manControlList = _dataAccess.GetManualControlList();//获取所有手动控制
            var varAlarmConfList = _dataAccess.GetVarAlarmConfList();//获取所有变量报警配置

            var deviceList = deviceEntityList.Select(d => new DeviceModel
            {
                IsMonitor = true,//是否监控
                ComponentId = d.ComponentId,
                ComponentType = d.ComponentType,
                DeviceName = d.DeviceName,
                DeviceNum = d.DeviceNum,
                Width = d.Width,
                Height = d.Height,
                X = d.X,
                Y = d.Y,
                Z = d.Z,
                FlowDirection = d.FlowDirection,
                Rotate = d.Rotate,
                RefreshRate = d.RefreshRate,

                //获取该设备属性
                DevicePropList = new ObservableCollection<DevicePropModel>(devicePropEntityList.Where(dp => dp.DeviceNum == d.DeviceNum).Select(dp => new DevicePropModel { PropName = dp.PropName, PropValue = dp.PropValue })),

                //获取该设备变量
                DeviceVarList = new ObservableCollection<DeviceVarModel>(deviceVarEntityList.Where(dv => dv.DeviceNum == d.DeviceNum).Select(dv => new DeviceVarModel
                {
                    DeviceNum = dv.DeviceNum,
                    DeviceName = d.DeviceName,
                    VarNum = dv.VarNum,
                    VarName = dv.VarName,
                    VarAddress = dv.VarAddress,
                    Offset = dv.Offset,
                    Modulus = dv.Modulus,
                    VarType = dv.VarType,
                    IsRawFrame = dv.IsRawFrame,//勾帧行:地址格是完整帧文本,轮询单独裸发

                    //取出该变量的报警配置
                    VarAlarmConfList = new ObservableCollection<VarAlarmConfModel>(varAlarmConfList.Where(vac => vac.VarNum == dv.VarNum).Select(vac => new VarAlarmConfModel
                    {
                        ConfNum = vac.ConfNum,
                        Operator = vac.Operator,
                        CompareValue = vac.CompareValue,
                        AlarmContent = vac.AlarmContent,
                    }))
                })),

                //获取该设备的手动控制
                ManualControlList = new ObservableCollection<ManualControlModel>(manControlList.Where(mc => mc.DeviceNum == d.DeviceNum).Select(mc => new ManualControlModel
                {
                    ControlName = mc.ControlName,
                    ControlAddress = mc.ControlAddress,
                    IsRawFrame = mc.IsRawFrame,//勾帧行:地址格是完整帧文本,点击/循环直接裸发
                    ControlValue = mc.ControlValue,
                    IsLoop = mc.IsLoop,
                    ValueType = mc.ValueType,
                    ControlDelay = mc.ControlDelay,
                })),

            });

            DeviceList = new ObservableCollection<DeviceModel>(deviceList);


        }
        #endregion

        #region 监控
        private int _alarmDeviceCount;

        /// <summary>
        /// 报警设备量
        /// </summary>
        public int AlarmDeviceCount
        {
            get { return _alarmDeviceCount; }
            set
            {
                SetProperty(ref _alarmDeviceCount, value);
            }
        }

        CancellationTokenSource cts = new CancellationTokenSource();//取消发生器 需要取消取消的时候 调用Cancel()
        List<Task> taskList = new List<Task>();//保存所有的task

        //订阅式监听:每个设备的推送回调记录(保存时反注册,防止重复挂接、重复入库)
        private readonly List<(IPushSubscribe Push, Action<string, string> Handler)> _pushHandlers = new List<(IPushSubscribe Push, Action<string, string> Handler)>();

        //运行中各设备的执行对象(DeviceNum → EO):窗口层持有的"_device1"对应物,手动写按钮由此取连接
        private readonly Dictionary<string, ExecuteObject> _runEos = new Dictionary<string, ExecuteObject>();

        DataTable computeDt = new DataTable();//用来计算的

        Communication communication = Communication.CreateInstance();//通信实例

        #region  监控线程
        private void Monitor()
        {
            // 每个设备开一个线程
            foreach (var deviceModel in DeviceList)
            {
                #region 检查通信配置 和 变量参数
                if (deviceModel.DevicePropList.Count == 0 || deviceModel.DeviceVarList.Count == 0)
                {
                    _loggerService.Info($"设备{deviceModel.DeviceNum}没有属性或设备变量");
                    continue;
                }
                #endregion

                #region 获取执行对象  同一返回结果
                var resultEo = communication.GetExecuteObject(deviceModel.DevicePropList.Select(p => new DevicePropEntity { PropName = p.PropName, PropValue = p.PropValue }).ToList());
                #endregion 

                #region 检查传输对象状态，出错跳过
                if (!resultEo.Status)
                {
                    _loggerService.Fatal($"获取执行对象失败:{resultEo.Msg}");
                    deviceModel.IsWarning = true;//要报警了
                    deviceModel.WarningMsg = resultEo.Msg;//报警信息
                    string alarmNum = "A" + DateTime.Now.ToString("yyyyMMddHHmmssFFF");//报警编号
                    SaveDeviceAlarm(alarmNum, deviceModel, null, null, null, resultEo.Msg);//记录报警
                    continue;
                }
                #endregion

                #region 登记运行连接(窗口层持有,手动写按钮由此取)
                _runEos[deviceModel.DeviceNum] = resultEo.Data;
                #endregion

                #region 获取变量集合
                List<VariableProp> varList;
                try
                {
                    varList = deviceModel.DeviceVarList.Where(v => !v.IsRawFrame).Select(v => new VariableProp//勾帧行不进结构化分组,轮询里单独裸帧收发
                    {
                        VarNum = v.VarNum,
                        VarAddr = v.VarAddress,
                        ValueType = ByteConvert.ToType(v.VarType)//与写路径同一白名单:配错响亮失败
                    }).ToList();
                }
                catch (Exception ex)
                {
                    deviceModel.IsWarning = true;
                    deviceModel.WarningMsg = "配置错误";
                    _loggerService.Error($"设备{deviceModel.DeviceNum}变量类型配置错误:{ex.Message}");
                    SaveDeviceAlarm("A" + DateTime.Now.ToString("yyyyMMddHHmmssFFF"), deviceModel, null, null, null, ex.Message);
                    continue;//只跳过这台,其他设备照常监控
                }
                #endregion

                #region 分组
                var resultGroupAddr = resultEo.Data.GroupAddress(varList);
                if (!resultGroupAddr.Status)
                {
                    deviceModel.IsWarning = true;//要报警了
                    deviceModel.WarningMsg = resultGroupAddr.Msg;//报警信息
                    string alarmNum = "A" + DateTime.Now.ToString("yyyyMMddHHmmssFFF");//报警编号
                    SaveDeviceAlarm(alarmNum, deviceModel, null, null, null, resultGroupAddr.Msg);//记录报警
                    continue;
                }
                #endregion

                #region 数据解析
                Task task = Task.Run(async () =>
                {
                    try
                    {
                    bool loopStarted = false;//循环写启动标记:健康检查通过后只启动一次
                    //订阅式走事件驱动,问答式走轮询
                    #region 订阅式
                    if (deviceModel.DevicePropList?.FirstOrDefault(p => p.PropName == "Protocol")?.PropValue == "MQTT")
                    {
                        var push = resultEo.Data.TransferObject as IPushSubscribe;
                        if (push == null)
                        {
                            _loggerService.Fatal("MQTT 设备的传输对象为空");
                            return;
                        }

                        //首次读(后台):触发连接 Broker + 订阅主题
                        var firstRead = resultEo.Data.Read(resultGroupAddr.Data);
                        if (!firstRead.Status)
                        {
                            _loggerService.Fatal($"{resultEo.Data}读出错了，错误信息:{firstRead.Msg}");
                            deviceModel.IsWarning = true;//要报警了
                            deviceModel.WarningMsg = "服务器繁忙";//报警信息
                            SaveDeviceAlarm("A" + DateTime.Now.ToString("yyyyMMddHHmmssFFF"), deviceModel, null, null, null, firstRead.Msg);//记录报警
                            return;
                        }

                        if (!loopStarted)//健康检查通过(首轮读成功)才启动循环写
                        {
                            loopStarted = true;
                            int loopCount = StartLoopWrites(deviceModel, resultEo.Data, cts.Token);
                            if (loopCount > 0) _loggerService.Info($"设备{deviceModel.DeviceNum}健康检查通过,已启动{loopCount}条循环写");
                        }

                        void OnPush(string topic, string payload)
                        {
                            if (deviceModel.IsWarning) return;//与轮询的人工处理一致

                            var readResult = resultEo.Data.Read(resultGroupAddr.Data);//取缓存+协议层解析
                            if (!readResult.Status)
                            {
                                _loggerService.Fatal($"{resultEo.Data}读出错了，错误信息:{readResult.Msg}");
                                deviceModel.IsWarning = true;//要报警了
                                deviceModel.WarningMsg = "服务器繁忙";//报警信息
                                SaveDeviceAlarm("A" + DateTime.Now.ToString("yyyyMMddHHmmssFFF"), deviceModel, null, null, null, readResult.Msg);//记录报警
                                return;
                            }
                            ParseDeviceData(deviceModel, resultGroupAddr.Data);//与问答式轮询共用同一套解析/计算/报警/记录
                        }
                        Action<string, string> handler = OnPush;//固定委托实例,保存时才能精确摘除
                        push.MessageReceived += handler;
                        _pushHandlers.Add((push, handler));
                        return;
                    }
                    #endregion

                    #region 轮询
                    while (!cts.IsCancellationRequested)
                    {
                        #region 读:轮询读+解析+报警自动恢复
                        try
                        {
                            await Task.Delay(deviceModel.IsWarning ? 5000 : PosInt(deviceModel.RefreshRate), cts.Token);
                        }
                        catch (OperationCanceledException) { break; }//停止监控/进编辑:立即退出轮询,顺序走下方 DisConnect

                        var readResult = resultEo.Data.Read(resultGroupAddr.Data);//分组 按照功能码分组  每读一次就分组一次
                        if (!readResult.Status)
                        {
                            if (!deviceModel.IsWarning)//首次失败 → 记日志+报警表各一次;已报警期间静默重试,不刷日志/报警表
                            {
                                _loggerService.Fatal($"{resultEo.Data.ToString()}读出错了，错误信息:{readResult.Msg}");
                                deviceModel.IsWarning = true;//要报警了
                                deviceModel.WarningMsg = "服务器繁忙";//报警信息

                                string alarmNum = "A" + DateTime.Now.ToString("yyyyMMddHHmmssFFF");//报警编号
                                SaveDeviceAlarm(alarmNum, deviceModel, null, null, null, readResult.Msg);//记录报警
                            }
                            continue;
                        }

                        if (deviceModel.IsWarning)//报警中读到成功 → 自动解除报警
                        {
                            deviceModel.IsWarning = false;
                            deviceModel.WarningMsg = "";
                            _loggerService.Info($"设备{deviceModel.DeviceNum}通信恢复,报警自动解除");
                        }

                        ParseDeviceData(deviceModel, resultGroupAddr.Data);//解析+计算+报警+监控记录

                        foreach (var rawVar in deviceModel.DeviceVarList.Where(v => v.IsRawFrame))//勾帧行:一帧一收发,值直接上模型
                        {
                            try 
                            { 
                                rawVar.ReadValue = resultEo.Data.SendRaw(rawVar.VarAddress, ByteConvert.ToType(rawVar.VarType)); 
                            }
                            catch (Exception ex)
                            {
                                _loggerService.Error($"设备{deviceModel.DeviceNum}裸帧变量[{rawVar.VarName}]读取失败:{ex.Message}");
                                deviceModel.IsWarning = true; deviceModel.WarningMsg = ex.Message;
                            }
                        }
                        #endregion

                        if (!loopStarted)//健康检查通过(首轮读成功)才启动循环写
                        {
                            loopStarted = true;
                            int loopCount = StartLoopWrites(deviceModel, resultEo.Data, cts.Token);
                            if (loopCount > 0) _loggerService.Info($"设备{deviceModel.DeviceNum}健康检查通过,已启动{loopCount}条循环写");
                        }
                    }
                    #endregion

                    resultEo.Data.DisConnect();//断开连接
                    }
                    catch (Exception ex)
                    {
                        //读/解析抛出的异常原来会静默丢掉task,这里兜底记日志(设备已报警)
                        _loggerService.Error($"设备{deviceModel.DeviceNum}监控线程异常终止:{ex.Message}", ex);
                        deviceModel.IsWarning = true;
                        deviceModel.WarningMsg = ex.Message;
                    }
                }, cts.Token);
                #endregion 

                taskList.Add(task);//保存task
            }
        }
        #endregion

        #region 写
        /// <summary>
        /// 手动执行命令:组件卡片上的按钮,同 Aggrement 的写按钮——后台线程即时写,UI 立即返回
        /// </summary>
        public DelegateCommand<ManualControlModel> ManualControlCommand => new DelegateCommand<ManualControlModel>(DoManualControl);

        /// <summary>
        /// 手动执行:勾"循环"的由监控自动循环(点击无效);未勾的=后台线程即时写(与轮询读串行于传输实例锁)
        /// </summary>
        private void DoManualControl(ManualControlModel model)
        {
            if (model.IsLoop) return;//循环型随监控自动运行:进编辑停/出编辑重启,点击不介入
            var device = DeviceList.FirstOrDefault(d => d.ManualControlList.Contains(model));//行在哪个设备的列表里就归属哪个设备:监控层自持数据,模型不留回指字段(同 Aggrement)
            if (device == null) { _loggerService.Error("手动控制行未找到归属设备,无法执行"); return; }

            WriteDataInfo wd = null;
            if (!model.IsRawFrame)//勾帧行不走结构化装配:写入数据列可空,整帧文本直接裸发
            {
                wd = BuildWriteData(device, model);//按行上数据类型转字节;不符 → 已报警"配置错误"
                if (wd == null) return;
            }

            if (!_runEos.TryGetValue(device.DeviceNum, out ExecuteObject eo))//监控没启动 → 没有可用连接
            {
                _loggerService.Error($"设备{device.DeviceNum}未在监控中,手动控制[{model.ControlName}]不执行");
                device.IsWarning = true;
                device.WarningMsg = "未监控";
                return;
            }

            _ = Task.Run(() =>
            {
                if (model.IsRawFrame)//勾帧:整帧文本经协议 SendRaw 原样发送
                {
                    try { eo.SendRaw(model.ControlAddress, null); }
                    catch (Exception ex)
                    {
                        _loggerService.Error($"设备{device.DeviceNum}手动控制[{model.ControlName}]裸帧发送失败:{ex.Message}");
                        device.IsWarning = true; device.WarningMsg = ex.Message;
                    }
                    return;
                }

                var rw = eo.Write(wd);
                if (!rw.Status)//如果写失败了，则报警提示
                {
                    _loggerService.Error($"设备{device.DeviceNum}手动控制[{model.ControlName}]写地址{model.ControlAddress}失败:{rw.Msg}");
                    device.IsWarning = true;
                    device.WarningMsg = rw.Msg;
                }
            });
        }

        /// <summary>
        /// 按行的数据类型把写入数据转成写字节:类型决定字节数与寄存器数(Int32/Single=4字节2寄存器,Int64/Double=8字节4寄存器...);
        /// 数值转不动该类型或类型不支持 → 设备报警"配置错误",返回 null 不写
        /// </summary>
        private WriteDataInfo BuildWriteData(DeviceModel device, ManualControlModel model)
        {
            Type valueType;
            byte[] writeBytes;
            try
            {
                valueType = ByteConvert.ToType(model.ValueType);//类型名 → Type,查不到抛异常
                //WriteBytes 传主机小端(BitConverter 原生结果),转端由协议层 ConvertEndian 负责
                writeBytes = ByteConvert.ToBytes(new[] { model.ControlValue }, valueType, 1);
            }
            catch (Exception ex)//类型不支持/数值与类型不符(如 Int16 填 65280)或格式非法
            {
                _loggerService.Error($"设备{device.DeviceNum}手动控制[{model.ControlName}]写入数据[{model.ControlValue}]转{model.ValueType}失败:{ex.Message}");
                device.IsWarning = true; device.WarningMsg = "配置错误";
                return null;
            }

            return new WriteDataInfo
            {
                StartAddr = model.ControlAddress,
                ValueType = valueType,
                WriteBytes = writeBytes,
            };
        }

        /// <summary>
        /// 非负整数或兜底:0=不等待(按输入执行),空/非数字/负数 → 500兜底(轮询间隔与循环行间隔共用此保护)
        /// </summary>
        private static int PosInt(string text) => int.TryParse(text, out int v) && v >= 0 ? v : 500;

        /// <summary>
        /// 启动手动控制循环池:勾"循环"的条目收进同一个任务,按清单顺序写完一条再写下一条,整轮写完统一等4秒
        /// (与轮询读同一生命周期:随监控启动,进编辑随 cts 取消)
        /// </summary>
        /// <returns>加入池中的条数</returns>
        private int StartLoopWrites(DeviceModel deviceModel, ExecuteObject device, CancellationToken ct)
        {
            List<ManualControlModel> loopList = deviceModel.ManualControlList.Where(m => m.IsLoop).ToList();
            if (loopList.Count == 0) return 0;

            _ = Task.Run(async () =>
            {
                try
                {
                    while (!ct.IsCancellationRequested)
                    {
                        List<ManualControlModel> deadList = new List<ManualControlModel>();//本轮配置错误的条目
                        foreach (ManualControlModel model in loopList)//循环池:顺序写,一条写完(含串口应答)再写下一条
                        {
                            if (ct.IsCancellationRequested) break;
                            if (!model.IsLoop) continue;//运行中被取消勾选 → 跳过

                            if (model.IsRawFrame)//勾帧行:整帧文本直接裸发,写入数据列不参与
                            {
                                try { device.SendRaw(model.ControlAddress, null); }
                                catch (Exception ex)
                                {
                                    _loggerService.Error($"设备{deviceModel.DeviceNum}循环写[{model.ControlName}]发送失败:{ex.Message}");
                                    deviceModel.IsWarning = true; deviceModel.WarningMsg = ex.Message;
                                }
                                await Task.Delay(PosInt(model.ControlDelay), ct);//循环行间隔=行级延时(ms),0=连续写,空/非法→500兜底
                                continue;
                            }

                            WriteDataInfo wd = BuildWriteData(deviceModel, model);
                            if (wd == null) { deadList.Add(model); continue; }//配置错误不会自愈 → 已报警,本轮后剔除

                            var rw = device.Write(wd);
                            if (!rw.Status)
                            {
                                _loggerService.Error($"设备{deviceModel.DeviceNum}循环写[{model.ControlName}]地址{model.ControlAddress}失败:{rw.Msg}");
                                deviceModel.IsWarning = true; deviceModel.WarningMsg = rw.Msg;
                            }
                            await Task.Delay(PosInt(model.ControlDelay), ct);//循环行间隔=行级延时(ms),0=连续写,空/非法→500兜底
                        }
                        loopList.RemoveAll(deadList.Contains);
                        if (loopList.Count == 0) break;//池空 → 结束任务
                    }
                }
                catch (OperationCanceledException) { }//进编辑/停止监控 → 正常退出
                catch (Exception ex)
                {
                    _loggerService.Error($"设备{deviceModel.DeviceNum}循环池异常终止:{ex.Message}", ex);
                    deviceModel.IsWarning = true; deviceModel.WarningMsg = ex.Message;//设备断开等 → 循环终止
                }
            }, ct);
            return loopList.Count;
        }
        #endregion

        #region 解析数据
        /// <summary>
        /// 解析数据:计算+报警+监控记录。问答式轮询与订阅式推送共用
        /// </summary>
        /// <param name="deviceModel">设备</param>
        /// <param name="groupAddrList">分组后的变量</param>
        private void ParseDeviceData(DeviceModel deviceModel, List<GroupAddress> groupAddrList)
        {
            bool isAlarm = false;//是否报警 默认没有报警
            foreach (GroupAddress groupAddress in groupAddrList)
            {
                foreach (VariableProp variableProp in groupAddress.VarPropList)//循环设备的变量
                {
                    //协议层已解析好值(ReadValue),上层直接使用;解析失败在协议层 Read 的 catch 里体现
                    DeviceVarModel deviceVarModel = deviceModel.DeviceVarList.First(dv => dv.VarNum == variableProp.VarNum);
                    object oldReadValue = deviceVarModel.ReadValue;
                    deviceVarModel.ReadValue = variableProp.ReadValue;//协议层解析的结果放到变量模型

                    //偏移量换算(String 是文本,不参与数值换算)
                    if (deviceVarModel.VarType != "Boolean" && deviceVarModel.VarType != "String")
                    {
                        string exp = $"{deviceVarModel.ReadValue}*{deviceVarModel.Modulus}+{deviceVarModel.Offset}";
                        deviceVarModel.ReadValue = computeDt.Compute(exp, "");//将计算结果重新赋值
                    }

                    #region 报警提示
                    string? alarmNum = null;//报警编号
                    foreach (VarAlarmConfModel curAlarm in deviceVarModel.VarAlarmConfList)
                    {
                        //每个设备每次只能显示一个报警
                        if (isAlarm)
                        {
                            break;
                        }

                        //50<5
                        string exp = $"{deviceVarModel.ReadValue}{curAlarm.Operator}{curAlarm.CompareValue}";
                        if (Boolean.TryParse(computeDt.Compute(exp, "").ToString(), out bool result) && result)
                        {
                            //需要报警
                            isAlarm = true;
                            deviceModel.WarningMsg = curAlarm.AlarmContent;

                            alarmNum = "A" + DateTime.Now.ToString("yyyyMMddHHmmssFFF");//报警编号
                            SaveDeviceAlarm(alarmNum, deviceModel, deviceVarModel, curAlarm.ConfNum, deviceVarModel.ReadValue.ToString(), curAlarm.AlarmContent);//记录报警
                        }
                    }
                    #endregion

                    #region 记录监控值到DB -- 报表数据来源
                    //当需要记录的数量达到一定时(每积累到200条)，记录到db里一次。实时性有问题（采用）
                    //程序退出的时候，即使没有达到200条，也必须要记录，不能丢数据
                    if (deviceVarModel.VarType == "UInt16")//只记录数字
                    {
                        if (!deviceVarModel.ReadValue.Equals(oldReadValue))//值与之前不一样才写
                        {
                            MonitorRecordOperation.SaveRecords(_dataAccess,
                                new MonitorRecordEntity
                                {
                                    DeviceNum = deviceModel.DeviceNum,
                                    DeviceName = deviceModel.DeviceName,
                                    VarNum = deviceVarModel.VarNum,
                                    VarName = deviceVarModel.VarName,
                                    RecordValue = Convert.ToDecimal(deviceVarModel.ReadValue),
                                    Account = LoginUserModel.Account,
                                    AlarmNum = alarmNum,
                                });
                        }
                    }
                    #endregion
                }
            }

            deviceModel.IsWarning = isAlarm;//设置监控
        }
        #endregion 

        #region 记录报警信息
        /// <summary>
        /// 记录报警信息
        /// </summary>
        /// <param name="alarmNum">报警编号</param>
        /// <param name="device">报警设备</param>
        /// <param name="deviceVarModel">报警变量(可能不需要)</param>
        /// <param name="confNum">报警变量配置编号(可能不需要)</param>
        /// <param name="alarmValue">报警值(可能不需要)</param>
        /// <param name="AlarmContent">报警提示</param>
        private void SaveDeviceAlarm(string alarmNum, DeviceModel device, DeviceVarModel? deviceVarModel, string? confNum, string? alarmValue, string AlarmContent)
        {
            _dataAccess.SaveDeviceAlarm(new DeviceAlarmEntity
            {
                AlarmNum = alarmNum,
                Account = LoginUserModel.Account,
                AlarmContent = AlarmContent,
                AlarmValue = alarmValue,
                ConfNum = confNum,
                DeviceNum = device.DeviceNum,
                DeviceName = device.DeviceName,
                VarNum = (deviceVarModel == null ? null : deviceVarModel.VarNum),
                VarName = (deviceVarModel == null ? null : deviceVarModel.VarName),
            });
        }
        #endregion 
        #endregion

        #region 跳转到详情模块
        public DelegateCommand AlarmDetailCommand => new DelegateCommand(() =>
        {
            MenuList[2].IsSelected = true;//该菜单设置为已选中
            DoSwitchFunc(MenuList[2]);
        });
        #endregion

        #region 7日能耗 - 耗电量

        /// <summary>
        /// 耗电量
        /// </summary>
        public ChartValues<decimal> PowerLineValues { get; set; } = new ChartValues<decimal>();//Y轴

        /// <summary>
        /// 日期
        /// </summary>
        public List<string> PowerLineLabels { get; set; } = new List<string>();//X轴

        /// <summary>
        /// 初始化7日能耗 - 耗电量
        /// </summary>
        private void InitSevenPower()
        {
            var sevenPowerList = _dataAccess.GetSevenPowerList();

            foreach (var power in sevenPowerList)
            {
                PowerLineLabels.Add(power.Day);
                PowerLineValues.Add(power.Power);
            }
        }

        #endregion

        #region 7日能耗 - 用气量
        public ChartValues<decimal> AirLineValues { get; set; } = new ChartValues<decimal>();

        public List<string> AirLineLabels { get; set; } = new List<string>();//X轴

        /// <summary>
        /// 初始化7日能耗 - 用气量
        /// </summary>
        private void InitSevenAir()
        {
            var sevenAirList = _dataAccess.GetSevenAirList();

            foreach (var air in sevenAirList)
            {
                AirLineLabels.Add(air.Day);
                AirLineValues.Add(air.Air);
            }
        }
        #endregion

        #region 7日能耗 - 泄露量
        public ChartValues<decimal> LeakLineValues { get; set; } = new ChartValues<decimal>();

        public List<string> LeakLineLabels { get; set; } = new List<string>();//X轴

        /// <summary>
        /// 初始化7日能耗 - 泄露量
        /// </summary>
        private void InitSevenLeak()
        {
            var sevenLeakList = _dataAccess.GetSevenLeakList();

            foreach (var leak in sevenLeakList)
            {
                LeakLineLabels.Add(leak.Day);
                LeakLineValues.Add(leak.Leak);
            }
        }
        #endregion

        #region 用气排行
        public List<AirRankingModel> AirRankingList { get; set; }//用气量排行

        /// <summary>
        /// 初始化用气排行
        /// </summary>
        private void InitAirRanking()
        {
            AirRankingList = _dataAccess.GetAirRankings().Select(a => new AirRankingModel { WorkshopName = a.WorkshopName, PlanValue = a.PlanValue, FinishedValue = a.FinishedValue }).ToList();
        }

        #endregion

        #region 设备提醒

        public List<DeviceWarningModel> DeviceWarningList { get; set; }

        /// <summary>
        /// 初始化设备提醒
        /// </summary>
        private void InitDeviceWarning()
        {
            DeviceWarningList = _dataAccess.GetDeviceWarnings().Select(w => new DeviceWarningModel
            {
                Message = w.Message,
                DateTime = w.DateTime
            }).ToList();
        }

        #endregion

        #region 根据监控配置显示监控数据温度、湿度、PM25、压力、瞬时流量

        /// <summary>
        /// 温度
        /// </summary>
        public DeviceVarModel TemperatureVar { get; set; }

        /// <summary>
        /// 湿度
        /// </summary>
        public DeviceVarModel HumidityVar { get; set; }

        /// <summary>
        /// PM2.5
        /// </summary>
        public DeviceVarModel PM25Var { get; set; }//PM2.5

        /// <summary>
        /// 压力
        /// </summary>
        public DeviceVarModel PressureVar { get; set; }

        /// <summary>
        /// 瞬时流量
        /// </summary>
        public DeviceVarModel FlowRateVar { get; set; }

        /// <summary>
        /// 初始化监控配置的变量
        /// </summary>
        private void InitMonitorSettingVar()
        {
            var settingEntityList = _dataAccess.GetMonitorSettings();

            //温度
            var tempSetting = settingEntityList.FirstOrDefault(s => s.SettingNum == MonitorSettingNumEnum.TemperatureVar);
            if (tempSetting != null)
            {
                var device = DeviceList.FirstOrDefault(d => d.DeviceNum == tempSetting.DeviceNum);
                if (device != null)
                {
                    TemperatureVar = device.DeviceVarList.FirstOrDefault(v => v.VarNum == tempSetting.VarNum);
                }
            }

            //湿度
            var humiditySetting = settingEntityList.FirstOrDefault(s => s.SettingNum == MonitorSettingNumEnum.HumidityVar);
            if (humiditySetting != null)
            {
                var device = DeviceList.FirstOrDefault(d => d.DeviceNum == humiditySetting.DeviceNum);
                if (device != null)
                {
                    HumidityVar = device.DeviceVarList.FirstOrDefault(v => v.VarNum == humiditySetting.VarNum);
                }

            }

            //PM2.5
            var pm25Setting = settingEntityList.FirstOrDefault(s => s.SettingNum == MonitorSettingNumEnum.PM25Var);
            if (pm25Setting != null)
            {
                var device = DeviceList.FirstOrDefault(d => d.DeviceNum == pm25Setting.DeviceNum);
                if (device != null)
                {
                    PM25Var = device.DeviceVarList.FirstOrDefault(v => v.VarNum == pm25Setting.VarNum);
                }
            }

            //母管压力
            var pressureSetting = settingEntityList.FirstOrDefault(s => s.SettingNum == MonitorSettingNumEnum.PressureVar);
            if (pressureSetting != null)
            {
                var device = DeviceList.FirstOrDefault(d => d.DeviceNum == pressureSetting.DeviceNum);
                if (device != null)
                {
                    PressureVar = device.DeviceVarList.FirstOrDefault(v => v.VarNum == pressureSetting.VarNum);
                }
            }

            //瞬时流量
            var flowRateVarSetting = settingEntityList.FirstOrDefault(s => s.SettingNum == MonitorSettingNumEnum.FlowRateVar);
            if (flowRateVarSetting != null)
            {
                var device = DeviceList.FirstOrDefault(d => d.DeviceNum == flowRateVarSetting.DeviceNum);
                if (device != null)
                {
                    FlowRateVar = device.DeviceVarList.FirstOrDefault(v => v.VarNum == flowRateVarSetting.VarNum);
                }
            }
        }
        #endregion
    }
}
