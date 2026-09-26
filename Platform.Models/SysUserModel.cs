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

        /// <summary>
        /// 是否管理员
        /// </summary>
        public bool IsAdmin { get; set; }

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
