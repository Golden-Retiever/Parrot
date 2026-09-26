using DryIoc;
using LiveCharts;
using Platform.DataEntities;
using Platform.DBAccess;
using Platform.DeviceAccess;
using Platform.DeviceAccess.Base;
using Platform.DeviceAccess.Transfer;
using Platform.Helper;
using Platform.Logger;
using Platform.Models;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace Platform.ViewModels
{
    public class MainUCViewModel : BindableBase, IDialogAware
    {
        ILoggerService<MainUCViewModel> _loggerService;

        private IDataAccess _dataAccess;//数据访问

        /// <summary>
        /// 登录成功的用户信息
        /// </summary>
        public SysUserModel LoginUserModel { get; set; } = new SysUserModel();

        public MainUCViewModel(IDataAccess dataAccess, IEventAggregator eventAggregator, ILoggerService<MainUCViewModel> loggerService)
        {
            _loggerService = loggerService;//日志服务

            _dataAccess = dataAccess;//数据访问
            //左侧菜单初始化
            InitMenu();

            //初始化设备
            InitHadDevices();

            //直接连接PLC设备监控
            Monitor();

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

            InitSevenPower();//初始化7日耗电量

            InitSevenAir();//初始化7日能耗 - 用气量

            InitSevenLeak();//初始化7日能耗 - 泄露量

            InitAirRanking();//初始化用气排行

            InitDeviceWarning();//初始化设备提醒

            #region 订阅报警处理结果
            eventAggregator.GetEvent<HandleAlarmEvent>().Subscribe(HandleDeviceAlram);
            #endregion

            //初始化监控配置变量
            InitMonitorSettingVar();
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
        }
        #endregion

        #region 左侧菜单
        private UserControl _viewFunc;//右侧功能对象

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
            //_loggerService.Info("左侧初始化菜单123");
            MenuList =
                [
                    new MenuModel{  IsSelected=true, MenuName="监控", MenuIcon="\ue639",TargetFunc="MonitorUC"},
                    new MenuModel{  MenuName="趋势", MenuIcon="\ue61a",TargetFunc="TrendUC"},
                    new MenuModel{  MenuName="报警", MenuIcon="\ue60b",TargetFunc="AlarmUC"},
                    new MenuModel{  MenuName="报表", MenuIcon="\ue703",TargetFunc="ReportUC"},
                    new MenuModel{  MenuName="配置", MenuIcon="\ue60f",TargetFunc="SettingsUC"},
                ];

            DoSwitchFunc(MenuList[0]);//默认第一个功能
        }

        //切换功能命令
        public DelegateCommand<MenuModel> SwitchFuncCommand => new DelegateCommand<MenuModel>(DoSwitchFunc);

        /// <summary>
        /// 切换功能
        /// </summary>
        /// <param name="menuModel"></param>
        private void DoSwitchFunc(MenuModel menuModel)
        {
            //1、只针对该功能实现
            //2、代码优化（封装） 后面 弹出窗体，在窗体上面有操作 dialogresult

            //如果不是管理员，并且不是监控功能
            if (!LoginUserModel.IsAdmin && menuModel.TargetFunc != "MonitorUC")
            {
                MenuList[0].IsSelected = true;//没有权限，默认还是选择监控功能
                if (ActionHelper.ExecuteAndResult<object>("ShowRight", null))
                {
                    //重新登录
                    DoReLogin();
                }
            }
            else
            {
                //和之前点击的功能是一样的，就不用再执行
                if (ViewFunc != null && ViewFunc.GetType().Name == menuModel.TargetFunc)
                {
                    return;
                }
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

        #region 工艺链路编辑/组件编辑/设备编辑

        #region 模糊度
        private int _viewBlur = 0;//默认0，即清晰

        public int ViewBlur
        {
            get { return _viewBlur; }
            set { SetProperty(ref _viewBlur, value); }
        }
        #endregion

        /// <summary>
        /// 打开编辑命令
        /// </summary>
        public DelegateCommand OpenComponentsEditCommand => new DelegateCommand(DoOpenComponentsEdit);

        /// <summary>
        /// 打开编辑窗体
        /// </summary>
        private void DoOpenComponentsEdit()
        {
            if (LoginUserModel.IsAdmin)
            {
                this.ViewBlur = 5;//默认模糊
                //1、如果dialogresult=true
                if (ActionHelper.ExecuteAndResult<object>("ComponentsEdit", null))
                {
                    //1、取消task
                    cts.Cancel();

                    //2、等待之前所有的task完成  耗时操作
                    Task.WaitAll(taskList.ToArray());

                    //3、清空之前所有的task
                    taskList.Clear();

                    //4、重新实例一个取消发生器
                    cts = new CancellationTokenSource();

                    //5、订阅式:摘除推送回调
                    foreach (var (push, handler) in _pushHandlers)
                    {
                        push.MessageReceived -= handler;
                    }
                    _pushHandlers.Clear();

                    //初始化设备
                    InitHadDevices();

                    //直接连接PLC设备监控
                    Monitor();

                    //通过windows服务进行监控
                    //MonitorByService();

                }

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
                    ControlValue = mc.ControlValue,
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

                #region 获取变量集合
                List<VariableProp> varList = deviceModel.DeviceVarList.Select(v => new VariableProp
                {
                    VarNum = v.VarNum,
                    VarAddr = v.VarAddress,
                    ValueType = Type.GetType("System." + v.VarType)
                }).ToList();
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
                        //如果发现设备已经报警了，程序不要处理
                        if (deviceModel.IsWarning)
                        {
                            continue;
                        }

                        await Task.Delay(500);//等待500毫秒

                        var readResult = resultEo.Data.Read(resultGroupAddr.Data);//分组 按照功能码分组  每读一次就分组一次
                        if (!readResult.Status)
                        {
                            _loggerService.Fatal($"{resultEo.Data.ToString()}读出错了，错误信息:{readResult.Msg}");
                            deviceModel.IsWarning = true;//要报警了
                            deviceModel.WarningMsg = "服务器繁忙";//报警信息

                            string alarmNum = "A" + DateTime.Now.ToString("yyyyMMddHHmmssFFF");//报警编号
                            SaveDeviceAlarm(alarmNum, deviceModel, null, null, null, readResult.Msg);//记录报警

                            continue;
                        }

                        ParseDeviceData(deviceModel, resultGroupAddr.Data);//解析+计算+报警+监控记录(订阅式推送也走这里)
                    }
                    #endregion 

                    resultEo.Data.DisConnect();//断开连接
                }, cts.Token);
                #endregion 

                taskList.Add(task);//保存task
            }
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

                    //偏移量换算
                    if (deviceVarModel.VarType != "Boolean")
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
