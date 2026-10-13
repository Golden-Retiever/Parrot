using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Platform.DataEntities
{
    /// <summary>
    /// 管理员口令
    /// </summary>
    [Table("AdminCommand")]
    public class AdminCmdEntity
    {
        /// <summary>
        /// 编号
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 口令内容
        /// </summary>
        public string CommandText { get; set; }
    }
}
