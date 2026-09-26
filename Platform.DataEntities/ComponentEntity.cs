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
    /// 组件表
    /// </summary>
    [Table("Components")]
    public class ComponentEntity
    {
        /// <summary>
        /// 组件ID 主键 自增
        /// </summary>
        [Key]
        public int ComponentId { get; set; }

        /// <summary>
        /// 组件名称(空压机、冷冻式干燥机等)
        /// </summary>
        public string ComponentName { get; set; }

        /// <summary>
        /// 组件类型
        /// </summary>
        public string ComponentType { get; set; }

        /// <summary>
        /// 组件图标文件名
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// 类型名称(设备、数字仪表、管道等)
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// 默认宽
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// 默认高
        /// </summary>
        public double Height { get; set; }
    }
}
