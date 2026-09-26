using Microsoft.Win32;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Platform.DBAccess;
using Platform.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using static System.Net.Mime.MediaTypeNames;

namespace Platform.ViewModels.FunctionUC
{
    /// <summary>
    /// 报警视图模型
    /// </summary>
    public class AlarmUCViewModel : BindableBase
    {
        IDataAccess _dataAccess;//数据库访问
        IEventAggregator _eventAggregator;

        public AlarmUCViewModel(IDataAccess dataAccess, IEventAggregator eventAggregator)
        {
            _dataAccess = dataAccess;
            _eventAggregator = eventAggregator;

            //InitAlarms();//显示数据
        }

        /// <summary>
        /// 用来存放所有符合查询条件的报警数据
        /// </summary>
        List<DeviceAlarmModel> AllDeviceAlarmList = new List<DeviceAlarmModel>();

        private ObservableCollection<DeviceAlarmModel> _deviceAlarmList = new ObservableCollection<DeviceAlarmModel>();

        /// <summary>
        /// 显示的报警集合
        /// </summary>
        public ObservableCollection<DeviceAlarmModel> DeviceAlarmList
        {
            get
            {
                return _deviceAlarmList;
            }
            set
            {
                SetProperty(ref _deviceAlarmList, value);
            }
        }

        /// <summary>
        /// 查询关键字
        /// </summary>
        public string KeyWord { get; set; }

        private int _pageSize = 10;

        /// <summary>
        /// 页码集合
        /// </summary>
        public ObservableCollection<int> PageNumberButtons { get; } = new ObservableCollection<int>();

        /// <summary>
        /// 每页多少条数据
        /// </summary>
        public int PageSize
        {
            get { return _pageSize; }
            set
            {
                _pageSize = value;

                #region 第一种 不需要查询数据库。代码多。构造函数不调用InitAlarms();

                ////1、AllDeviceAlarmList 取出第1页数据。
                //PageIndex = 1;
                //ShowPageData();

                //// 2、重新算总页数。
                //TotalPageCount = 1;

                //if (AllDeviceAlarmList.Count > 0)
                //{
                //    TotalPageCount = (AllDeviceAlarmList.Count % value == 0) ? (AllDeviceAlarmList.Count / value) : (AllDeviceAlarmList.Count / value + 1);
                //}

                ////初始化页码按钮
                //PageNumberButtons.Clear();
                //for (int page = 1; page <= TotalPageCount; page++)
                //{
                //    PageNumberButtons.Add(page);
                //}

                #endregion

                #region 第二种 代码少。需要查询数据库。构造函数调用InitAlarms();
                InitAlarms();
                #endregion
            }
        }

        private int _pageIndex = 1;//默认第1页

        /// <summary>
        /// 页码  第几页
        /// </summary>
        public int PageIndex
        {
            get { return _pageIndex; }
            set
            {
                SetProperty(ref _pageIndex, value);
            }
        }

        /// <summary>
        /// 总页数
        /// </summary>
        int TotalPageCount = 1;

        /// <summary>
        /// 取出所有数据并计算总页数，默认显示第1页数据
        /// </summary>
        private void InitAlarms()
        {
            AllDeviceAlarmList.Clear();//总记录集合清空
            PageNumberButtons.Clear();//页码集合也要清空

            var alarmEntityList = _dataAccess.GetDeviceAlarmList(KeyWord);
            foreach (var alarmEntity in alarmEntityList)
            {
                //放到所有的报警集合
                AllDeviceAlarmList.Add(new DeviceAlarmModel(_dataAccess, _eventAggregator)
                {
                    Index = AllDeviceAlarmList.Count + 1,//序号
                    Account = alarmEntity.Account,
                    DeviceNum = alarmEntity.DeviceNum,
                    DeviceName = alarmEntity.DeviceName,
                    VarNum = alarmEntity.VarNum,
                    VarName = alarmEntity.VarName,
                    AlarmValue = alarmEntity.AlarmValue,
                    State = alarmEntity.State,
                    AlarmNum = alarmEntity.AlarmNum,
                    RecordTime = alarmEntity.RecordTime,
                    SolveTime = alarmEntity.SolveTime,
                    AlarmContent = alarmEntity.AlarmContent,
                });
            }

            //计算总页数
            TotalPageCount = 1;

            if (AllDeviceAlarmList.Count > 0)
            {
                TotalPageCount = (AllDeviceAlarmList.Count % PageSize == 0) ? (AllDeviceAlarmList.Count / PageSize) : (AllDeviceAlarmList.Count / PageSize + 1);
            }

            //初始化页码按钮
            for (int page = 1; page <= TotalPageCount; page++)
            {
                PageNumberButtons.Add(page);
            }

            PageIndex = 1;//默认第1页
            ShowPageData();//显示当前页的数据
        }

