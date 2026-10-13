using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Models
{
    /// <summary>
    /// 用户模型
    /// </summary>
    public class SysUserModel: BindableBase
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 登录账号
        /// </summary>
        private string _Account;
        public string Account 
        { 
            get { return _Account; }
            set 
            { 
                SetProperty(ref _Account, value);
            }
        }

        /// <summary>
        /// 密码(明文)
        /// </summary>
        private string _Password;
        public string Password
        {
            get { return _Password; }
            set
            {
                SetProperty(ref _Password, value);
            }
        }

        private int _role;

        /// <summary>
        /// 角色:0=普通,1=二级管理员,2=一级管理员
        /// </summary>
        public int Role
        {
            get { return _role; }
            set
            {
                if (SetProperty(ref _role, value))
                    RaisePropertyChanged(nameof(RoleText));//升降级后行内角色文本同步刷新
            }
        }

        /// <summary>
        /// 角色显示文本(配置页只读列)
        /// </summary>
        public string RoleText => Role switch { 2 => "一级管理员", 1 => "二级管理员", _ => "非管理员" };

        /// <summary>
        /// 真实姓名
        /// </summary>
        public string RealName { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        public string Gender { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// 部门
        /// </summary>
        public string Department { get; set; }
    }
}
