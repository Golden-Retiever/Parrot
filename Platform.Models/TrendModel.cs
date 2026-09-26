using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Platform.Models
{
    /// <summary>
    /// 趋势模型
    /// </summary>
    public class TrendModel : BindableBase
    {
        public TrendModel()
        {
            // X轴的初始化
            Axis xAxis = new Axis();
            XAxis.Add(xAxis);
            xAxis.Separator = new Separator { Step = 1, StrokeThickness = 0 };
            xAxis.Labels = new List<string>();
            xAxis.LabelsRotation = -45;

            // Y轴可以有多条
            YAxis = new AxesCollection();
            YAxis.Add(AxisList[0].Axis);

            //当Series值发生变化，调用Series_CollectionChanged
            Series.CollectionChanged += Series_CollectionChanged;
        }

        /// <summary>
        /// 当Series值发生变化，调用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Series_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.Dispose();
            cts = new CancellationTokenSource();
            this.ChartScan();
        }

        #region 基本属性(一般都是用来保存到db，极少部分是业务需要)

        /// <summary>
        /// 趋势编号
        /// </summary>
        public string TNum { get; set; } = "T" + DateTime.Now.ToString("yyyyMMddHHmmssFFFF");

        private string _trendName = "新建趋势图";

        /// <summary>
        /// 趋势名称
        /// </summary>
        public string TrendName
        {
            get { return _trendName; }
            set { SetProperty(ref _trendName, value); }
        }

        // 图例处理
        private bool _isShowLegend;

        /// <summary>
        /// 是否显示图例
        /// </summary>
        public bool IsShowLegend
        {
            get { return _isShowLegend; }
            set
            {
                SetProperty(ref _isShowLegend, value);

                if (value)
                {
                    LegendLocation = LegendLocation.Top;//如果，显示图例放顶端
                }
                else
                {
                    LegendLocation = LegendLocation.None;//不显示
                }
            }
        }

        private bool _isSelected;

        /// <summary>
        /// 是否选择
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }

        // 纵轴集合
        private ObservableCollection<TrendAxisModel> _axisList = new ObservableCollection<TrendAxisModel>()
        {
                new TrendAxisModel() {IsShowSeperator = true}
        };

        /// <summary>
        /// 纵轴集合
        /// </summary>
        public ObservableCollection<TrendAxisModel> AxisList
        {
            get { return _axisList; }
            set
            {
                SetProperty(ref _axisList, value);

                //图表
                YAxis.Clear();
                foreach (TrendAxisModel trendAxisModel in value)
                {
                    YAxis.Add(trendAxisModel.Axis);
                }
            }
        }

        private ObservableCollection<TrendSeriesModel> _series =
        new ObservableCollection<TrendSeriesModel>();

        /// <summary>
        /// 图表序列
        /// </summary>
        public ObservableCollection<TrendSeriesModel> Series
        {
            get => _series;
            set
            {
                _series = value;

                //出来显示的图表数据
                ChartSeries.Clear();
                foreach (TrendSeriesModel trendSeriesModel in value)
                {
                    ChartSeries.Add(trendSeriesModel.Series);
                }
            }
        }


        #endregion

        #region 纵轴添加/删除

        /// <summary>
        /// 添加纵轴命令
        /// </summary>
        public DelegateCommand AddAxisCommand => new DelegateCommand(() =>
        {
            TrendAxisModel tmodel = new TrendAxisModel();

            AxisList.Add(tmodel);//还要保存到数据库里

            YAxis.Add(tmodel.Axis);//将加进来的显示出来 在图表里显示
        });

        /// <summary>
        /// 删除纵轴命令
        /// </summary>
        public DelegateCommand<TrendAxisModel> DeleteAxisCommand => new DelegateCommand<TrendAxisModel>(model =>
        {
            // 删除的时候，保留一个纵轴
            if (AxisList.Count == 1)
            {
                return;
            }
            AxisList.Remove(model);

            YAxis.Remove(model.Axis);//将删除的纵轴 在图表里也要删除
        });
        #endregion

        #region 显示横轴/纵轴(都是livecharts)
        /// <summary>
        /// 图表里显示的横轴
        /// </summary>
        public AxesCollection XAxis { get; set; } = new AxesCollection();

        /// <summary>
        /// 图表里显示的纵轴
        /// </summary>
        public AxesCollection YAxis { get; set; } = new AxesCollection();
        #endregion

        #region 显示图表

        private LegendLocation _legendLocation = LegendLocation.None;//默认不显示

        /// <summary>
        /// 图例
        /// </summary>
        public LegendLocation LegendLocation
        {
            get { return _legendLocation; }
            set
            {
                SetProperty(ref _legendLocation, value);
            }
        }

        public Func<List<DeviceModel>> GetAllDeviceFunc;//用来接收 获取所有设备的方法

        CancellationTokenSource cts = new CancellationTokenSource();
        List<Task> tasks = new List<Task>();

        /// <summary>
        /// 显示图表
        /// </summary>
        public void ChartScan()
        {
            // 根据维护所有序列信息，获取对应的设备和变量
            // 1、通过配置的设备编号获取所有设备
            List<DeviceModel> scanDevices = GetAllDeviceFunc?.Invoke();

            if (scanDevices == null || scanDevices.Count == 0)
            {
                return;
            }

            var task = Task.Run(async () =>
            {
                while (!cts.IsCancellationRequested)
                {
                    await Task.Delay(1000);
                    // 添加一个X轴的标签：显示的时间（时：分：秒）
                    Application.Current?.Dispatcher.Invoke(() =>
                    {
                        if (XAxis[0].Labels != null)
                        {
                            XAxis[0].Labels.Add(DateTime.Now.ToString("HH:mm:ss"));
                            if (XAxis[0].Labels.Count > 30)//最多30个
                            {
                                XAxis[0].Labels.RemoveAt(0);
                            }
                        }
                    });

                    #region 显示图像
                    foreach (var trendSeriesModel in Series)//图表序列
                    {
                        var device = scanDevices.FirstOrDefault(d => d.DeviceNum == trendSeriesModel.DeviceNum);
                        if (device == null)
                        {
                            continue;
                        }

                        var deviceVar = device.DeviceVarList.FirstOrDefault(v => v.VarNum == trendSeriesModel.VarNum);
                        if (deviceVar == null)
                        {
                            continue;
                        }

                        Application.Current?.Dispatcher.Invoke(new Action(() =>
                        {
                            if (double.TryParse(deviceVar.ReadValue.ToString(), out double doubleRst))
                            {
                                trendSeriesModel.Series.Values.Add(doubleRst);
                                if (trendSeriesModel.Series.Values.Count > 30)//最多30个
                                {
                                    trendSeriesModel.Series.Values.RemoveAt(0);
                                }
                            }
                        }));
                    }
                    #endregion
                }
            }, cts.Token);
            tasks.Add(task);
        }

        /// <summary>
        /// 要显示的所有图表数据
        /// </summary>
        public SeriesCollection ChartSeries { get; set; } = new SeriesCollection();

        #endregion

        /// <summary>
        /// task释放
        /// </summary>
        public void Dispose()
        {
            cts.Cancel();
            Task.WaitAll(tasks.ToArray(), 500);
        }
    }
}
