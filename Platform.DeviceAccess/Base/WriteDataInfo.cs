using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.DeviceAccess.Base
{
    /// <summary>
    /// 写数据信息
    /// </summary>
    public  class WriteDataInfo
    {
        /// <summary>
        /// 起始地址
        /// </summary>
        public string StartAddr { get; set; }

        /// <summary>
        /// 数据类型
        /// </summary>
        public Type ValueType { get; set; }

        /// <summary>
        /// 写进去的字节数组
        /// </summary>
        public byte[] WriteBytes { get; set; }

        /// <summary>
        /// 值个数(前端明确指定;<=0 时协议层才按字节数反推)
        /// </summary>
        public int Count { get; set; }
    }
}
