using Platform.DataEntities;
using Platform.DeviceAccess;
using Platform.DeviceAccess.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Platform.Models
{
    /// <summary>
    /// 设备模型
    /// </summary>
    public class DeviceModel : BindableBase
    {

        #region 基本属性

        /// <summary>
        /// 所属组件Id
        /// </summary>
        public int ComponentId { get; set; }

        /// <summary>
        /// 设备编号
        /// </summary>
        public string DeviceNum { get; set; }

        private double _x;

        /// <summary>
        /// 坐标X
        /// </summary>
        public double X
        {
            get { return _x; }
            set { SetProperty(ref _x, value); }
        }

        private double _y;
        /// <summary>
        /// 坐标Y
        /// </summary>
        public double Y
        {
            get { return _y; }
            set { SetProperty(ref _y, value); }
        }

        private double _z;
        /// <summary>
        /// 坐标Z
        /// </summary>
        public double Z
        {
            get { return _z; }
            set { SetProperty(ref _z, value); }
        }

        private double _width;
        /// <summary>
        /// 宽
        /// </summary>
        public double Width
        {
            get { return _width; }
            set { SetProperty(ref _width, value); }
        }

        private double _height;
        /// <summary>
        /// 高
        /// </summary>
        public double Height
        {
            get { return _height; }
            set { SetProperty(ref _height, value); }
        }

        /// <summary>
        /// 设备类型名称(AirCompressor等，与程序中控件名一致)
        /// </summary>
        public string ComponentType { get; set; }

        /// <summary>
        /// 设备名称(空压机、冷冻式干燥机等)
        /// </summary>
        public string DeviceName { get; set; }

        private int _flowDirection;

        /// <summary>
        /// 流动方向(左右、上下)  0  1
        /// </summary>
        public int FlowDirection
        {
            get { return _flowDirection; }
            set { SetProperty(ref _flowDirection, value); }
        }

        private int _rotate;

        /// <summary>
        /// 旋转角度
        /// </summary>
        public int Rotate
        {
            get { return _rotate; }
            set { SetProperty(ref _rotate, value); }
        }

        #endregion

        #region 起始坐标

        /// <summary>
        /// 起始坐标
        /// </summary>
        private Point startPoint = new Point(0, 0);//起始坐标

        #endregion

        #region 找上级为canvas的控件
        /// <summary>
        /// 找上级为canvas的控件
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        private Canvas GetParent(FrameworkElement device)
        {
            //视觉树
            dynamic parent = VisualTreeHelper.GetParent(device);
            if (parent != null && parent is Canvas)
            {
                return parent;
            }

            return GetParent(parent);//递归调用
        }
        #endregion

        #region 获取所属有设备的委托
        /// <summary>
        /// 委托 用来接收 获取所有设备 的方法
        /// </summary>
        public Func<List<DeviceModel>> GetAllDeviceFunc { get; set; }
        #endregion

        #region 除当前设备以外的所有其他设备，不包括水平/垂直对齐，也不包括高/宽对齐线
        private List<DeviceModel> OtherDeviceList = new List<DeviceModel>();
        #endregion

        #region 设备拖动

        private bool IsMoving = false;//是否正在移动设备

        private List<DeviceModel> LineList = new List<DeviceModel>();//水平线/垂直线

        /// <summary>
        /// 按下鼠标左键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //1、设备正在移动
            IsMoving = true;//要移动

            //2、记录鼠标左键按下时，设备(contentcontrol)的位置
            startPoint = e.GetPosition((IInputElement)sender);

            //3、光标捕获设备(contentcontrol)
            Mouse.Capture((IInputElement)sender);

            #region 移动的显示水平线/垂直线用的
            //除当前设备以外的所有其他设备，不包括水平线/垂直线   不包括宽/高对齐线
            OtherDeviceList = GetAllDeviceFunc().Where(d => !new string[] { "HL", "VL", "WidthRule", "HeightRule" }.Contains(d.ComponentType) && d != this).ToList();

            //水平线/垂直线
            LineList = GetAllDeviceFunc().Where(d => new string[] { "HL", "VL" }.Contains(d.ComponentType)).ToList();
            #endregion

            //4、已经处理了，不要再往上冒泡处理
            e.Handled = true;
        }

        /// <summary>
        /// 移动鼠标
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (IsMoving)
            {
                //1、记录新位置  鼠标在canvas里面的新位置
                //2、设备所在的canvas
                var parentCanvas = GetParent((FrameworkElement)sender);
                Point curPoint = e.GetPosition(parentCanvas);//当前光标在canvas里的位置

                double tempX = curPoint.X - startPoint.X;//临时X
                double tempY = curPoint.Y - startPoint.Y;////临时Y

                //水平线/垂直线 对齐
                if (OtherDeviceList.Count > 0)
                {
                    #region 垂直对齐
                    //垂直对齐，找X离得近
                    var deviceV = OtherDeviceList.FirstOrDefault(d => Math.Abs(d.X - tempX) < 20);//离得近的设备
                    var vLine = LineList.First(line => line.ComponentType == "VL");//垂直对齐线
                    if (deviceV != null)
                    {
                        vLine.IsVisible = true;//显示垂直对齐线
                        vLine.X = deviceV.X;//将对齐线移动到离得近的设备

                        if (Math.Abs(deviceV.X - tempX) < 10)//离得更近
                        {
                            tempX = deviceV.X;//吸附
                        }
                    }
                    else
                    {
                        vLine.IsVisible = false;//隐藏垂直对齐线
                    }
                    #endregion

                    #region 水平对齐
                    //水平对齐，找Y离得近
                    var deviceH = OtherDeviceList.FirstOrDefault(d => Math.Abs(d.Y - tempY) < 20);//离得近的设备
                    var hLine = LineList.First(line => line.ComponentType == "HL");//水平对齐线
                    if (deviceH != null)
                    {
                        hLine.IsVisible = true;//显示水平对齐线
                        hLine.Y = deviceH.Y;//将对齐线移动到离得近的设备

                        if (Math.Abs(deviceH.Y - tempY) < 10)//离得更近
                        {
                            tempY = deviceH.Y;//吸附
                        }
                    }
                    else
                    {
                        hLine.IsVisible = false;//隐藏水平对齐线
                    }
                    #endregion
                }

                X = tempX;
                Y = tempY;
            }
        }

        /// <summary>
        /// 松开鼠标左键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            IsMoving = false;//不移动设备了

            //鼠标松开的时候，隐藏对齐线
            foreach (var line in LineList)
            {
                line.IsVisible = false;
            }

            Mouse.Capture(null);//释放光标
        }
        #endregion

        #region 删除
        public DelegateCommand<DeviceModel> DeleteCommand { get; set; }
        #endregion

        #region 是否选中

        private bool _isSelected;

        double ZTemp = 0;//层的中间变量

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value)//如果选择了就应该在最上层
                {
                    ZTemp = Z;
                    Z = 3;
                }
                else//如果没有选择，则恢复到之前的层
                {
                    Z = ZTemp;
                }

                SetProperty(ref _isSelected, value);
            }
        }
        #endregion

        #region 是否显示
        private bool _isVisible = true;//默认显示

        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                SetProperty(ref _isVisible, value);
            }
        }

        #endregion

        #region 设备缩放
        private bool IsResize = false;//是否正在缩放

        double OldWidth, OldHeight;//旧高宽(鼠标左键按下那一刻设备的高宽)

        /// <summary>
        /// 按下左键
        /// </summary>
        public DelegateCommand<object> ResizeDownCommand => new DelegateCommand<object>(DoResizeDown);

        /// <summary>
        /// 移动
        /// </summary>
        public DelegateCommand<object> ResizeMoveCommand => new DelegateCommand<object>(DoResizeMove);


        /// <summary>
        /// 松开
        /// </summary>
        public DelegateCommand<object> ResizeUpCommand => new DelegateCommand<object>(DoResizeUp);


        private List<DeviceModel> HWLineList = new List<DeviceModel>();//高/宽对齐线

        /// <summary>
        /// 按下左键执行
        /// </summary>
        /// <param name="obj"></param>
        private void DoResizeDown(object obj)
        {
            //1、设置按下左键
            IsResize = true;

            //2、记录鼠标现在在canvas里的坐标
            var e = obj as MouseButtonEventArgs;

            startPoint = e.GetPosition(GetParent((FrameworkElement)e.Source));

            //3、记录此时的高宽
            OldWidth = Width;
            OldHeight = Height;

            //4、捕获光标
            Mouse.Capture((IInputElement)e.Source);

            //5、已经处理了
            e.Handled = true;

            #region 高/宽对齐 需要
            //除当前设备以外的所有其他设备，不包括水平线/垂直线   不包括宽/高对齐线
            OtherDeviceList = GetAllDeviceFunc().Where(d => !new string[] { "HL", "VL", "WidthRule", "HeightRule" }.Contains(d.ComponentType) && d != this).ToList();

            HWLineList = GetAllDeviceFunc().Where(d => new string[] { "WidthRule", "HeightRule" }.Contains(d.ComponentType) && d != this).ToList();
            #endregion
        }

        /// <summary>
        /// 移动
        /// </summary>
        /// <param name="obj"></param>
        private void DoResizeMove(object obj)
        {
            var e = obj as MouseEventArgs;
            if (IsResize)
            {
                //1、获取鼠标现在的位置
                Point currentPoint = e.GetPosition(GetParent((FrameworkElement)e.Source));
                var cursor = ((Ellipse)e.Source).Cursor;
                if (cursor != null)
                {
                    if (cursor == Cursors.SizeWE)//水平方向
                    {
                        double computeWidth = OldWidth + (currentPoint.X - startPoint.X);
                        //改变computeWidth的过程

                        //需求：键盘按住Alt键的时才宽高对齐
                        if (Keyboard.Modifiers == ModifierKeys.Alt)
                        {
                            computeWidth = ResetWidth(computeWidth);
                        }

                        Width = computeWidth;
                    }

                    if (cursor == Cursors.SizeNWSE)//右下方向
                    {
                        //var rate = Width / Height;//比例
                        //double computeWidth = OldWidth + (currentPoint.X - startPoint.X);
                        //computeWidth = ResetWidth(computeWidth);

                        //Width = computeWidth;
                        //Height = Width / rate;//按比例

                        if (Keyboard.Modifiers == ModifierKeys.Alt)
                        {
                            double computeWidth = OldWidth + (currentPoint.X - startPoint.X);
                            computeWidth = ResetWidth(computeWidth);

                            double computeHeight = OldHeight + (currentPoint.Y - startPoint.Y);
                            computeHeight = ResetHeight(computeHeight);
                        }
                        else
                        {
                            var rate = Width / Height;//比例
                            double computeWidth = OldWidth + (currentPoint.X - startPoint.X);

                            Width = computeWidth;
                            Height = Width / rate;//按比例
                        }
                    }

                    if (cursor == Cursors.SizeNS)//垂直方向
                    {
                        double computeHeight = OldHeight + (currentPoint.Y - startPoint.Y);

                        if (Keyboard.Modifiers == ModifierKeys.Alt)
                        {
                            computeHeight = ResetHeight(computeHeight);
                        }

                        Height = computeHeight;
                    }
                }
            }

            //5、已经处理了
            e.Handled = true;
        }

        /// <summary>
        /// 鼠标松开
        /// </summary>
        /// <param name="obj"></param>
        private void DoResizeUp(object obj)
        {
            IsResize = false;
            var e = obj as MouseButtonEventArgs;
            e.Handled = true;
            Mouse.Capture(null);

            //高/宽对齐线都要隐藏
            foreach (var line in HWLineList)
            {
                line.IsVisible = false;
            }
        }

        /// <summary>
        /// 重新设置宽度
        /// </summary>
        /// <param name="width">对齐前的原来的宽度</param>
        /// <returns>对齐后的新宽度</returns>
        private double ResetWidth(double width)
        {
            //改变computeWidth的过程
            if (OtherDeviceList.Count > 0)
            {
                //1、从其他设备找到宽度接近的设备
                var wDevice = OtherDeviceList.FirstOrDefault(d => Math.Abs(d.Width - width) < 20);
                //宽度对齐线
                var wLine = HWLineList.First(line => line.ComponentType == "WidthRule");
                if (wDevice != null)
                {
                    wLine.IsVisible = true;//显示宽对齐线
                    wLine.Width = wDevice.Width;//宽对齐线宽度和找到的设备宽度一致

                    //调整对齐线的位置
                    wLine.X = wDevice.X;
                    wLine.Y = wDevice.Y + wDevice.Height + 5;//宽度对齐线显示在设备下方，+5表示不要贴设备底端

                    if (Math.Abs(wDevice.Width - width) < 10)//如果宽度更接近了
                    {
                        width = wDevice.Width;
                    }
                }
                else
                {
                    wLine.IsVisible = false;
                }
            }

            return width;
        }

        /// <summary>
        /// 重新设置高度
        /// </summary>
        /// <param name="height">对齐前的原来的高度</param>
        /// <returns>对齐后的新高度</returns>
        private double ResetHeight(double height)
        {
            //改变computeHeight的过程
            if (OtherDeviceList.Count > 0)
            {
                //1、从其他设备找到宽度接近的设备
                var hDevice = OtherDeviceList.FirstOrDefault(d => Math.Abs(d.Height - height) < 20);
                //宽度对齐线
                var hLine = HWLineList.First(line => line.ComponentType == "HeightRule");
                if (hDevice != null)
                {
                    hLine.IsVisible = true;//显示高对齐线
                    hLine.Height = hDevice.Height;//高对齐线高度和找到的设备高度一致

                    //调整对齐线的位置
                    hLine.X = hDevice.X + hDevice.Width + 5;//高度对齐线显示在设备下方，+5表示不要贴设备右边
                    hLine.Y = hDevice.Y;


                    if (Math.Abs(hDevice.Height - height) < 10)//如果高度更接近了
                    {
                        height = hDevice.Height;
                    }
                }
                else
                {
                    hLine.IsVisible = false;
                }
            }

            return height;
        }
        #endregion

        #region 设备右键功能
        public List<Control> ContextMenus { get; set; }

        /// <summary>
        /// 初始化右键菜单
        /// </summary>
        public void InitContextMenu()
        {
            ContextMenus = new List<Control>();
            ContextMenus.Add(new MenuItem
            {
                Header = "顺时针旋转",

                //只有接头组件才显示该功能菜单
                Visibility = new string[] {
                    "RAJointsUC", "TeeJointsUC"
                }.Contains(this.ComponentType) ? Visibility.Visible : Visibility.Collapsed,

                Command = new DelegateCommand(() => this.Rotate += 90),
            });
            ContextMenus.Add(new MenuItem
            {
                Header = "逆时针旋转",

                Visibility = new string[] {
                    "RAJointsUC", "TeeJointsUC"
                }.Contains(this.ComponentType) ? Visibility.Visible : Visibility.Collapsed,

                Command = new DelegateCommand(() => this.Rotate -= 90),
            });

            ContextMenus.Add(new MenuItem
            {
                Header = "改变流向",

                //只有管道才显示该功能
                Visibility = new string[] {
                    "HorizontalPipelineUC", "VerticalPipelineUC"
                }.Contains(this.ComponentType) ? Visibility.Visible : Visibility.Collapsed,

                Command = new DelegateCommand(() =>
                {
                    FlowDirection = (FlowDirection + 1) % 2;
                })

            });

            ContextMenus.Add(new Separator());//添加分割线

            ContextMenus.Add(new MenuItem
            {
                Header = "向上一层",
                Command = new DelegateCommand(() =>
                {
                    if (Z < 2)
                    {
                        Z++;
                    }
                    else
                    {
                        Z = 2;
                    }
                })
            });
            ContextMenus.Add(new MenuItem
            {
                Header = "向下一层",
                Command = new DelegateCommand(() =>
                {
                    if (Z > 0)
                    {
                        if (Z == 3)//3是因为当选择的时候设置为3
                        {
                            Z = Z - 2;
                        }
                        else
                        {
                            Z--;
                        }
                    }
                    else
                    {
                        Z = 0;
                    }
                })
            });
            ContextMenus.Add(new Separator { });

            ContextMenus.Add(new MenuItem
            {
                Header = "删除",
                Command = DeleteCommand,
                CommandParameter = this
            });

            //如果分割线显示在开头则隐藏分割线
            var firstVisibleMenu = ContextMenus.First(m => m.Visibility == Visibility.Visible);
            if (firstVisibleMenu is Separator)
            {
                firstVisibleMenu.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #region 设备属性
        public ObservableCollection<DevicePropModel> DevicePropList { get; set; } = new ObservableCollection<DevicePropModel>();

        /// <summary>
        /// 添加设备属性
        /// </summary>
        public DelegateCommand AddPropCommand => new DelegateCommand(() =>
        {
            DevicePropList.Add(new DevicePropModel()
            {
                PropName = "Protocol",
                PropValue = "ModbusRtu"
            }
            );
        });

        /// <summary>
        /// 删除设备属性
        /// </summary>
        public DelegateCommand<DevicePropModel> DeletePropCommand => new DelegateCommand<DevicePropModel>(model => DevicePropList.Remove(model));

        #endregion

        #region 设备变量

        /// <summary>
        /// 变量配置集合
        /// </summary>
        public ObservableCollection<DeviceVarModel> DeviceVarList { get; set; } = new ObservableCollection<DeviceVarModel>();

        /// <summary>
        /// 添加变量
        /// </summary>
        public DelegateCommand AddVariableCommand => new DelegateCommand(() =>
        {
            DeviceVarList.Add(new DeviceVarModel() { VarNum = "V" + DateTime.Now.ToString("yyyyMMddHHmmssFFF") });
        });

        /// <summary>
        /// 删除变量
        /// </summary>
        public DelegateCommand<DeviceVarModel> DeleteVariableCommand => new DelegateCommand<DeviceVarModel>(model => DeviceVarList.Remove(model));

        #endregion

        #region 设备变量显示
        private bool _isMonitor;
        /// <summary>
        /// 是否是监控状态(用于区分监控界面和编辑界面)
        /// </summary>
        public bool IsMonitor
        {
            get { return _isMonitor; }
            set { SetProperty(ref _isMonitor, value); }
        }
        #endregion

        #region 报警提醒
        private bool _isWarning;

        /// <summary>
        /// 是否报警
        /// </summary>
        public bool IsWarning
        {
            get { return _isWarning; }
            set { SetProperty(ref _isWarning, value); }
        }

        private string _warningMsg;

        /// <summary>
        /// 报警信息
        /// </summary>
        public string WarningMsg
        {
            get { return _warningMsg; }
            set { SetProperty(ref _warningMsg, value); }
        }

        #endregion

        #region 手动控制选项
        public ObservableCollection<ManualControlModel> ManualControlList { get; set; } = new ObservableCollection<ManualControlModel>();

        /// <summary>
        /// 添加手动控制
        /// </summary>
        public DelegateCommand AddManualControlCommand => new DelegateCommand(() => { ManualControlList.Add(new ManualControlModel()); });

        /// <summary>
        /// 删除手动控制
        /// </summary>
        public DelegateCommand<ManualControlModel> DeleteManualControlCommand => new DelegateCommand<ManualControlModel>(model => { ManualControlList.Remove(model); });

        // 手动控制命令
        public DelegateCommand<ManualControlModel> ManualControlCommand => new DelegateCommand<ManualControlModel>((model
            ) =>
        {
            #region 直接写
            Communication communication = Communication.CreateInstance();//调用单例方法
            // 准备通信对象
            var reEo = communication.GetExecuteObject(this.DevicePropList.Select(p =>
                new DevicePropEntity { PropName = p.PropName, PropValue = p.PropValue }
                ).ToList());
            if (!reEo.Status)
            {
                this.IsWarning = true;
                this.WarningMsg = reEo.Msg;
                return;
            }

            //开始执行写
            //契约:WriteBytes 传主机小端(BitConverter 原生结果),转端由协议层 ConvertEndian 负责,此处不再手动翻转
            byte[] writeBytes = BitConverter.GetBytes(Convert.ToInt16(model.ControlValue));

            WriteDataInfo writeDataInfo = new WriteDataInfo
            {
                StartAddr = model.ControlAddress,
                ValueType = typeof(UInt16),
                WriteBytes = writeBytes,
            };

            var rw = reEo.Data.Write(writeDataInfo);//执行写
            if (!rw.Status)//如果写失败了，则报警提示
            {
                this.IsWarning = true;
                this.WarningMsg = reEo.Msg;
            }
            #endregion
        });
        #endregion
    }
}
