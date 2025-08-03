using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace VideoConversionApp.Converters;

public class DecimalToTimeStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
            return "";

        if (value is decimal d)
        {
            return TimeSpan.FromSeconds((double)d).ToString("hh\\:mm\\:ss");
        }

        return "";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return BindingOperations.DoNothing;
    }
}