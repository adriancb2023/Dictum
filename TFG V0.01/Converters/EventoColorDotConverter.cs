using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Collections.Generic;

namespace TFG_V0._01.Converters
{
    public class EventoColorDotConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // values[0]: DataContext (ventana), values[1]: Date
            if (values.Length == 2 && values[0] != null && values[1] is DateTime date)
            {
                var window = values[0];
                var getDiasConEventoColor = window.GetType().GetMethod("GetDiasConEventoColor");
                if (getDiasConEventoColor != null)
                {
                    var dict = getDiasConEventoColor.Invoke(window, null) as Dictionary<DateTime, string>;
                    if (dict != null && dict.TryGetValue(date.Date, out var color))
                    {
                        return (SolidColorBrush)(new BrushConverter().ConvertFrom(color));
                    }
                }
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 