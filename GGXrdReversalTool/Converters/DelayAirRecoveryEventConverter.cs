using System;
using System.Globalization;
using System.Windows.Data;

namespace GGXrdReversalTool.Converters;

public class DelayAirRecoveryEventConverter : IMultiValueConverter 
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var min = (int)values[0];
        var max = (int)values[1];

        return min == max ? $"Delay tech by {min} {selectPlural(min, "frame", "frames")}"
                          : $"Delay tech by a random amount from {min} to {max} frames";
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
    private string selectPlural(int count, string singular, string plural) =>
        count == 1 ? singular : plural;
}