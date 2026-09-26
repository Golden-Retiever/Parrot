using Microsoft.Win32;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Platform.DBAccess;
using Platform.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Platform.ViewModels.FunctionUC
{
    /// <summary>
    /// 报表视图模型
    /// </summary>
    public class ReportUCViewModel:BindableBase
    {
        IDataAccess _dataAccess;//数据库访问
        public ReportUCViewModel(IDataAccess dataAccess)
        {
            _dataAccess=dataAccess;

            Refresh();//获取数据

            //绑定已经选择的列
            foreach (DataGridColumnModel dataGridColumnModel in AllColumnList)
            {
                ChangedColumn(dataGridColumnModel);
            }

            //AllColumnList =new  AllColumnList.OrderBy(t => t.Index);
        }

        /// <summary>
        /// 所有可能显示的列，可供选择的列
        /// </summary>
        public ObservableCollection<DataGridColumnModel> AllColumnList { get; set; } =
            [
              // 初始化列列表
            new DataGridColumnModel { Header = "设备编号", BindingPath = "DeviceNum", ColumnWidth = 70 },
            new DataGridColumnModel { IsSelected = true, Header = "设备名称", BindingPath = "DeviceName", ColumnWidth = 90 },
            new DataGridColumnModel { Header = "变量编号", BindingPath = "VarNum", ColumnWidth = 70 },
            new DataGridColumnModel { IsSelected = true, Header = "变量名称", BindingPath = "VarName", ColumnWidth = 90},
            new DataGridColumnModel { Header = "平均值", BindingPath = "AvgValue", ColumnWidth = 90 },
            new DataGridColumnModel { Header = "最大值", BindingPath = "MaxValue", ColumnWidth = 90 },
            new DataGridColumnModel { Header = "最小值", BindingPath = "MinValue", ColumnWidth = 90 },
            new DataGridColumnModel { Header = "报警触发次数", BindingPath = "AlarmCount", ColumnWidth = 90 },
            new DataGridColumnModel { Header = "记录次数", BindingPath = "RecordCount", ColumnWidth = 80 },
            new DataGridColumnModel { Header = "最后记录时间", BindingPath = "LastTime", ColumnWidth = 120 }
            ];

        
        /// <summary>
        /// 选择的列集合
        /// </summary>
        public ObservableCollection<DataGridColumn> SelectedColumnList { get; set; } =
           new ObservableCollection<DataGridColumn>();

        private ObservableCollection<RecordStatModel> _allStatList;

        /// <summary>
        /// 统计结果集合
        /// </summary>
        public ObservableCollection<RecordStatModel> AllStatList
        {
            get { return _allStatList; }
            set { SetProperty(ref _allStatList, value); }
        }

        /// <summary>
        /// 刷新数据(从数据库里获取数据)
        /// </summary>
        private void Refresh()
        {
            // 从数据库来的
            var recordsDB = _dataAccess.GetMonitorRecords();

            //GroupBy 分组
            var query = recordsDB.GroupBy(re => new { re.DeviceNum, re.DeviceName, re.VarNum, re.VarName }).Select(ri => new RecordStatModel
            {
                //key取分组属性
                DeviceNum = ri.Key.DeviceNum,
                DeviceName = ri.Key.DeviceName,
                VarNum = ri.Key.VarNum,
                VarName = ri.Key.VarName,

                LastTime = ri.Max(x => x.RecordTime.ToString("yyyy-MM-dd HH:mm:ss")),//最近记录时间
                AvgValue = ri.Average(x => x.RecordValue).ToString("F2"),
                AlarmCount = ri.Count(t => !string.IsNullOrEmpty(t.AlarmNum)),//有报警编号就说明该变量报警了
                MaxValue = ri.Max(x => x.RecordValue),
                MinValue = ri.Min(x => x.RecordValue),
                RecordCount = ri.Count()//所有监控次数
            });

            AllStatList = new ObservableCollection<RecordStatModel>(query);
        }

        ///// <summary>
        ///// 选择列的顺序
        ///// </summary>
        //private int index = 0;

        /// <summary>
        /// 将选择的列加进去/勾掉的列移出去
        /// </summary>
        /// <param name="model"></param>
        private void ChangedColumn(DataGridColumnModel model)
        {
            if (model.IsSelected)
            {
                //model.Index = index++;

                //列样式
                Style style = new System.Windows.Style(typeof(TextBlock));
                style.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Center));
                style.Setters.Add(new Setter(TextBlock.VerticalAlignmentProperty, System.Windows.VerticalAlignment.Center));

                //DataGridTextColumn: DataGridColumn 间接继承
                //DataGridCheckBoxColumn:DataGridColumn
                SelectedColumnList.Add(new DataGridTextColumn
                {
                    Header = model.Header,
                    Binding = new Binding(model.BindingPath),
                    MinWidth = model.ColumnWidth,
                    ElementStyle = style
                });
            }
            else
            {
                var column = SelectedColumnList.FirstOrDefault(c => c.Header.ToString() == model.Header);
                if (column != null)
                {
                    SelectedColumnList.Remove(column);
                }
            }
        }

        /// <summary>
        /// 勾选/勾掉 列
        /// </summary>
        public DelegateCommand<DataGridColumnModel> ChooseColumnCommand => new DelegateCommand<DataGridColumnModel>(ChangedColumn);

        /// <summary>
        /// 刷新
        /// </summary>
        public DelegateCommand RefreshCommand => new DelegateCommand(Refresh);

        #region 报表数据导出

        /// <summary>
        /// 导出命令
        /// </summary>
        public DelegateCommand ExportCommand => new DelegateCommand(Exeport);

        /// <summary>
        /// 导出报表
        /// </summary>
        private void Exeport()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            try
            {
                // 设置对话框属性
                saveFileDialog.Title = "选择Excel保存路径";
                saveFileDialog.Filter = "Excel 文件 (*.xlsx)|*.xlsx"; // 格式筛选

                saveFileDialog.RestoreDirectory = true; // 恢复上次目录
                saveFileDialog.FileName = $"报表数据_{DateTime.Now:yyyyMMddHHmmss}"; // 默认文件名（带时间戳）

                if (saveFileDialog.ShowDialog() == true)
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
            var selectedCols = AllColumnList.Where(c => c.IsSelected).ToList();//选择的列  ["设备编号","设备名称"]
            IRow headerRow = sheet.CreateRow(0); // 第一行（索引从0开始）
            for (int i = 0; i < selectedCols.Count; i++)
            {
                ICell cell = headerRow.CreateCell(i);
                cell.SetCellValue(selectedCols[i].Header);
                cell.CellStyle = headerStyle; // 应用表头样式
            }

            // 7. 写入数据行
            for (int i = 0; i < AllStatList.Count; i++)
            {
                IRow dataRow = sheet.CreateRow(i + 1); // 从第二行开始（表头占第一行）

                // 灵活配置的列，列没有顺序
                for (int j = 0; j < selectedCols.Count; j++)
                {
                    //["设备编号 DeviceNum  D201600000000", "设备名称 DeviceName"]
                    PropertyInfo? pi = AllStatList[i].GetType().GetProperty(selectedCols[j].BindingPath, BindingFlags.Instance | BindingFlags.Public);
                    if (pi == null)
                    {
                        continue;
                    }
                    //Device.DeviceNum
                    var v = pi.GetValue(AllStatList[i]);//获取值

                    // 赋值并应用样式
                    dataRow.CreateCell(j).SetCellValue(v.ToString());
                }

                //为数据单元格应用样式
                for (int j = 0; j < selectedCols.Count; j++)
                {
                    dataRow.GetCell(j).CellStyle = dataStyle;
                }
            }

            // 8. 自动调整列宽（可选）
            for (int i = 0; i < selectedCols.Count; i++)
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
        #endregion
    }
}
