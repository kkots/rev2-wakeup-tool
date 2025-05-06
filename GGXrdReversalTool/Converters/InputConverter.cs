using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using GGXrdReversalTool.Library.Models.Inputs;

namespace GGXrdReversalTool.Converters;

[ValueConversion(typeof(string),typeof(IEnumerable<CondensedInput>))]
public class InputConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
        {
            return Enumerable.Empty<CondensedInput>();
        }
        
        var slotInput = new SlotInput(value.ToString() ?? "");

        return slotInput.CondensedInputList.Take(15);  // it starts to freeze up for a while when drawing too many inputs, even though they're off-screen.
        // I wonder if VirtualizingStackPanel can help with this, if we can somehow calculate the available horizontal space to determine how many inputs fit on one row.
        // Or we could just hardcode 4 inputs per row and add a horizontal scrollbar.
        // Maybe it's just the bindings being really slow, it is very hard to tell using diagnostic tools because they just point to the WPF dll.
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}