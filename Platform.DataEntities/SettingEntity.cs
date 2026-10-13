using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.DataEntities
{
    /// <summary>
    /// 配置
    /// </summary>
    [Table("Setting")]
    public class SettingEntity
    {
        /// <summary>
        /// 
        /// </summary>
        [Key]
        public int Id {  get; set; }

        /// <summary>
        /// 主题
        /// </summary>
        public string? Theme {  get; set; }

        /// <summary>
        /// 语言
        /// </summary>
        public string? Language {  get; set; }

        /// <summary>
        /// 背景
        /// </summary>
        public string? Background {  get; set; }
    }
}
