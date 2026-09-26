using Platform.DataEntities;
using Platform.DBAccess;
using Platform.Helper;
using Platform.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Platform.ViewModels.FunctionUC
{
    /// <summary>
    /// 配置视图模型
    /// </summary>
    public class SettingsUCViewModel : BindableBase
    {
        IDataAccess _dataAccess;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataAccess"></param>
        /// <param name="mainUCViewModel">因为需要设备集合。</param>
        public SettingsUCViewModel(IDataAccess dataAccess, MainUCViewModel mainUCViewModel)
        {
            _dataAccess = dataAccess;

            //可以选择的设备集合(必须要有变量)
            CanSelectedList = mainUCViewModel.DeviceList.Where(d => d.DeviceVarList.Count > 0).ToList();

            InitMonitorSetting();//初始化监控配置

            InitUsers();//初始化用户信息
        }

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

        #region 用户管理
        /// <summary>
        /// 用户集合
        /// </summary>
        public ObservableCollection<SysUserModel> UserList { get; set; } = new ObservableCollection<SysUserModel>();

        /// <summary>
        /// 用户类型集合
        /// </summary>
        public List<UserTypeModel> UserTypeList => [
            new UserTypeModel {  IsAdmin=false, TypeName="非管理员"},
            new UserTypeModel {  IsAdmin=true, TypeName="管理员"}
            ];

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
                    IsAdmin = user.IsAdmin,
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
        /// 删除用户
        /// </summary>
        public DelegateCommand<SysUserModel> DelUserCommand => new DelegateCommand<SysUserModel>((userModel) =>
        {
            UserList.Remove(userModel);
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
                IsAdmin = u.IsAdmin,
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
    }
}