        /// <summary>
        /// 显示当前页的数据
        /// </summary>
        private void ShowPageData()
        {
            //假设每页10条数据，取第5页   思路:跳过前面5-1页数据，再取10条数据
            DeviceAlarmList = new ObservableCollection<DeviceAlarmModel>(AllDeviceAlarmList.Skip((PageIndex - 1) * PageSize).Take(PageSize));
            
            //优化——GetRange()，从startIndex开始复制count个值。
            //int startIndex = (PageIndex - 1) * PageSize;
            //int count = Math.Min(PageSize, AllDeviceAlarmList.Count - startIndex);
            //DeviceAlarmList = new ObservableCollection<DeviceAlarmModel>(
            //    AllDeviceAlarmList.GetRange(startIndex, count)
            //);
        }

        /// <summary>
        /// 刷新
        /// </summary>
        public DelegateCommand RefreshCommand => new DelegateCommand(InitAlarms);

        #region 分页命令

        /// <summary>
        /// 首页
        /// </summary>
        public DelegateCommand FirstCommand => new DelegateCommand(() =>
        {
            PageIndex = 1;
            ShowPageData();
        });

        /// <summary>
        /// 上一页
        /// </summary>
        public DelegateCommand PreCommand => new DelegateCommand(() =>
        {
            if (PageIndex == 1)
            {
                MessageBox.Show("已经是第一页了");
                return;
            }
            PageIndex--;
            ShowPageData();
        });

        /// <summary>
        /// 下一页
        /// </summary>
        public DelegateCommand NextCommand => new DelegateCommand(() =>
        {
            if (PageIndex == TotalPageCount)
            {
                MessageBox.Show("已经是最后一页了");
                return;
            }

            PageIndex++;
            ShowPageData();
        });

        /// <summary>
        /// 末页页
        /// </summary>
        public DelegateCommand LastCommand => new DelegateCommand(() =>
        {
            PageIndex = TotalPageCount;
            ShowPageData();
        });

        /// <summary>
        /// 点击页码命令
        /// </summary>
        public DelegateCommand<object> JumpToPageCommand => new DelegateCommand<object>((obj) =>
        {
            int pageIndex = (int)obj;

            PageIndex = pageIndex;
            ShowPageData();
        });
        #endregion

        #region 导出

        public DelegateCommand ExportCommand => new DelegateCommand(Export);

        private void Export()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            try
            {
                // 设置对话框属性
                saveFileDialog.Title = "选择Excel保存路径";
                saveFileDialog.Filter = "Excel 文件 (*.xlsx)|*.xlsx"; // 格式筛选
                saveFileDialog.RestoreDirectory = true; // 恢复上次目录
                saveFileDialog.FileName = $"报警数据_{DateTime.Now:yyyyMMddHHmmss}"; // 默认文件名（带时间戳）

                // 显示对话框并判断是否确认
                bool? isSelectPath = saveFileDialog.ShowDialog();
                if (isSelectPath != null && isSelectPath == true)
                {
                    ExportToXlsx(saveFileDialog.FileName);
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                saveFileDialog = null;
            }

        }

