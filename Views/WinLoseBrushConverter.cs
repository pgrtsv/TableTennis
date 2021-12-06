using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace TableTennis.Views
{
    public class WinLoseBrushConverter: IValueConverter
    {
        public IBrush WinnerBrush { get; set; } = Brush.Parse("#3ea009");
        public IBrush LoserBrush { get; set; } = Brush.Parse("#d90738");
        
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool isWinner))
                return null;
            return isWinner 
                ? WinnerBrush
                : LoserBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}