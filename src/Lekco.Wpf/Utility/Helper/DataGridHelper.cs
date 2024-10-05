#nullable disable
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Lekco.Wpf.Utility.Helper
{
    // https://www.cnblogs.com/luqingfei/p/12697212.html
    public static class DataGridHelper
    {
        #region DisplayRowNumber
        public static bool GetDisplayRowNumber(DependencyObject obj)
        {
            return (bool)obj.GetValue(DisplayRowNumberProperty);
        }

        [AttachedPropertyBrowsableForType(typeof(DataGrid))]
        public static void SetDisplayRowNumber(DependencyObject obj, bool value)
        {
            obj.SetValue(DisplayRowNumberProperty, value);
        }

        /// <summary>
        /// 设置是否显示行号
        /// </summary>
        public static readonly DependencyProperty DisplayRowNumberProperty =
            DependencyProperty.RegisterAttached("DisplayRowNumber",
                                                typeof(bool),
                                                typeof(DataGridHelper),
                                                new PropertyMetadata(false, OnDisplayRowNumberChanged));

        private static void OnDisplayRowNumberChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DataGrid grid = d as DataGrid;
            if (grid == null)
            {
                return;
            }

            if ((bool)e.NewValue)
            {
                grid.LoadingRow += OnGridLoadingRow;
                grid.UnloadingRow += OnGridUnloadingRow;
            }
            else
            {
                grid.LoadingRow -= OnGridLoadingRow;
                grid.UnloadingRow -= OnGridUnloadingRow;
            }
        }

        private static void RefreshDataGridRowNumber(object sender)
        {
            DataGrid grid = sender as DataGrid;
            if (grid == null)
            {
                return;
            }

            foreach (var item in grid.Items)
            {
                var row = (DataGridRow)grid.ItemContainerGenerator.ContainerFromItem(item);
                if (row == null)
                {
                    return;
                }
                row.Header = row.GetIndex() + 1;
            }
        }

        private static void OnGridUnloadingRow(object sender, DataGridRowEventArgs e)
        {
            RefreshDataGridRowNumber(sender);
        }

        private static void OnGridLoadingRow(object sender, DataGridRowEventArgs e)
        {
            RefreshDataGridRowNumber(sender);
        }

        #endregion

        #region ColumnsProperty

        public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.RegisterAttached(
                "Columns",
                typeof(IEnumerable<DataGridColumn>),
                typeof(DataGridHelper),
                new PropertyMetadata(null, OnColumnsChanged)
            );

        public static void SetColumns(DependencyObject element, IEnumerable<DataGridColumn> value)
        {
            element.SetValue(ColumnsProperty, value);
        }

        public static IEnumerable<DataGridColumn> GetColumns(DependencyObject element)
        {
            return (IEnumerable<DataGridColumn>)element.GetValue(ColumnsProperty);
        }

        private static void OnColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGrid dataGrid && dataGrid != null)
            {
                dataGrid.Columns.Clear();
                if (e.NewValue is IEnumerable<DataGridColumn> columns)
                {
                    foreach (var column in columns)
                    {
                        dataGrid.Columns.Add(column);
                    }
                }
            }
        }

        #endregion
    }
}
