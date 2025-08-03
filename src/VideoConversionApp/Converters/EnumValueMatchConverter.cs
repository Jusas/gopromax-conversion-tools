using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace VideoConversionApp.Converters;

public class EnumValueMatchConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
            return false;

        if (value.GetType() == parameter.GetType() && Enum.Equals(value, parameter))
            return true;
        
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return BindingOperations.DoNothing;
    }
}

public class EnumValueNotMatchConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
            return false;

        if (value.GetType() == parameter.GetType() && !Enum.Equals(value, parameter))
            return true;
        
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return BindingOperations.DoNothing;
    }
}

public class EnumValueMatchAnyConverter : IMultiValueConverter
{

    // First value is the value we compare others to
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 2)
            return false;

        var first = values[0];
        foreach (var value in values.Skip(1))
        {
            if (value.GetType() == first.GetType() && Enum.Equals(value, first))
                return true;
        }
        
        return false;
    }
}