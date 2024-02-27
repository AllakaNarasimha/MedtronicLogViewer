using System.Globalization;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using BusinessLayer;
using BusinessLayer.Models;

namespace MVVMBase.Converters
{
    public class GridSpanValidator :  IValueConverter
    {   
        /*// You can adjust the condition in this method
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null && values.Length > 0 && values[0] is int index)
            {
                // Get the index of the row
                //int index = GetRowIndex(row);
                 
                //if (values[1] is int timelineIndex)
                {
                    int timelineIndex = LocalCache.GetActiveIndex();
                    var res = LocalCache.GetItemByKey(timelineIndex);
                    if (res.Item1 < index && res.Item2 > index)
                    {
                        return true;// Brushes.Red;
                    }
                    
                }
            }
            return false;// Brushes.Black;
           
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
        */

        private int GetRowIndex(DataGridRow row)
        {
            var dataGrid = FindParent<DataGrid>(row);

            if (dataGrid != null)
            {
                return dataGrid.ItemContainerGenerator.IndexFromContainer(row);
            }

            return -1;
        }

        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null)
            {
                return null;
            }

            if (parentObject is T parent)
            {
                return parent;
            }

            return FindParent<T>(parentObject);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is int index)
            {
                int timelineIndex = LocalCache.GetActiveIndex();
                var res = LocalCache.GetItemByKey(timelineIndex);
                if (res.Item1 == index)
                {
                    return true;
                }
                if (res.Item1 <= index && res.Item2 > index)
                {
                    return true;// Brushes.Red;
                }
            }
            return false;// Brushes.Black;

            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
