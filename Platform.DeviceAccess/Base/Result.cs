using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.DeviceAccess.Base
{
    /// <summary>
    /// 同一返回结果类。没有数据
    /// </summary>
    public class Result
    {
        /// <summary>
        /// 状态。true表示正常状态/没有错，false表示有错/不正常
        /// </summary>
        public bool Status { get; set; } = false;

        /// <summary>
        /// 错误信息
        /// </summary>
        public string Msg { get; set; } = "";
    }
}
