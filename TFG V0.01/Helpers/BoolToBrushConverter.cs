using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace TFG_V0._01.Helpers
{
    public class BoolToBrushConverter : IValueConverter
    {
        public Brush SelectedBrush { get; set; }
        public Brush UnselectedBrush { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? SelectedBrush : UnselectedBrush;
            }
            return UnselectedBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 