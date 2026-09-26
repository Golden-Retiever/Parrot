
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Models
{
    /// <summary>
    /// 组件类型
    /// </summary>
    public class ComponentTypeModel
    {

        /// <summary>
        /// 组件类型名称(比如设备、管道)
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// 类型里面的组件集合
        /// </summary>
        public List<ComponentModel> Children { get; set; }
    }
}
