using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using Platform.DataEntities;
using Platform.DBAccess;
using Platform.Helper;
using Platform.Models;
using Platform.Views;

namespace Platform.ViewModels.FunctionUC
{
    /// <summary>
    /// 配置视图模型
    /// </summary>
    public class SettingsUCViewModel : BindableBase
    {
        IDataAccess _dataAccess;
        public SettingModel Setting { get; set; }=new SettingModel();

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="dataAccess"></param>
        /// <param name="mainUCViewModel"></param>
        public SettingsUCViewModel(IDataAccess dataAccess, MainUCViewModel mainUCViewModel)
        {
            _dataAccess = dataAccess;
            _myRole = mainUCViewModel.LoginUserModel?.Role ?? 0; /* 当前登录者角色:升降级/删除的权限判据 */
            
            /* 配置主题 */
            Setting.Theme = ThemeHelper.CurrentTheme;
            Setting.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(Setting.Theme))
                    ThemeHelper.Apply(Setting.Theme);
            };

            CanSelectedList = mainUCViewModel.DeviceList.Where(d => d.DeviceVarList.Count > 0).ToList(); /* 可以选择的设备集合(必须要有变量) */          
            InitMonitorSetting(); /* 初始化监控配置 */        
            InitUsers();          /* 初始化用户信息 */
        }

        #region 监控配置
        /// <summary>
        /// 监控配置集合
        /// </summary>
        public ObservableCollection<MonitorSettingsModel> MonitorSettingList { get; set; } = new ObservableCollection<MonitorSettingsModel>();

        /// <summary>
        /// 可以选择的设备集合
        /// </summary>
        private List<DeviceModel> CanSelectedList;

        /// <summary>
        /// 初始化监控配置
        /// </summary>
        private void InitMonitorSetting()
        {
            var monitorEntityList = _dataAccess.GetMonitorSettings();

            MonitorSettingList.Clear();
            foreach (var item in monitorEntityList)
            {
                MonitorSettingList.Add(new MonitorSettingsModel
                {
                    SettingNum = item.SettingNum,
                    Header = item.Header,
                    Description = item.Description,
                    DeviceList = CanSelectedList,
                    DeviceNum = item.DeviceNum,
                    VarNum = item.VarNum
                });
            }
        }

        /// <summary>
        /// 保存监测命令
        /// </summary>
        public DelegateCommand SaveMonitorSetsCommand => new DelegateCommand(() =>
        {
            List<MonitorSettingEntity> settingList = new List<MonitorSettingEntity>();

            foreach (var item in this.MonitorSettingList)
            {
                settingList.Add(new MonitorSettingEntity
                {
                    SettingNum = item.SettingNum,
                    Header = item.Header,
                    Description = item.Description,
                    DeviceNum = item.DeviceNum,
                    VarNum = item.VarNum,
                });
            }

            _dataAccess.SaveMonitorSets(settingList);
            MessageBox.Show("保存监控设置成功");
        });
        #endregion

        #region 用户管理
        /// <summary>
        ///当前登录者角色:0=普通,1=二级,2=一级
        /// <summary>
        private readonly int _myRole;

        /// <summary>
        /// 升降级列是否可用:两级管理员都开放,口令门槛在命令里区分
        /// </summary>
        public bool CanEditRole => _myRole != 0;
        
        /// <summary>
        /// 用户集合
        /// </summary>
        public ObservableCollection<SysUserModel> UserList { get; set; } = new ObservableCollection<SysUserModel>();

        /// <summary>
        /// 初始化用户信息
        /// </summary>
        private void InitUsers()
        {
            var userEntityList = _dataAccess.GetSysUserList();

            UserList.Clear();
            userEntityList.ForEach(user =>
            {
                UserList.Add(new SysUserModel
                {
                    UserId = user.UserId,
                    Account = user.Account,
                    Role = user.Role,
                    Password = user.Password,
                    RealName = user.RealName,
                    Gender = user.Gender,
                    Phone = user.Phone,
                    Department = user.Department
                });
            });
        }

        /// <summary>
        /// 添加用户
        /// </summary>
        public DelegateCommand AddUserCommand => new DelegateCommand(() =>
        {
            UserList.Add(new SysUserModel());
        });

        /// <summary>
        /// 删除用户:二级管理员不能删除一级管理员
        /// </summary>
        public DelegateCommand<SysUserModel> DelUserCommand => new DelegateCommand<SysUserModel>((userModel) =>
        {
            if (userModel.Role == 2 && _myRole != 2)
            {
                MessageBox.Show("二级管理员不能删除一级管理员");
                return;
            }
            UserList.Remove(userModel);
        });

        /// <summary>
        /// 升级:单击升一级;升到二级谁都可直升,升到一级仅二级操作员需口令对上(一级操作员直升)
        /// </summary>
        public DelegateCommand<SysUserModel> PromoteUserCommand => new DelegateCommand<SysUserModel>(userModel =>
        {
            if (userModel.Role >= 2) return;//已是一级,无级可升

            if (userModel.Role + 1 == 2 && _myRole == 1)//升到一级且操作者是二级:必须口令对上
            {
                string dbCmd = _dataAccess.GetAdminCommand();
                string input = AdminCmdInputWin.Input();//取消或未填返回null
                if (string.IsNullOrEmpty(dbCmd) || input != dbCmd)
                {
                    MessageBox.Show("管理员口令错误");
                    return;
                }
            }

            userModel.Role++;//RoleText随PropertyChanged刷新
        });

        /// <summary>
        /// 降级:一级→二级→普通,单击降一级;两级管理员都可直接降,但二级不能动一级
        /// </summary>
        public DelegateCommand<SysUserModel> DemoteUserCommand => new DelegateCommand<SysUserModel>(userModel =>
        {
            if (userModel.Role <= 0) return;//已是普通,无级可降

            if (userModel.Role == 2 && _myRole != 2)
            {
                MessageBox.Show("二级管理员不能操作一级管理员");
                return;
            }

            userModel.Role--;
        });

        /// <summary>
        /// 重置密码。将用户密码改成123456
        /// </summary>
        public DelegateCommand<SysUserModel> ResetPwdCommand => new DelegateCommand<SysUserModel>((userModel) =>
        {
            _dataAccess.ResetPwd(userModel.UserId, Md5Hepler.ComputeMD5Hash("123456"));
        });

        /// <summary>
        /// 保存用户配置
        /// </summary>
        public DelegateCommand SaveUserSetsCommand => new DelegateCommand(SaveUserSets);

        /// <summary>
        /// 保存用户
        /// </summary>
        private void SaveUserSets()
        {
            List<SysUserEntity> userList = UserList.Select(u => new SysUserEntity
            {
                Account = u.Account,
                Password = string.IsNullOrEmpty(u.Password) ? Md5Hepler.ComputeMD5Hash("123456") : u.Password,
                Role = u.Role,
                RealName = u.RealName,
                Gender = u.Gender,
                Phone = string.IsNullOrEmpty(u.Phone) ? "" : u.Phone,
                Department = u.Department,
            }).ToList();
            _dataAccess.SaveUserSets(userList);

            MessageBox.Show("保存用户成功");

            InitUsers();//重新初始化用户
        }
        #endregion

        #region 配置
        /// <summary>
        /// 主题和字
        /// </summary>
        public class ThemeOption
        {
            public string Label { get; set; }
            public string Value { get; set; }
            public Brush PreviewBackground { get; set; }
            public Brush PreviewForeground { get; set; }
            public ThemeOption(string label,string value,Brush bg,Brush fg) 
            { 
                Label = label; Value = value; 
                PreviewBackground = bg; PreviewForeground=fg; 
            }
        }

        /// <summary>
        /// 填充主题实例选项
        /// </summary>
        public List<ThemeOption> ThemeOptions { get; } = new List<ThemeOption>()
        {
             new ThemeOption("白底黑字", "",Brushes.White, Brushes.Black),
             new ThemeOption("黑底白字", "Dark",Brushes.Black, Brushes.White),
        };

        /// <summary>
        /// 保存命令
        /// </summary>
        public DelegateCommand SaveSettingCommand => new DelegateCommand(() =>
        {
            _dataAccess.SaveSetting(new SettingEntity { Id = 1, Theme = Setting.Theme });
            MessageBox.Show("保存成功");
        });
        #endregion
    }
}
