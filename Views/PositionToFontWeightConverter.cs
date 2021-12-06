using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace TableTennis.Views
{
    public sealed class PositionToFontWeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not int position)
                throw new ArgumentException(string.Empty, nameof(value));
            return position switch
            {
                1 or 2 or 3 => FontWeight.Medium,
                _ => FontWeight.Regular
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}