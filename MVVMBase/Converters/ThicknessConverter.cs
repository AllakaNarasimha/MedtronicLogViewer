using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Media3D;

namespace MVVMBase.Converters
{
    public class ThicknessConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 5)
            {
                double width = System.Convert.ToDouble(values[0]);
                double left = System.Convert.ToDouble(values[1]);
                double top = System.Convert.ToDouble(values[2]);
                double right = System.Convert.ToDouble(values[3]);
                double bottom = System.Convert.ToDouble(values[4]);

                double gridColumnWidth = width / 100;
                if (values.Length == 6)
                {
                    if (values[5] is ColumnDefinitionCollection columnDefinitions)
                    {
                        int sIndex = (int)left;
                        if (columnDefinitions.Count > sIndex)
                        {
                            var cold = columnDefinitions[sIndex];
                            if (cold != null)
                            {
                                gridColumnWidth = cold.Width.Value;                                
                            }
                        }
                       
                    }
                }
                
                if (width > 0 && left > 0)
                left = gridColumnWidth * left;

                //return new System.Windows.Thickness(left + gridColumnWidth, top, right, bottom);
                return new System.Windows.Thickness(left + (1 * gridColumnWidth), top, right, bottom);
            }
            else
            {
                double paramWidth = 0;
                if(double.TryParse(parameter as string, out double widht)){
                    paramWidth = widht;
                }
                
                if (values.Length == 1 )
                {
                    var left = paramWidth / System.Convert.ToDouble(values[0]);
                    return new System.Windows.Thickness(left, 0, 0, 0);
                }
                return new System.Windows.Thickness(0, 0, 0, 0);
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            if (value is System.Windows.Thickness thickness)
            {
                return new object[] { thickness.Left, thickness.Top, thickness.Right, thickness.Bottom };
            }

            throw new InvalidOperationException("Unsupported value type");
        }

    }
}
