using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using VideoConversionApp.Utils;

namespace VideoConversionApp.Converters;

public class DateTimeAsLocalFormatConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
            return "";

        if (value is DateTime d)
        {
            return d.ToString(DataFormattingHelpers.TryResolveActiveCulture());
        }

        return "";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return BindingOperations.DoNothing;
    }
}