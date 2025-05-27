using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace TFG_V0._01.Helpers
{
    public class BoolToBrushConverter : IValueConverter
    {
        public Brush SelectedBrush { get; set; } = new SolidColorBrush(Color.FromRgb(33, 150, 243)); // Azul
        public Brush UnselectedBrush { get; set; } = new SolidColorBrush(Color.FromArgb(0,0,0,0)); // Transparente

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isSelected = value is bool b && b;
            return isSelected ? SelectedBrush : UnselectedBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 