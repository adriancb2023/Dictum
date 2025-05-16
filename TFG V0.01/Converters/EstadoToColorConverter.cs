using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace TFG_V0._01.Converters
{
    public class EstadoToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string estado = value as string;
            switch (estado?.ToLower())
            {
                case "cerrado": return new SolidColorBrush(Colors.Gray);
                case "abierto": return new SolidColorBrush(Colors.LimeGreen);
                case "pendiente": return new SolidColorBrush(Colors.Gold);
                case "revisado": return new SolidColorBrush(Colors.DodgerBlue);
                default: return new SolidColorBrush(Colors.LightGray);
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
