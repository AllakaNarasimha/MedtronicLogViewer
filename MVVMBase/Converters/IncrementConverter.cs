using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MVVMBase.Converters
{
    public class IncrementConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int numericValue && parameter is int increment)
            {
                // Increment the numeric value
                return numericValue;// + increment;
            }

            if (value != null)
            {
                int intValue;
                int parmValue;
                if (int.TryParse(value.ToString(), out intValue) && int.TryParse(parameter.ToString(), out parmValue))
                {
                    return intValue + parmValue;
                }
            }

            // Return the original value if conversion is not possible
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // This converter does not support ConvertBack
            throw new NotSupportedException();
        }
    }
}
