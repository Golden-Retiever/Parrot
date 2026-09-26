using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.DeviceAccess.Base
{
    /// <summary>
    /// 变量属性
    /// </summary>
    public class VariableProp
    {
        /// <summary>
        /// 变量属性唯一编码
        /// </summary>
        public string VarNum { get; set; }

        /// <summary>
        /// 起始地址(modbus是绝对地址，其他不一定)
        /// </summary>
        public string VarAddr { get; set; }

        /// <summary>
        /// 类型 (ushort、bool)
        /// </summary>
        public Type ValueType { get; set; }

        /// <summary>
        /// 读几个值
        /// </summary>
        public int Count { get; set; } = 1;

        /// <summary>
        /// 总字节数（=单值字节 * Count）
        /// </summary>
        public int ReadByteCount { get; set; }

        /// <summary>
        /// 读取的字节内容
        /// </summary>
        public byte[] ReadBytes { get; set; }

        /// <summary>
        /// 读取到的值（执行层已转好类型；count≤1 为单值，count>1 为 object[]；未读取前为 null）
        /// </summary>
        public object ReadValue { get; set; }
    }
}
