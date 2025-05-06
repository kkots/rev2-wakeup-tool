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
        var shouldCheckHitstunStarting = (bool)values[5];
        var shouldCheckHitstunEnding = (bool)values[6];


        if (!shouldCheckWakingUp
                && !shouldCheckWallSplat
                && !shouldCheckAirTech
                && !shouldCheckStartBlocking
                && !shouldCheckBlockstunEnding
                && !shouldCheckHitstunStarting
                && !shouldCheckHitstunEnding)
        {
            return "Event is invalid!!!";
        }



        var events = new List<string>()
        {
            shouldCheckWakingUp ? "wakes up" : "",
            shouldCheckWallSplat ? "recovers from wall splat" : "",
            shouldCheckAirTech ? "recovers from air tech" : "",
            shouldCheckStartBlocking ? "is starting to block" : "",
            shouldCheckBlockstunEnding ? "stops blocking" : "",
            shouldCheckHitstunStarting ? "enters hitstun" : "",
            shouldCheckHitstunEnding ? "recovers from hitstun" : ""
            
        };

        var result = "Dummy ";


        result += string.Join(" or ", events.Where(evt => !string.IsNullOrEmpty(evt)));
        

        return result;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}