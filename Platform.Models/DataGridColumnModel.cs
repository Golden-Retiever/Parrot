using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Models
{
    /// <summary>
    /// DataGrid显示列model
    /// </summary>
    public class DataGridColumnModel// : BindableBase
    {
        /// <summary>
        /// 实现选择
        /// </summary>
        public bool IsSelected { get; set; }

        ///// <summary>
        ///// 索引(顺序)
        ///// </summary>
        //public int Index { get; set; }

        /// <summary>
        /// 显示文字
        /// </summary>
        public string Header { get; set; }

        /// <summary>
        /// 统计模型里的绑定路径(属性)
        /// </summary>
        public string BindingPath { get; set; }

        /// <summary>
        /// 列宽
        /// </summary>
        public int ColumnWidth { get; set; }
    }
}
