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
    /// 设备变量表
    /// </summary>
    [Table("DeviceVars")]
    public class DeviceVarEntity
    {
        /// <summary>
        /// 变量编号
        /// </summary>
        [Key]
        public string VarNum { get; set; }

        /// <summary>
        /// 设备编号
        /// </summary>

        public string DeviceNum { get; set; }


        /// <summary>
        /// 变量名称
        /// </summary>
        public string VarName { get; set; }

        /// <summary>
        /// 变量地址
        /// </summary>
        public string VarAddress { get; set; }

        /// <summary>
        /// 变量类型
        /// </summary>
        public string VarType { get; set; }

        /// <summary>
        /// 偏移量
        /// </summary>
        public double Offset { get; set; }

        /// <summary>
        /// 系数
        /// </summary>
        public double Modulus { get; set; } = 1;
    }
}
