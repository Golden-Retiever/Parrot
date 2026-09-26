using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Platform.Helper
{
    /// <summary>
    /// 通过附加属性Columns，在XAML中动态绑定ObservableCollection<DataGridColumn>到DataGrid的Columns集合，实现列的动态增删（响应集合变化）。
    /// </summary>
    public class DataGridHelper
    {
        public static ObservableCollection<DataGridColumn> GetColumns(DependencyObject obj)
        {
            return (ObservableCollection<DataGridColumn>)obj.GetValue(ColumnsProperty);
        }

        public static void SetColumns(DependencyObject obj, ObservableCollection<DataGridColumn> value)
        {
            obj.SetValue(ColumnsProperty, value);
        }
        public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.RegisterAttached("Columns", typeof(ObservableCollection<DataGridColumn>), typeof(DataGridHelper), new PropertyMetadata(null, new PropertyChangedCallback(OnPropertyChanged)));

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DataGrid dataGrid = (d as DataGrid);
            if (dataGrid == null)
            {
                return;
            }

            // 监听Columns集合的变化（增/删/清空）
            GetColumns(d).CollectionChanged += (se, ev) =>
            {
                //dataGrid = (d as DataGrid);
                dataGrid.Columns.Clear(); // 清空原有列

                // 遍历绑定的集合，重新添加所有列
                foreach (var item in GetColumns(dataGrid))
                {
                    dataGrid.Columns.Add(item);
                }
            };

            // 初始化：属性首次赋值时，将集合中的列添加到DataGrid
            foreach (var item in GetColumns(d))
            {
                dataGrid.Columns.Add(item);
            }
        }
    }
}
