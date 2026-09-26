using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Models
{
    /// <summary>
    /// 趋势-设备变量模型
    /// </summary>
    public class TrendDeviceVarModel : BindableBase
    {
        #region 基本属性

        private bool _isSelected;

        /// <summary>
        /// 是否选择
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                SetProperty(ref _isSelected, value);
            }
        }

        /// <summary>
        /// 设备编号
        /// </summary>
        public string DeviceNum { get; set; }

        /// <summary>
        /// 变量编号
        /// </summary>
        public string VarNum { get; set; }

        /// <summary>
        /// 变量名称
        /// </summary>
        public string VarName { get; set; }

        /// <summary>
        /// 变量类型
        /// </summary>
        public string VarType { get; set; }

        private string _aNum;

        /// <summary>
        /// 纵轴编号
        /// </summary>
        public string ANum
        {
            get { return _aNum; }
            set { SetProperty(ref _aNum, value); }
        }

        private string _color;

        /// <summary>
        /// 颜色
        /// </summary>
        public string Color
        {
            get { return _color; }
            set { SetProperty(ref _color, value); }
        }

        #endregion
    }
}
