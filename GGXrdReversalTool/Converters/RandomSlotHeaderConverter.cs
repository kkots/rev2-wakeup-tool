using System;
using System.Globalization;
using System.Windows.Data;

namespace GGXrdReversalTool.Converters;


[ValueConversion(typeof(int), typeof(string))]
public class RandomSlotHeaderConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int slotNumber)
        {
            return $"Use slot {slotNumber}";
        }

        return "Use slot ";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}