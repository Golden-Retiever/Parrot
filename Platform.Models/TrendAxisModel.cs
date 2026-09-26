using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Platform.Models
{
    /// <summary>
    /// 纵轴模型
    /// </summary>
    public class TrendAxisModel : BindableBase
    {
        #region 基本属性

        /// <summary>
        /// 纵轴编号
        /// </summary>
        public string ANum { get; set; } = "A" + DateTime.Now.ToString("yyyyMMddHHmmssFFFF");

        private string _title = "新纵轴";

        /// <summary>
        /// 纵轴标题
        /// </summary>
        public string Title
        {
            get { return _title; }
            set
            {
                SetProperty(ref _title, value);

                //如果显示标题，则需要修改显示的标题（Axis的Title）
                Axis.Title = (IsShowTitle ? value : "");
            }
        }

        private bool _isShowTitle;

        /// <summary>
        /// 是否显示标题
        /// </summary>
        public bool IsShowTitle
        {
            get { return _isShowTitle; }
            set
            {
                SetProperty(ref _isShowTitle, value);

                Axis.Title = (value? Title:"");
            }
        }

        private double _minimum = 0;

        /// <summary>
        /// 最小值
        /// </summary>
        public double Minimum
        {
            get { return _minimum; }
            set
            {
                SetProperty(ref _minimum, value);

                Axis.MinValue = value;//图表也要更新
            }
        }

        private double _maximum = 100;

        /// <summary>
        /// 最大值
        /// </summary>
        public double Maximum
        {
            get { return _maximum; }
            set
            {
                SetProperty(ref _maximum, value);

                Axis.MaxValue = value;//图表也要更新
            }
        }

        private bool _isShowSeperator = false;

        /// <summary>
        /// 是否显示分割线
        /// </summary>
        public bool IsShowSeperator
        {
            get { return _isShowSeperator; }
            set
            {
                SetProperty(ref _isShowSeperator, value);

                Axis.Separator.StrokeThickness = (value ? 1 : 0);
            }
        }

        private string _labelFormater = "00";

        /// <summary>
        /// 格式
        /// </summary>
        public string LabelFormater
        {
            get { return _labelFormater; }
            set
            {
                SetProperty(ref _labelFormater, value);

                Axis.LabelFormatter = new Func<double, string>(v => v.ToString(value));
            }
        }

        private string _position = "Left";

        /// <summary>
        /// 位置(左/右)
        /// </summary>
        public string Position
        {
            get { return _position; }
            set
            {
                SetProperty(ref _position, value);

                if (value == "Left")
                {
                    this.Axis.Position = LiveCharts.AxisPosition.LeftBottom;//如果是纵轴就是left；如果是横轴就是bottom
                }
                else if (value == "Right")
                {
                    this.Axis.Position = LiveCharts.AxisPosition.RightTop;//如果是纵轴就是right；如果是横轴就是top
                }
            }
        }
        #endregion

        #region 显示轴

        /// <summary>
        ///  LiveCharts的对象  用来显示图表的
        /// </summary>
        public Axis Axis { get; set; } = new Axis()
        {
            Title = "",
            MinValue = 0,
            MaxValue = 100,
            LabelFormatter = new Func<double, string>(v => v.ToString("00")),

            Separator = new Separator { Step = 20, Stroke = new SolidColorBrush(Color.FromArgb(255, 245, 245, 245)) },//分割线

            //Sections = new SectionsCollection()//轴显示的线段
        };
        #endregion

        #region 预警线

        private string _sectionValues = "<未配置>";

        /// <summary>
        /// 显示预警线的值(格式90,50)
        /// </summary>
        public string SectionValues
        {
            get { return _sectionValues; }
            set
            {
                SetProperty(ref _sectionValues, value);
            }
        }

        /// <summary>
        /// 预警线集合
        /// </summary>
        //public ObservableCollection<TrendSectionModel> SectionList { get; set; } = new ObservableCollection<TrendSectionModel>();

        private ObservableCollection<TrendSectionModel> _sectionList = new ObservableCollection<TrendSectionModel>();

        /// <summary>
        /// 预警线集合
        /// </summary>
        public ObservableCollection<TrendSectionModel> SectionList
        {
            get => _sectionList;
            set
            {
                SetProperty(ref _sectionList, value);

                //预警值
                SectionValues = (value.Count == 0 ? "<未配置>" : string.Join(",", value.Select(s => s.Value)));

                //图表要显示预警线
                Axis.Sections.Clear();
                foreach (TrendSectionModel trendSectionModel  in value)
                {
                    Axis.Sections.Add(trendSectionModel.Section);
                }
            }
        }

        //添加预警线命令
        public DelegateCommand AddSectionCommand => new DelegateCommand(() =>
        {
            //集合显示
            TrendSectionModel tsmodel = new TrendSectionModel();
            SectionList.Add(tsmodel);

            //图表显示 属性值改了后才调用
            tsmodel.ValueChanged = new Action(() =>
            {
                SectionValues = string.Join(",", SectionList.Select(s => s.Value));//将所有预警值用,连接起来并显示
                Axis.Model.Chart.Updater.Run(false, true);//刷新
            });
            
            Axis.Sections.Add(tsmodel.Section);//线段加进来了
            Axis.Model.Chart.Updater.Run(false, true);//刷新
        });

        //删除预警线
        public DelegateCommand<TrendSectionModel> DeleteSectionCommand => new DelegateCommand<TrendSectionModel>(model =>
        {
            //集合显示
            SectionList.Remove(model);

            //将所有预警值用,连接起来并显示
            SectionValues = (SectionList.Count == 0 ? "<未配置>" : string.Join(",", SectionList.Select(s => s.Value)));

            //图表显示
            Axis.Sections.Remove(model.Section);
            Axis.Model.Chart.Updater.Run(false, true);//刷新
        });

        #endregion
    }
}
