using System;
using System.Windows.Data;

namespace TFG_V0._01.Converters
{
    public class BoolToNumberConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool isOdd)
            {
                return isOdd ? 2 : 1;
            }
            return 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}