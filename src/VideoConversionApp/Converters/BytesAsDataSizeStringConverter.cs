using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using VideoConversionApp.Utils;

namespace VideoConversionApp.Converters;

public class BytesAsDataSizeStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
            return "";

        if (value is long l)
        {
            return l.AsDataQuantityString();
        }

        return "";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return BindingOperations.DoNothing;
    }
}