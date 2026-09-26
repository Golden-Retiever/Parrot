using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Win32;
using Platform.DataEntities;
using Platform.DBAccess;
using Platform.Helper;
using Platform.Models;
using Platform.ViewModels.DialogWin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Platform.ViewModels.FunctionUC
{
    /// <summary>
    /// 趋势视图模型
    /// </summary>
    public class TrendUCViewModel
    {
        MainUCViewModel _mainUCViewModel;//主界面视图

        IDataAccess _dataAccess;//数据库访问

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="mainUCViewModel"></param>
        public TrendUCViewModel(MainUCViewModel mainUCViewModel, IDataAccess dataAccess)
        {
            _mainUCViewModel = mainUCViewModel;
            _dataAccess= dataAccess;

            //初始化趋势
            TrendList = new ObservableCollection<TrendModel>()
                {
                    new TrendModel{  IsSelected=true, GetAllDeviceFunc=GetAllDevice}//默认有一个趋势并选择了
                };

            ChartInit();//初始化趋势

            //每个趋势都要显示图表
            foreach (TrendModel trendModel in TrendList)
            {
                trendModel.ChartScan();//显示图表
            }
        }

        /// <summary>
        /// 获取所有设备
        /// </summary>
        /// <returns></returns>
        private List<DeviceModel> GetAllDevice()
        {
            return _mainUCViewModel.DeviceList.ToList();
        }

        /// <summary>
        /// 趋势集合
        /// </summary>
        public ObservableCollection<TrendModel> TrendList { get; set; }


        #region 趋势 添加 删除

        /// <summary>
        /// 添加趋势
        /// </summary>
        public DelegateCommand AddTrendCommand => new DelegateCommand(() =>
        {
            TrendList.Add(new TrendModel() { IsSelected = true, GetAllDeviceFunc = GetAllDevice });//默认新添加的
        });

        /// <summary>
        /// 删除趋势
        /// </summary>
        public DelegateCommand<TrendModel> DelTrendCommand => new DelegateCommand<TrendModel>((model) =>
        {
            if (TrendList.Count == 1)//至少保留一个
            {
                return;
            }

            //如果删除的趋势为选择状态，默认选择删除的上一个趋势
            int index = Math.Max(0, TrendList.IndexOf(model) - 1);//新选择的趋势索引

            TrendList.Remove(model);
            if (model.IsSelected)//如果删除的趋势为选择状态
            {
                TrendList[index].IsSelected = true;
            }
        });

        public TrendModel SelectedTrend { get; set; }//选择的趋势
        #endregion

        #region 纵轴编辑

        /// <summary>
        /// 颜色集合
        /// </summary>
        public List<string> BrushList
        {
            get
            {
                return typeof(Brushes).GetProperties().Select(p => p.Name).ToList();
            }
        }

        public DelegateCommand ShowAxisEditCommand => new DelegateCommand(() =>
        {
            if (SelectedTrend == null)
            {
                return;
            }

            ActionHelper.ExecuteAndResult("ShowTrendAxisEdit",
                new TrendAxisEditWinViewModel
                {
                    Trend = SelectedTrend,
                    BrushList = BrushList
                });
        });
        #endregion

        #region 打开选择设备变量
        public DelegateCommand ShowDeviceVarDialogCommand => new DelegateCommand(() =>
        {
            if (SelectedTrend == null)
            {
                return;
            }
            ActionHelper.ExecuteAndResult("ShowTrendVars",
                new TrendDeviceChooseWinViewModel(SelectedTrend, BrushList, _mainUCViewModel));
        });
        #endregion

        #region 保存趋势所有配置
        public DelegateCommand SaveTrendCommand => new DelegateCommand(() =>
        {
            List<TrendEntity> trendEntities = new List<TrendEntity>();//趋势
            List<TrendAxisEntity> trendAxisEntities = new List<TrendAxisEntity>();//纵轴
            List<TrendSectionEntity> trendSectionEntities = new List<TrendSectionEntity>();//预警线
            List<TrendSeriesEntity> trendSeriesEntities = new List<TrendSeriesEntity>();//图序列

            foreach (TrendModel trendInfo in TrendList)
            {
                //趋势
                trendEntities.Add(new TrendEntity { TrendNum = trendInfo.TNum, TrendName = trendInfo.TrendName, IsShowLegend = trendInfo.IsShowLegend });

                //纵轴信息
                foreach (TrendAxisModel trendAxisInfo in trendInfo.AxisList)
                {
                    trendAxisEntities.Add(new TrendAxisEntity
                    {
                        AxisNum = trendAxisInfo.ANum,
                        TrendNum = trendInfo.TNum,
                        Title = trendAxisInfo.Title,
                        IsShowTitle = trendAxisInfo.IsShowTitle,
                        Minimum = trendAxisInfo.Minimum,
                        Maximum = trendAxisInfo.Maximum,
                        IsShowSeperator = trendAxisInfo.IsShowSeperator,
                        LabelFormater = trendAxisInfo.LabelFormater,
                        Position = trendAxisInfo.Position,
                    });

                    //预警线
                    foreach (TrendSectionModel trendSectionInfo in trendAxisInfo.SectionList)
                    {
                        trendSectionEntities.Add(new TrendSectionEntity { AxisNum = trendAxisInfo.ANum, Value = trendSectionInfo.Value, Color = trendSectionInfo.Color });
                    }
                }

                //图序列
                foreach (TrendSeriesModel trendSeriesInfo in trendInfo.Series)
                {
                    trendSeriesEntities.Add(new TrendSeriesEntity
                    {
                        DeviceNum = trendSeriesInfo.DeviceNum,
                        VarNum = trendSeriesInfo.VarNum,
                        Title = trendSeriesInfo.Title,
                        Color = trendSeriesInfo.Color,
                        ANum = trendSeriesInfo.ANum,
                        TrendNum = trendInfo.TNum,
                    });
                }
            }

            _dataAccess.SaveTrend(trendEntities, trendAxisEntities, trendSectionEntities, trendSeriesEntities);
            MessageBox.Show("保存趋势信息成功");
        });
        #endregion

        #region 初始化趋势图表

        /// <summary>
        /// 初始化趋势图表
        /// </summary>
        private void ChartInit()
        {
            var trendList = _dataAccess.GetTrends();//趋势
            var trendAxisList = _dataAccess.GetTrendAxises();//趋势纵轴
            var trendSections = _dataAccess.GetTrendSections();//预警
            var trendSerieses = _dataAccess.GetTrendSerieses();//图表序列

            this.TrendList = new ObservableCollection<TrendModel>(trendList.Select(t => new TrendModel
            {
                //趋势基本信息
                GetAllDeviceFunc = GetAllDevice,//获取所有设备的方法
                TNum = t.TrendNum,
                TrendName = t.TrendName,
                IsShowLegend = t.IsShowLegend,

                //纵轴
                AxisList = new ObservableCollection<TrendAxisModel>(trendAxisList.Where(ta => ta.TrendNum == t.TrendNum).Select(a => new TrendAxisModel
                {
                    ANum = a.AxisNum,
                    Title = a.Title,
                    IsShowTitle = a.IsShowTitle,
                    Minimum = a.Minimum,
                    Maximum = a.Maximum,
                    IsShowSeperator = a.IsShowSeperator,

                    LabelFormater = a.LabelFormater,
                    Position = a.Position,

                    //预警信息
                    SectionList = new ObservableCollection<TrendSectionModel>(trendSections.Where(ts => ts.AxisNum == a.AxisNum).Select(s => new TrendSectionModel
                    {
                        Value = s.Value,
                        Color = s.Color
                    }))
                })),

                //图表序列
                Series = new ObservableCollection<TrendSeriesModel>(trendSerieses.Where(tseries => tseries.TrendNum == t.TrendNum).Select(s => new TrendSeriesModel
                {
                    DeviceNum = s.DeviceNum,
                    VarNum = s.VarNum,
                    Title = s.Title,
                    Color = s.Color,
                    ANum = s.ANum
                }))
            }));

            if (this.TrendList.Count > 0)//默认选中第一个
            {
                this.TrendList[0].IsSelected = true;
            }
        }
        #endregion

        #region 导出图像

        public DelegateCommand<Visual> SaveToImageCommand => new DelegateCommand<Visual>(SaveToImage);

        /// <summary>
        /// 导出图像
        /// </summary>
        /// <param name="obj">导出图像的视觉，如grid等</param>
        private void SaveToImage(Visual obj)
        {
            // 创建保存文件对话框
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            // 设置默认文件名：Chart + 时间戳（精确到毫秒） + .png，避免文件名重复
            saveFileDialog.FileName = "Chart" + DateTime.Now.ToString("yyyyMMddHHmmssFFF") + ".png";
            saveFileDialog.Filter = "PNG图片 (*.png)|*.png|JPG图片 (*.jpg)|*.jpg|GIF图片 (*.gif)|*.gif";// 过滤文件类型
            // 检查保存路径是否存在（不存在则提示用户） 检查文件所在的文件夹是否存在
            saveFileDialog.CheckPathExists = true;
            // 显示对话框，用户点击"保存"时返回true
            if (saveFileDialog.ShowDialog() == true)
            {
                // 将传入的obj转换为Visual类型，调用渲染保存方法
                CreateBitmapFromVisual(obj, saveFileDialog.FileName);
            }
        }

        /// <summary>
        /// 渲染保存方法
        /// </summary>
        /// <param name="target">视觉</param>
        /// <param name="fileName">文件名</param>
        private void CreateBitmapFromVisual(Visual target, string fileName)
        {
            if (target == null || string.IsNullOrEmpty(fileName))
            {
                return;
            }

            // 获取目标Visual元素的边界（宽、高、位置）
            Rect bounds = VisualTreeHelper.GetDescendantBounds(target);

            // 创建渲染目标位图：参数依次为宽、高、DPI（水平/垂直）、像素格式
            RenderTargetBitmap renderTarget = new RenderTargetBitmap((int)bounds.Width, (int)bounds.Height, 96, 96, PixelFormats.Pbgra32);

            // 创建绘图可视化对象，用于承载要渲染的内容
            DrawingVisual visual = new DrawingVisual();

            // 打开绘图上下文（using自动释放资源）
            using (DrawingContext context = visual.RenderOpen())
            {
                // 创建视觉画刷，绑定到目标Visual元素
                VisualBrush visualBrush = new VisualBrush(target);

                // 绘制矩形：用视觉画刷填充，覆盖目标元素的整个边界
                context.DrawRectangle(visualBrush, null, new Rect(new Point(), bounds.Size));
            }

            // 将绘制好的visual渲染到位图中
            renderTarget.Render(visual);

            // 创建PNG编码器（用于将位图转为PNG格式）

            PngBitmapEncoder bitmapEncoder = new PngBitmapEncoder();

            // 将渲染后的位图添加为编码器的帧
            bitmapEncoder.Frames.Add(BitmapFrame.Create(renderTarget));

            // 创建文件流（using自动关闭流），将编码后的内容写入文件
            using (Stream stm = File.Create(fileName))
            {
                bitmapEncoder.Save(stm);
            }
        }
        #endregion
    }
}
