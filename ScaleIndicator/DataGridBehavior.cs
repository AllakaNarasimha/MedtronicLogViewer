using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace ScaleIndicator
{
    public static class DataGridBehavior
    {
        public static readonly DependencyProperty ScrollSelectedItemIntoViewProperty =
        DependencyProperty.RegisterAttached(
            "ScrollSelectedItemIntoView",
            typeof(bool),
            typeof(DataGridBehavior),
            new PropertyMetadata(false, OnScrollSelectedItemIntoViewChanged));

        public static bool GetScrollSelectedItemIntoView(DataGrid dataGrid)
        {
            return (bool)dataGrid.GetValue(ScrollSelectedItemIntoViewProperty);
        }

        public static void SetScrollSelectedItemIntoView(DataGrid dataGrid, bool value)
        {
            dataGrid.SetValue(ScrollSelectedItemIntoViewProperty, value);
        }

        private static void OnScrollSelectedItemIntoViewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGrid dataGrid && (bool)e.NewValue)
            {
                dataGrid.SelectionChanged += DataGrid_SelectionChanged;
            }
        }

        private static void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (sender is DataGrid dataGrid && dataGrid.SelectedItem != null)
            {
                dataGrid.Focus();
                dataGrid.ScrollIntoView(dataGrid.SelectedItem);
            }
        }
    }

}
