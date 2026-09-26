using Platform.DataEntities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Models
{
    /// <summary>
    /// 监控配置模型
    /// </summary>
    public class MonitorSettingsModel : BindableBase
    {
        /// <summary>
        /// 配置编号
        /// </summary>
        public MonitorSettingNumEnum SettingNum { get; set; }

        /// <summary>
        /// 配置标题/头
        /// </summary>
        public string Header { get; set; }

        /// <summary>
        /// 配置描述
        /// </summary>
        public string? Description { get; set; }

        private string _deviceNum;

        /// <summary>
        /// 设备编号
        /// </summary>
        public string DeviceNum
        {
            get { return _deviceNum; }
            set
            {
                _deviceNum = value;

                #region 设备和设备变量二级联动
               
                if (string.IsNullOrEmpty(value))
                {
                    return;
                }

                // 动态的可选变量列表
                VarNum = "";
                VarList.Clear();

                var device = DeviceList.FirstOrDefault(d => d.DeviceNum == value);
                if (device == null)
                {
                    return;
                }

                device.DeviceVarList.ToList().ForEach(v => VarList.Add(v));

                if (VarList.Count > 0)
                {
                    VarNum = VarList[0].VarNum;//默认选择第一个变量
                }

                #endregion
            }
        }

        private string _varNum;

        /// <summary>
        /// 选择的变量编号
        /// </summary>
        public string VarNum
        {
            get { return _varNum; }
            set { SetProperty(ref _varNum, value); }
        }

        /// <summary>
        /// 设备集合
        /// </summary>
        public List<DeviceModel> DeviceList { get; set; }

        /// <summary>
        /// 当前选择的设备的变量集合
        /// </summary>
        public ObservableCollection<DeviceVarModel> VarList { get; set; } = new ObservableCollection<DeviceVarModel>();
    }
}
