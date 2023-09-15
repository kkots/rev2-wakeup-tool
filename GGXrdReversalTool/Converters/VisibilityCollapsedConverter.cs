using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GGXrdReversalTool.Converters;

[ValueConversion(typeof(bool),typeof(Visibility))]
public class VisibilityCollapsedConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not Boolean valueBool) throw new ArgumentException();

        return valueBool ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}