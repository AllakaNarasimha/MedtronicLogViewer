using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MVVMBase.Converters
{
    public class AutoWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 && values[0] is double windowWidth && values[1] is double reducibleWidth)
            {
                int i = 1;
                double widthDifference = windowWidth - 40; //20 is margin
                while (values.Length > i && values[i] is double)
                {
                    widthDifference -= (double)values[i];
                    i++;
                }
                return widthDifference >= 0 ? widthDifference : 0.0;
            }
            else if (values.Length == 1 && values[0] is double winWidth)
            {
                double widthDifference = winWidth - 40; //20 is margin

                if (double.TryParse(parameter as string, out double reduce))
                {
                    widthDifference -= reduce;
                }
                return widthDifference >= 0 ? widthDifference : 0.0;
            }
            return 480.0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
