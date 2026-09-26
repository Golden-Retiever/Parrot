using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Models
{
    /// <summary>
    /// 设备变量信息
    /// </summary>
    public class DeviceVarModel : BindableBase
    {
        /// <summary>
        /// 变量唯一编码
        /// </summary>
        public string VarNum { get; set; }

        /// <summary>
        /// 设备编号
        /// </summary>
        public string DeviceNum { get; set; }

        /// <summary>
        /// 设备名称
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// 变量名称
        /// </summary>
        public string VarName { get; set; }

        /// <summary>
        /// 变量地址
        /// </summary>
        public string VarAddress { get; set; }

        //从设备读取的数据
        private object _readValue = 0;

        /// <summary>
        /// 从设备读取的数据
        /// </summary>
        public object ReadValue
        {
            get { return _readValue; }
            set
            {
                SetProperty(ref _readValue, value);
            }
        }

        /// <summary>
        /// 偏移量
        /// </summary>
        public double Offset { get; set; }

        /// <summary>
        /// 系数
        /// </summary>
        public double Modulus { get; set; } = 1;

        /// <summary>
        /// 变量数据类型
        /// </summary>
        public string VarType { get; set; } = "UInt16";

        #region 变量报警
        public ObservableCollection<VarAlarmConfModel> VarAlarmConfList { get; set; } = new ObservableCollection<VarAlarmConfModel>();

        public DelegateCommand AddConfCommand => new DelegateCommand(() =>
        {
            VarAlarmConfList.Add(new VarAlarmConfModel() { Operator = "<", ConfNum = "C" + DateTime.Now.ToString("yyyyMMddHHmmssFFF") });
        });

        public DelegateCommand<VarAlarmConfModel> DeleteConfCommand => new DelegateCommand<VarAlarmConfModel>(model =>
        {
            VarAlarmConfList.Remove(model);
        });

        #endregion
    }
}
