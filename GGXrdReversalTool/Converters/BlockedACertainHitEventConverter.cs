using System;
using System.Globalization;
using System.Windows.Data;

namespace GGXrdReversalTool.Converters;

public class BlockedACertainHitEventConverter : IMultiValueConverter 
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var min = (int)values[0];
        var max = (int)values[1];

        return min == max ? $"When blocking hit number {min}" : $"When blocking hit number from {min} to {max}";
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}