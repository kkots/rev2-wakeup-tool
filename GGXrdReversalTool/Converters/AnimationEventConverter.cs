using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace GGXrdReversalTool.Converters;

public class AnimationEventConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var shouldCheckWakingUp = (bool)values[0];
        var shouldCheckWallSplat = (bool)values[1];
        var shouldCheckAirTech = (bool)values[2];
        var shouldCheckStartBlocking = (bool)values[3];
        var shouldCheckBlockstunEnding = (bool)values[4];


        if (!shouldCheckWakingUp && !shouldCheckWallSplat && !shouldCheckAirTech && !shouldCheckStartBlocking && !shouldCheckBlockstunEnding)
        {
            return "Event is invalid!!!";
        }



        var events = new List<string>()
        {
            shouldCheckWakingUp ? "wakes up" : "",
            shouldCheckWallSplat ? "recovers from wall splat" : "",
            shouldCheckAirTech ? "recovers from air tech" : "",
            shouldCheckStartBlocking ? "is starting to block" : "",
            shouldCheckBlockstunEnding ? "stops blocking" : ""
            
        };

        var result = "Dummy ";


        result += events.Where(evt => !string.IsNullOrEmpty(evt)).Aggregate((a, b) => $"{a} or {b}");
        

        return result;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}