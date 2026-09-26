using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.DeviceAccess.Base
{
    /// <summary>
    /// 分组地址(基类)
    /// </summary>
    public class GroupAddress
    {
        /// <summary>
        /// 该组包含哪些变量
        /// </summary>
        public List<VariableProp> VarPropList { get; set; } = new List<VariableProp>();
    }
}
