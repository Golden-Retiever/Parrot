using Platform.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.ViewModels.DialogWin
{
    /// <summary>
    /// 趋势-选择设备变量视图模型
    /// </summary>
    public class TrendDeviceChooseWinViewModel
    {
        /// <summary>
        /// 趋势
        /// </summary>
        public TrendModel TrendModel { get; set; }

        /// <summary>
        /// 有变量的设备集合
        /// </summary>
        public List<TrendDeviceModel> ChooseDevicesList { get; set; }

        /// <summary>
        /// 颜色集合
        /// </summary>
        public List<string> BrushList { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="trendModel">趋势</param>
        /// <param name="brushList">颜色下拉值</param>
        /// <param name="mainViewModel">主视图模型。需要里面的设备集合</param>
        public TrendDeviceChooseWinViewModel(TrendModel trendModel, List<string> brushList, MainUCViewModel mainViewModel)
        {
            TrendModel = trendModel;
            BrushList = brushList;

            ChooseDevicesList = mainViewModel.DeviceList
                .Where(d => d.DeviceVarList.Count > 0)//必须有变量
                .Select(d => new TrendDeviceModel
                {
                    DeviceName = d.DeviceName,

                    VarList = d.DeviceVarList.Where(t => t.VarType != "Boolean").Select(v => InitDevices(d, v)).ToList()

                }).ToList();
        }

        /// <summary>
        /// 初始化设备变量信息
        /// </summary>
        /// <param name="dm">设备</param>
        /// <param name="vm">变量</param>
        /// <returns></returns>
        private TrendDeviceVarModel InitDevices(DeviceModel dm, DeviceVarModel vm)
        {
            // 检查一下已选择的变量有哪些
            var item = TrendModel.Series.ToList().FirstOrDefault(ts => ts.DeviceNum == dm.DeviceNum && ts.VarNum == vm.VarNum);

            TrendDeviceVarModel varModel = new TrendDeviceVarModel();//趋势设备变量

            varModel.IsSelected = (item != null);
            varModel.DeviceNum = dm.DeviceNum;
            varModel.VarNum = vm.VarNum;
            varModel.VarName = vm.VarName;
            varModel.VarType = vm.VarType;

            varModel.ANum = (item == null) ? TrendModel.AxisList[0].ANum : item.ANum;
            varModel.Color = (item == null) ? BrushList[new Random().Next(0, BrushList.Count)] : item.Color;

            varModel.PropertyChanged += (se, ev) =>
            {
                if (ev.PropertyName == "IsSelected")//如果是IsSelected值改变
                {
                    SeriesChanged(se as TrendDeviceVarModel);
                }
                else if (ev.PropertyName == "Color" || ev.PropertyName == "ANum")
                {
                    var m = se as TrendDeviceVarModel;
                    if (m == null)
                    {
                        return;
                    }

                    var si = TrendModel.Series.FirstOrDefault(ts => ts.DeviceNum == m.DeviceNum && ts.VarNum == m.VarNum);
                    if (si == null)
                    {
                        return;
                    }

                    si.Color = m.Color;
                    si.ANum = m.ANum;
                }
            };

            return varModel;
        }

        /// <summary>
        /// 序列属性发生变化
        /// </summary>
        /// <param name="model"></param>
        private void SeriesChanged(TrendDeviceVarModel model)
        {
            if (TrendModel == null)
            {
                return;
            }
            if (model.IsSelected)
            {
                // 添加序列
                TrendSeriesModel tsModel = new TrendSeriesModel
                {
                    ANum = model.ANum,
                    DeviceNum = model.DeviceNum,
                    VarNum = model.VarNum,
                    Title = model.VarName,
                    Color = model.Color,

                    //获取纵轴index
                    AxisIndexFunc = (num => TrendModel.AxisList.ToList().FindIndex(a => a.ANum == num))
                };
                TrendModel.Series.Add(tsModel);

                TrendModel.ChartSeries.Add(tsModel.Series);//加到显示里面
            }
            else
            {
                // 移除序列
                int index = TrendModel.Series.ToList().FindIndex(s => s.DeviceNum == model.DeviceNum && s.VarNum == model.VarNum);
                TrendModel.Series.RemoveAt(index);

                TrendModel.ChartSeries.RemoveAt(index);//从显示里面移除
            }
        }
    }
}
