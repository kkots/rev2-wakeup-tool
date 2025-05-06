using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace GGXrdReversalTool.Converters;

[ValueConversion(typeof(bool),typeof(Brush))]
public class IsSelectedToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not Boolean valueBool) throw new ArgumentException();

        return valueBool ? Brushes.Yellow : Brushes.Black;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}