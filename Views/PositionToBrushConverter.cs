using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace TableTennis.Views
{
    public sealed class PositionToBrushConverter : IValueConverter
    {
        public SolidColorBrush FirstPlaceBrush { get; set; } = new(new Color(255, 212, 175, 55));
        public SolidColorBrush SecondPlaceBrush { get; set; } = new(new Color(255, 192, 192, 192));
        public SolidColorBrush ThirdPlaceBrush { get; set; } = new(new Color(255, 205, 127, 50));
        public SolidColorBrush CommonBrush { get; set; } = new(new Color(255, 255, 255, 255));
        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not int position)
                throw new ArgumentException(string.Empty, nameof(value));
            return position switch
            {
                1 => FirstPlaceBrush,
                2 => SecondPlaceBrush,
                3 => ThirdPlaceBrush,
                _ => CommonBrush
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}