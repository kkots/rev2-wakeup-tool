using System;
using System.Globalization;
using System.Windows.Data;

namespace GGXrdReversalTool.Converters;

public class PeriodicEventConverter : IMultiValueConverter 
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var min = (int)values[0];
        var max = (int)values[1];

        return min == max ? $"Every {min / 60.0F:F2} {selectPlural(min, "second", "seconds")}"
                          : $"Every {min / 60.0F:F2} to {max / 60.0F:F2} seconds";
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
    private string selectPlural(int count, string singular, string plural) =>
        count == 60 ? singular : plural;
}