using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using GGXrdReversalTool.Library.Scenarios.Event;

namespace GGXrdReversalTool.Converters;

public class EnumToStringConverter<T> : IValueConverter
where T : struct, Enum
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
        {
            return string.Empty;
        }
        
        if (value.GetType().IsArray)
        {
        	return (value as IEnumerable<T>)?.Select(
        		enumValue => MemberToString(Enum.GetName(enumValue) ?? string.Empty)
    		) ?? Array.Empty<string>();
        }
        
        return MemberToString(Enum.GetName((T)value) ?? string.Empty);
     }
    private string MemberToString(string enumName)
    {
    	System.Reflection.FieldInfo? fieldInfo = typeof(T).GetField(enumName);
    	
    	if (fieldInfo is null)
    		return string.Empty;
    	
    	object[] attributes = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
    	
		return attributes.Length > 0 && attributes[0] as DescriptionAttribute != null
			? ((DescriptionAttribute)attributes[0]).Description
        	: fieldInfo.Name;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
    	if (value as string == null)
    		throw new ArgumentException(nameof(value));
    	
        var filteredMembers = Enum.GetNames<T>()
        	.Where(enumName => MemberToString(enumName) == (string)value);
        
        if (filteredMembers.Any())
        {
        	return Enum.Parse<T>(filteredMembers.First());
        }
        throw new ArgumentException(nameof(value));
    }
}