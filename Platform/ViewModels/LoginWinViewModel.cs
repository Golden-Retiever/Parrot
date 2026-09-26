using Platform.DataEntities;
using Platform.DBAccess;
using Platform.Helper;
using Platform.Models;
using Platform.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Platform.ViewModels
{
    /// <summary>
    /// 登录视图模型
    /// </summary>
    public class LoginWinViewModel : BindableBase
    {
        private IDataAccess _dataAccess;//数据访问实例
        private IDialogService _dialogService;//对话框服务
        public LoginWinViewModel(IDataAccess dataAccess,IDialogService dialogService)
        {
            _dataAccess = dataAccess;//数据访问实例
            _dialogService= dialogService;
        }

        /// <summary>
        /// 登录用户
        /// </summary>
        public SysUserModel SysUser { get; set; } = new SysUserModel() { Account="黄渤",Password="123"};

        #region 登录结果
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

        #region 登录
        public DelegateCommand<Window> LoginCommand => new DelegateCommand<Window>(DoLogin);

        /// <summary>
        /// 执行登录
        /// </summary>
        /// <param name="loginWin">登录窗体</param>
        private void DoLogin(Window loginWin)
        {
            if (string.IsNullOrEmpty(SysUser.Account) || string.IsNullOrEmpty(SysUser.Password))
            {
                LoginErrorMsg = "账号和密码不能为空";
                return;
            }

            string md5Pwd = Md5Hepler.ComputeMD5Hash(SysUser.Password);//md5处理
            var loginUserEntity = _dataAccess.Login(SysUser.Account, md5Pwd);
            if (loginUserEntity == null)
            {
                LoginErrorMsg = "账号或密码错误";
                return;
            }

            loginWin.Hide();//隐藏登录窗体

            //登录成功跳转到主界面 使用dialogservice  并且将登录用户信息传给主界面
            DialogParameters dialogParas = new DialogParameters();
            //将登录信息传给主界面
            dialogParas.Add("LoginUser", new SysUserModel
            {
                UserId = loginUserEntity.UserId,
                RealName = loginUserEntity.RealName,
                Department = loginUserEntity.Department,
                Account = loginUserEntity.Account,
                IsAdmin = loginUserEntity.IsAdmin
            });

            _dialogService.ShowDialog("MainUC", dialogParas);
        }
        #endregion

        #region 注册
        public DelegateCommand<Window> RegisterCommand =>new DelegateCommand<Window>(DoRegister);

        private void DoRegister(Window loginWin)
        {
            loginWin.Hide();
            bool success = ActionHelper.ExecuteAndResult<object>("Register", null);

            if (success && RegisterWin.RegisteredUser != null)
            {
                SysUser.Account = RegisterWin.RegisteredUser.Account;
                SysUser.Password = RegisterWin.RegisteredUser.Password;
                RegisterWin.RegisteredUser = null; // 清空，避免下次误用
            }

            loginWin.Show();
        }
        #endregion
    }
}
