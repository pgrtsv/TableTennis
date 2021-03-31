using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Color = System.Drawing.Color;

namespace TableTennis.Views
{
    public class WinLoseBrushConverter: IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool isWinner))
                return null;
            return isWinner 
                ? Brush.Parse("#3ea009")
                : Brush.Parse("#d90738");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}