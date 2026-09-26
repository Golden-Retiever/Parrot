
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Platform.DataEntities
{
    /// <summary>
    /// 系统用户表
    /// </summary>
    [Table("SysUsers")]
    public class SysUserEntity
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        [Key]
        public int UserId { get; set; }

        /// <summary>
        /// 登录账号
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// 密码(存密文)
        /// </summary>
        public string Password { get; set; }

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
        public string Phone { get; set; } = "";

        /// <summary>
        /// 部门
        /// </summary>
        public string Department { get; set; }
    }

}
