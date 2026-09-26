using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.DeviceAccess.Base
{
    /// <summary>
    /// 分组后的modbus地址
    /// </summary>
    internal class ModbusGroupAddress:GroupAddress
    {
        /// <summary>
        /// 功能码
        /// </summary>
        public int FuncCode { get; set; }

        /// <summary>
        /// 起始地址(相对地址)
        /// </summary>
        public int StartAddress { get; set; }

        /// <summary>
        /// 寄存器个数/线圈
        /// </summary>
        public int Count { get; set; }
    }
}
