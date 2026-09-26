using DryIoc;
using Platform.DataEntities;
using Platform.DBAccess;
using Platform.Helper;
using Platform.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Platform.ViewModels.ComponentWin
{
    /// <summary>
    /// 链路编辑视图模型
    /// </summary>
    public class ComponentEditWinViewModel : BindableBase
    {
        private IDataAccess _dataAccess;//数据库访问
        public ComponentEditWinViewModel(IDataAccess dataAccess)
        {
            _dataAccess = dataAccess;

            InitComponents();//初始化组件

            InitHadDevices();//初始化已有设备

            PropEntityList = dataAccess.GetPropList();//获取数据库中所有属性

        }

        #region 组件

        /// <summary>
        /// 组件类型集合
        /// </summary>
        public List<ComponentTypeModel> ComponentTypeList { get; set; }

        /// <summary>
        /// 初始化组件
        /// </summary>
        private void InitComponents()
        {
            var compoentList = _dataAccess.GetComponents();

            ComponentTypeList = compoentList.GroupBy(c => c.TypeName).Select(
                comEntity => new ComponentTypeModel
                {
                    TypeName = comEntity.Key,
                    Children = comEntity.Select(com => new ComponentModel
                    {
                        ComponentId = com.ComponentId,
                        Icon = "pack://application:,,,/Platform.CommonResource;component/Images/Components/" + com.Icon,
                        ComponentName = com.ComponentName,
                        ComponentType = com.ComponentType,
                        Width = com.Width,
                        Height = com.Height
                    }).ToList()
                }).ToList();
        }
        #endregion

        #region 设备

        /// <summary>
        /// 设备集合
        /// </summary>
        public ObservableCollection<DeviceModel> DeviceList { get; set; } = new ObservableCollection<DeviceModel>();

        #region 组件拖拽松开
        public DelegateCommand<object> DeviceDropCommand => new DelegateCommand<object>(DoDeviceDrop);

        private void DoDeviceDrop(object obj)
        {
            DragEventArgs e = obj as DragEventArgs;
            var componentData = (ComponentModel)e.Data.GetData(typeof(ComponentModel));

            Point point = e.GetPosition((IInputElement)e.Source);//鼠标位置
            DeviceModel dropDevice = new DeviceModel
            {
                ComponentId = componentData.ComponentId,
                ComponentType = componentData.ComponentType,
                DeviceName = componentData.ComponentName,
                DeviceNum = $"D{DateTime.Now.ToString("yyyyMMddHHmmssFFF")}",//自动生成设备编号
                Width = componentData.Width,
                Height = componentData.Height,
                X = point.X,
                Y = point.Y,
                Z = 2,//默认最上层

                //删除设备命令
                DeleteCommand = new DelegateCommand<DeviceModel>((device) => { DeviceList.Remove(device); }),
                GetAllDeviceFunc = (() => { return DeviceList.ToList(); }),//传一个返回所有设备的方法
            };

            //初始化设备右键功能
            dropDevice.InitContextMenu();

            DeviceList.Add(dropDevice);//添加到设备集合
        }
        #endregion

        #region 初始化设备

        private void InitHadDevices()
        {
            //DeviceList.Add(new DeviceModel { ComponentId = 1, ComponentType = "AirCompressorUC", DeviceName = "空压机", DeviceNum = "D001", Width = 120, Height = 80, X = 80, Y = 160, Z = 2 });

            //DeviceList.Add(new DeviceModel { ComponentId = 1, ComponentType = "HorizontalPipelineUC", DeviceName = "水平管道", DeviceNum = "D002", Width = 200, Height = 80, X = 488, Y = 150, Z = 2 });

            var deviceEntityList = _dataAccess.GetDevices();//获取所有设备
            var devicePropEntityList = _dataAccess.GetDeviceProps();//获取所有设备属性
            var deviceVarEntityList = _dataAccess.GetDeviceVarList();//获取所有设备变量
            var manControlList = _dataAccess.GetManualControlList();//获取所有手动控制
            var varAlarmConfList = _dataAccess.GetVarAlarmConfList();//获取所有变量报警配置

            var deviceList = deviceEntityList.Select(d => new DeviceModel
            {
                ComponentId = d.ComponentId,
                ComponentType = d.ComponentType,
                DeviceName = d.DeviceName,
                DeviceNum = d.DeviceNum,
                Width = d.Width,
                Height = d.Height,
                X = d.X,
                Y = d.Y,
                Z = d.Z,
                FlowDirection = d.FlowDirection,
                Rotate = d.Rotate,

                //获取该设备属性
                DevicePropList = new ObservableCollection<DevicePropModel>(devicePropEntityList.Where(dp => dp.DeviceNum == d.DeviceNum).Select(dp => new DevicePropModel { PropName = dp.PropName, PropValue = dp.PropValue })),

                //获取该设备变量
                DeviceVarList = new ObservableCollection<DeviceVarModel>(deviceVarEntityList.Where(dv => dv.DeviceNum == d.DeviceNum).Select(dv => new DeviceVarModel
                {
                    DeviceNum = dv.DeviceNum,
                    DeviceName = d.DeviceName,
                    VarNum = dv.VarNum,
                    VarName = dv.VarName,
                    VarAddress = dv.VarAddress,
                    Offset = dv.Offset,
                    Modulus = dv.Modulus,
                    VarType = dv.VarType,

                    //取出该变量的报警配置
                    VarAlarmConfList = new ObservableCollection<VarAlarmConfModel>(varAlarmConfList.Where(vac => vac.VarNum == dv.VarNum).Select(vac => new VarAlarmConfModel
                    {
                        ConfNum = vac.ConfNum,
                        Operator = vac.Operator,
                        CompareValue = vac.CompareValue,
                        AlarmContent = vac.AlarmContent,
                    }))
                })),

                //获取该设备的手动控制
                ManualControlList = new ObservableCollection<ManualControlModel>(manControlList.Where(mc => mc.DeviceNum == d.DeviceNum).Select(mc => new ManualControlModel
                {
                    ControlName = mc.ControlName,
                    ControlAddress = mc.ControlAddress,
                    ControlValue = mc.ControlValue,
                })),

                //删除设备命令
                DeleteCommand = new DelegateCommand<DeviceModel>((device) => { DeviceList.Remove(device); }),
                GetAllDeviceFunc = (() => { return DeviceList.ToList(); }),//传一个返回所有设备的方法
            });

            DeviceList = new ObservableCollection<DeviceModel>(deviceList);

            //初始化设备右键功能
            foreach (var device in DeviceList)
            {
                device.InitContextMenu();
            }

            // 水平/垂直对齐线
            DeviceList.Add(new DeviceModel() { X = 0, Y = 0, Width = 2000, Height = 0.5, Z = 2, ComponentType = "HL", IsVisible = false });//水平对齐线 默认不显示
            DeviceList.Add(new DeviceModel() { X = 0, Y = 0, Width = 0.5, Height = 2000, Z = 2, ComponentType = "VL", IsVisible = false });//垂直对齐线 默认不显示 

            // 宽度/高度对齐线
            DeviceList.Add(new DeviceModel() { X = 0, Y = 0, Width = 0, Height = 15, Z = 2, ComponentType = "WidthRule", IsVisible = false });
            DeviceList.Add(new DeviceModel() { X = 0, Y = 0, Width = 15, Height = 0, Z = 2, ComponentType = "HeightRule", IsVisible = false });
        }
        #endregion

        #region 保存设备到数据库

        private string _saveFailedMessage;

        /// <summary>
        /// 保存错误提示
        /// </summary>
        public string SaveFailedMessage
        {
            get { return _saveFailedMessage; }
            set { SetProperty(ref _saveFailedMessage, value); }
        }

        public DelegateCommand<object> SaveDevicesCommand => new DelegateCommand<object>(DoSaveDevices);//保存设备命令

        //关闭错误提示命令
        public DelegateCommand<object> CloseSaveFailedCommand => new DelegateCommand<object>(obj =>
        {
            VisualStateManager.GoToElementState(obj as Window, "SaveFailedClose", true);
        });

        /// <summary>
        /// 执行保存
        /// </summary>
        /// <param name="obj">操作的window</param>
        private void DoSaveDevices(object obj)
        {
            //还原到保存初始化状态
            VisualStateManager.GoToElementState(obj as Window, "NormalSuccess", true);//布局控件状态管理 border、grid等

            VisualStateManager.GoToElementState(obj as Window, "SaveFailedNormal", true);

            #region 校验:属性行必须填了值(DeviceProps.PropValue 是 NOT NULL,空值会让保存失败)
            string emptyPropTip = null;
            foreach (DeviceModel deviceInfo in DeviceList)
            {
                foreach (DevicePropModel devicePropInfo in deviceInfo.DevicePropList)
                {
                    if (string.IsNullOrWhiteSpace(devicePropInfo.PropValue))
                    {
                        emptyPropTip = $"设备 {deviceInfo.DeviceName} 的属性「{devicePropInfo.PropName}」未填写值";
                        break;
                    }
                }
                if (emptyPropTip != null) break;
            }

            if (emptyPropTip != null)
            {
                SaveFailedMessage = "保存设备失败：" + emptyPropTip;
                VisualStateManager.GoToElementState(obj as Window, "SaveFailedShow", true);
                return;
            }
            #endregion

            // 保存到数据库的时候，要排除水平/垂直对齐线，还要排除宽高对齐线
            var deviceEntityList = DeviceList.Where(d => !new string[] { "HL", "VL", "WidthRule", "HeightRule" }.Contains(d.ComponentType)).Select(d => new DeviceEntity
            {
                DeviceNum = d.DeviceNum,
                ComponentId = d.ComponentId,
                DeviceName = d.DeviceName,
                ComponentType = d.ComponentType,
                X = d.X,
                Y = d.Y,
                Z = d.Z,
                Width = d.Width,
                Height = d.Height,
                FlowDirection = d.FlowDirection,
                Rotate = d.Rotate,
            }).ToList();

            #region 保存设备属性、设备变量
            List<DevicePropEntity> devicePropsList = new List<DevicePropEntity>();//要保存到db的设备属性
            List<DeviceVarEntity> deviceVarsList = new List<DeviceVarEntity>();//要保存到db的设备变量
            List<ManualControlEntity> manualControlList = new List<ManualControlEntity>();//要保存到db的手动控制
            List<VarAlarmConfEntity> varAlarmConfList = new List<VarAlarmConfEntity>();//要保存到db的变量报警配置

            foreach (DeviceModel deviceInfo in DeviceList)
            {
                //将每个设备属性DevicePropModel转成DevicePropEntity
                foreach (DevicePropModel devicePropInfo in deviceInfo.DevicePropList)
                {
                    devicePropsList.Add(new DevicePropEntity { DeviceNum = deviceInfo.DeviceNum, PropName = devicePropInfo.PropName, PropValue = devicePropInfo.PropValue });
                }

                //将每个设备变量DeviceVarModel转成DeviceVarEntity
                foreach (DeviceVarModel deviceVarInfo in deviceInfo.DeviceVarList)
                {
                    deviceVarsList.Add(new DeviceVarEntity
                    {
                        VarNum = deviceVarInfo.VarNum,
                        DeviceNum = deviceInfo.DeviceNum,
                        VarName = deviceVarInfo.VarName,
                        VarAddress = deviceVarInfo.VarAddress,
                        Offset = deviceVarInfo.Offset,
                        VarType = deviceVarInfo.VarType,
                        Modulus = deviceVarInfo.Modulus
                    });

                    //将变量的报警配置加到varAlarmConfList
                    foreach (VarAlarmConfModel varAlarmConfModel in deviceVarInfo.VarAlarmConfList)
                    {
                        varAlarmConfList.Add(new VarAlarmConfEntity
                        {
                            ConfNum = varAlarmConfModel.ConfNum,
                            VarNum = deviceVarInfo.VarNum,
                            Operator = varAlarmConfModel.Operator,
                            CompareValue = varAlarmConfModel.CompareValue,
                            AlarmContent = varAlarmConfModel.AlarmContent,
                        });
                    }
                }

                //将每个设备的ManualControlModel转成ManualControlEntity
                foreach (ManualControlModel manualControlModel in deviceInfo.ManualControlList)
                {
                    manualControlList.Add(new ManualControlEntity
                    {
                        DeviceNum = deviceInfo.DeviceNum,
                        ControlName = manualControlModel.ControlName,
                        ControlAddress = manualControlModel.ControlAddress,
                        ControlValue = manualControlModel.ControlValue
                    });
                }
            }
            #endregion

            bool isSaveSuccess = _dataAccess.SaveDevice(deviceEntityList, devicePropsList, deviceVarsList, manualControlList, varAlarmConfList);

            DilogResultVal = isSaveSuccess;//保存成功以后将DialogResult设置为true
            if (isSaveSuccess)
            {
                VisualStateManager.GoToElementState(obj as Window, "SaveSuccess", true);//布局控件状态管理 border、grid等
            }
            else
            {
                SaveFailedMessage = "保存设备失败：" + ("未知错误");
                VisualStateManager.GoToElementState(obj as Window, "SaveFailedShow", true);//布局控件状态管理 border、grid等
            }
        }
        #endregion

        #region 选中设备
        //public DeviceModel SelectedDevice { get; set; }//选中的设备

        private DeviceModel _selectedDevice;

        /// <summary>
        /// 选中的设备
        /// </summary>
        public DeviceModel SelectedDevice
        {
            get { return _selectedDevice; }
            set
            {
                SetProperty(ref _selectedDevice, value);
            }
        }


        public DelegateCommand<DeviceModel> SelectDeviceCommand => new DelegateCommand<DeviceModel>(DoSelectDevice);

        /// <summary>
        /// 选中设备
        /// </summary>
        /// <param name="model"></param>
        private void DoSelectDevice(DeviceModel model)
        {
            //1、将之前选中的，设置为不选中
            if (SelectedDevice != null)
            {
                SelectedDevice.IsSelected = false;
            }

            //2、将现在点击的设备设置为已选中
            if (model != null)
            {
                model.IsSelected = true;
            }

            //3、将选中的设备设置现在点击的设备
            SelectedDevice = model;

            InitProps();//初始化属性
        }

        #endregion

        #region 键盘移动设备和取消选中
        public DelegateCommand<object> KeyMoveDeviceCommand => new DelegateCommand<object>((obj) =>
        {
            if (SelectedDevice != null)
            {
                var kea = obj as KeyEventArgs;
                if (kea != null)
                {
                    if (kea.Key == Key.Up)//向上
                    {
                        SelectedDevice.Y--;
                    }

                    if (kea.Key == Key.Down)//向上
                    {
                        SelectedDevice.Y++;
                    }

                    if (kea.Key == Key.Left)//向左
                    {
                        SelectedDevice.X--;
                    }

                    if (kea.Key == Key.Right)//向右
                    {
                        SelectedDevice.X++;
                    }

                    if (kea.Key == Key.Escape)//esc取消选中设备
                    {
                        SelectedDevice.IsSelected = false;
                    }
                }
            }
        });
        #endregion

        #endregion

        #region 退出

        private bool DilogResultVal = false;//DialogResult

        public DelegateCommand<Window> CloseCommand => new DelegateCommand<Window>((win) =>
        {
            win.DialogResult = DilogResultVal;

        });
        #endregion

        #region 属性

        /// <summary>
        /// 变量数据类型选项(绑定到变量编辑Popup里的数据类型下拉框)
        /// </summary>
        public List<string> TypeList { get; } = new List<string>
        {
            "Boolean", "Int16", "UInt16", "Int32", "UInt32", "Int64", "UInt64", "Single", "Double"
        };

        private List<PropEntity> PropEntityList;//数据库中所有属性

        /// <summary>
        /// 属性集合
        /// </summary>
        public ObservableCollection<PropModel> PropList { get; set; }

        /// <summary>
        /// 初始化所有属性
        /// </summary>
        private void InitProps()
        {

            if (PropEntityList != null && PropEntityList.Count > 0)
            {
                var propList = PropEntityList.Select(p =>
                {
                    var pom = new PropModel
                    {
                        PropTitle = p.PropTitle,
                        PropName = p.PropName,
                        PropType = p.PropType
                    };
                    // 1、初始化当前属性选项所对应的值的选项
                    // 2、希望加载值选项后，初始化一个默认选项
                    var list = InitPropOptions(p.PropName, out int OptionsSelectedIndex);
                    pom.ValueOptions = list;
                    pom.OptionsSelectedIndex = OptionsSelectedIndex;

                    return pom;
                });

                PropList = new ObservableCollection<PropModel>(propList);
            }
        }

        /// <summary>
        /// 初始化属性选项
        /// </summary>
        /// <param name="propName">属性名</param>
        /// <param name="OptionsSelectedIndex">输出参数。默认选择的索引</param>
        /// <returns></returns>
        private List<string> InitPropOptions(string propName, out int OptionsSelectedIndex)
        {
            List<string> values = new List<string>();
            OptionsSelectedIndex = 0;//默认选择的索引
            switch (propName)
            {
                case "Protocol":
                    values.Add("ModbusRTU");
                    values.Add("ModbusTCP");
                    values.Add("MQTT");
                    break;
                case "PortName":
                    values = SerialPort.GetPortNames().ToList();
                    break;
                case "BaudRate":
                    values.Add("2400");
                    values.Add("4800");
                    values.Add("9600");
                    values.Add("19200");
                    values.Add("38400");
                    values.Add("57600");
                    values.Add("115200");

                    OptionsSelectedIndex = 2;
                    break;
                case "DataBit":
                    values.Add("5");
                    values.Add("7");
                    values.Add("8");

                    OptionsSelectedIndex = 2;
                    break;
                case "Parity":
                    values = Enum.GetNames<Parity>().ToList();
                    break;
                case "StopBit":
                    values = Enum.GetNames<StopBits>().ToList();
                    OptionsSelectedIndex = 1;
                    break;
                case "DeviceEndian":
                    values.Add("Big");
                    values.Add("Little");
                    break;
                case "WordOrder":
                    values.Add("HighFirst");
                    values.Add("LowFirst");
                    break;
                default: break;
            }

            //找到默认索引 解决从db里取出后，设置选择哪一个
            if (SelectedDevice != null && SelectedDevice.DevicePropList != null)
            {
                foreach (var propInfo in SelectedDevice.DevicePropList)
                {
                    if (propInfo.PropName == propName)
                    {
                        OptionsSelectedIndex = values.IndexOf(propInfo.PropValue);//找属性值
                        break;
                    }
                }
            }

            return values;
        }

        #endregion

        #region 打开变量报警配置
        public DelegateCommand AlarmConfCommand => new DelegateCommand(() =>
        {
            if (SelectedDevice != null)//一定要选中了一个设备
            {
                ActionHelper.Execute("AlarmConf", SelectedDevice);
            }
        });
        #endregion
    }
}