        /// <summary>
        /// 导出为 .xlsx 格式（Excel 2007+）
        /// </summary>
        /// <param name="filePath">保存路径</param>
        private void ExportToXlsx(string filePath)
        {
            // 1. 创建工作簿（XSSFWorkbook 对应 .xlsx）
            IWorkbook workbook = new XSSFWorkbook();

            // 2. 创建工作表并命名
            ISheet sheet = workbook.CreateSheet("用户数据");

            // 3. 创建表头样式（可选，美化表头）
            ICellStyle headerStyle = workbook.CreateCellStyle();
            // 设置背景色
            headerStyle.FillForegroundColor = IndexedColors.LightBlue.Index;
            headerStyle.FillPattern = FillPattern.SolidForeground;
            // 设置字体
            IFont headerFont = workbook.CreateFont();
            headerFont.FontName = "微软雅黑";
            headerFont.FontHeightInPoints = 12;
            headerFont.IsBold = true;
            headerStyle.SetFont(headerFont);
            // 设置边框
            headerStyle.BorderTop = BorderStyle.Thin;
            headerStyle.BorderBottom = BorderStyle.Thin;
            headerStyle.BorderLeft = BorderStyle.Thin;
            headerStyle.BorderRight = BorderStyle.Thin;
            // 居中对齐
            headerStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;

            // 4. 创建数据行样式（可选）
            ICellStyle dataStyle = workbook.CreateCellStyle();
            dataStyle.BorderTop = BorderStyle.Thin;
            dataStyle.BorderBottom = BorderStyle.Thin;
            dataStyle.BorderLeft = BorderStyle.Thin;
            dataStyle.BorderRight = BorderStyle.Thin;
            dataStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;

            // 5. 写入表头
            string[] headers = { "序号", "设备编号", "设备名称", "变量编号", "变量名称", "报警信息", "记录值", "记录时间", "处理时间", "记录人", "当前状态" };
            IRow headerRow = sheet.CreateRow(0); // 第一行（索引从0开始）
            for (int i = 0; i < headers.Length; i++)
            {
                ICell cell = headerRow.CreateCell(i);
                cell.SetCellValue(headers[i]);
                cell.CellStyle = headerStyle; // 应用表头样式
            }

            // 7. 写入数据行
            for (int i = 0; i < AllDeviceAlarmList.Count; i++)
            {
                IRow dataRow = sheet.CreateRow(i + 1); // 从第二行开始（表头占第一行）

                // 赋值并应用样式
                dataRow.CreateCell(0).SetCellValue(AllDeviceAlarmList[i].Index);
                dataRow.CreateCell(1).SetCellValue(AllDeviceAlarmList[i].DeviceNum);
                dataRow.CreateCell(2).SetCellValue(AllDeviceAlarmList[i].DeviceName);
                dataRow.CreateCell(3).SetCellValue(AllDeviceAlarmList[i].VarNum);
                dataRow.CreateCell(4).SetCellValue(AllDeviceAlarmList[i].VarName);
                dataRow.CreateCell(5).SetCellValue(AllDeviceAlarmList[i].AlarmContent);
                dataRow.CreateCell(6).SetCellValue(AllDeviceAlarmList[i].AlarmValue);
                dataRow.CreateCell(7).SetCellValue(AllDeviceAlarmList[i].RecordTime.ToString("yyyy-MM-dd HH:mm:ss"));
                dataRow.CreateCell(8).SetCellValue(AllDeviceAlarmList[i].SolveTime == null ? "" : Convert.ToDateTime(AllDeviceAlarmList[i].SolveTime).ToString("yyyy-MM-dd HH:mm:ss"));
                dataRow.CreateCell(9).SetCellValue(AllDeviceAlarmList[i].Account);
                dataRow.CreateCell(10).SetCellValue(AllDeviceAlarmList[i].StateName);

                //为数据单元格应用样式
                for (int j = 0; j < headers.Length; j++)
                {
                    dataRow.GetCell(j).CellStyle = dataStyle;
                }
            }

            // 8. 自动调整列宽（可选）
            for (int i = 0; i < headers.Length; i++)
            {
                sheet.AutoSizeColumn(i);
            }

            // 9. 写入文件
            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                workbook.Write(fs);
            }

            // 释放资源
            workbook.Close();
        }

        /// <summary>
        /// 导出为 .xls 格式（Excel 97-2003）
        /// </summary>
        /// <param name="filePath">保存路径</param>
        private void ExportToXls(string filePath)
        {
            // 仅需将 XSSFWorkbook 替换为 HSSFWorkbook，其余逻辑与 .xlsx 一致
            IWorkbook workbook = new HSSFWorkbook();
            ISheet sheet = workbook.CreateSheet("用户数据");

            // 以下逻辑与 ExportToXlsx 完全相同（复制上面的表头样式、数据写入等代码）
            // 【省略重复代码，可直接复用 ExportToXlsx 中的逻辑】

            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                workbook.Write(fs);
            }
            workbook.Close();
        }
        #endregion
    }
}
