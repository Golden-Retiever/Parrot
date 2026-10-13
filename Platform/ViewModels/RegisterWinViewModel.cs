using NPOI.OpenXmlFormats.Dml;
using Platform.DataEntities;
using Platform.DBAccess;
using Platform.Helper;
using Platform.Models;
using Platform.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Platform.ViewModels
{
    public class RegisterWinViewModel: BindableBase
    {
        private IDataAccess _dataAccess;//数据访问实例
        private IDialogService _dialogService;//对话框服务

        public SysUserModel SysUser { get; set; } = new SysUserModel();

        public RegisterWinViewModel(IDataAccess dataAccess, IDialogService dialogService)
        {
            _dataAccess = dataAccess;//数据访问实例
            _dialogService = dialogService;
            _dbAdminCmd = _dataAccess.GetAdminCommand();//口令取自库,注册时比对
            tempPwd = "";
            SysUser.Password = "";
        }

        #region 错误信息
        private string _loginErrorMsg;
        public string LoginErrorMsg
        {
            get { return _loginErrorMsg; }
            set
            {
                SetProperty(ref _loginErrorMsg, value);//通知
            }
        }
        #endregion

        #region 再次输入密码
        private string _tempPwd;
        public string tempPwd
        {
            get { return _tempPwd; }
            set {  SetProperty(ref _tempPwd, value);}
        }
        #endregion 

        #region 管理员注册
        //管理员口令:取自库(AdminCommand表),注册时输入对上才授管理员
        private readonly string _dbAdminCmd;

        private string _adminCmd;
        /// <summary>
        /// 管理员命令
        /// </summary>
        public string AdminCmd
        {
            get { return _adminCmd; }
            set { SetProperty(ref _adminCmd, value); }
        }
        #endregion

        #region 执行注册
        public DelegateCommand<Window> LoginCommand => new DelegateCommand<Window>(DoLogin);

        /// <summary>
        /// 执行注册
        /// </summary>
        /// <param name="loginWin">注册窗体</param>
        private void DoLogin(Window loginWin)
        {
            if (string.IsNullOrEmpty(SysUser.Account) || string.IsNullOrEmpty(SysUser.RealName) || string.IsNullOrEmpty(tempPwd) || string.IsNullOrEmpty(SysUser.Password) 
                || string.IsNullOrEmpty(SysUser.Department) || string.IsNullOrEmpty(SysUser.Gender) || string.IsNullOrEmpty(SysUser.Phone))
            {
                LoginErrorMsg = "信息不全";
                return;
            }

            if (!tempPwd.Equals(SysUser.Password))
            {
                LoginErrorMsg = "密码输入不一致";
                return;
            }

            //检查是否已注册
            var exit=_dataAccess.GetSysUserList().FirstOrDefault(u=>u.Account == SysUser.Account);
            if (exit != null)
            {
                LoginErrorMsg = "该账号已被注册";
                return;
            }

            //管理员命令检测:为空=默认非管理员;填了须与命令串一致,不一致底部报错不写库
            bool isAdmin = false;
            if (!string.IsNullOrEmpty(AdminCmd))
            {
                if (AdminCmd != _dbAdminCmd)
                {
                    LoginErrorMsg = "管理员命令错误";
                    return;
                }
                isAdmin = true;
            }

            string md5Pwd = Md5Hepler.ComputeMD5Hash(SysUser.Password);//md5处理
            var newUser = new SysUserEntity
            {
                Account = SysUser.Account,
                RealName = SysUser.RealName,
                Password = md5Pwd,
                Department = SysUser.Department,
                Gender = SysUser.Gender,
                Phone = SysUser.Phone,
                Role = isAdmin ? 2 : 0//口令对上=一级管理员,否则普通
            };

            try
            {
                _dataAccess.AddUser(newUser);

                RegisterWin.RegisteredUser = new SysUserModel
                {
                    Account = SysUser.Account,
                    Password = SysUser.Password
                };

                //注册成功跳转到登陆界面
                loginWin.DialogResult = true;
                loginWin.Close();
            }
            catch (Exception ex)
            {
                LoginErrorMsg = "注册失败"; return;
            }
            #endregion 
        }
    }
}
