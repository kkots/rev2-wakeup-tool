using System;
using System.Globalization;
using System.Windows.Data;

namespace GGXrdReversalTool.Converters;

public class DelayAirRecoveryHitCountEventConverter : IMultiValueConverter 
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var min = (int)values[0];
        var max = (int)values[1];

        return min == max ? $"Only when combo count is {min} {selectPlural(min, "hit", "hits")}"
                          : $"Only when combo count is from {min} to {max} hits";
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
    private string selectPlural(int count, string singular, string plural) =>
        count == 1 ? singular : plural;
}