using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace TableTennis.Views
{
    public sealed class PositionToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not int position)
                throw new ArgumentException(string.Empty, nameof(value));
            if (parameter is not bool isInversed)
                isInversed = false;
            return position switch
            {
                1 or 2 or 3 => !isInversed,
                _ => isInversed
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}