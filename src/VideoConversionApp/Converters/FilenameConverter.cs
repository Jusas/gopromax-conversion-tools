using System;
using System.Globalization;
using System.IO;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace VideoConversionApp.Converters;

public class FilenameConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
            return "";

        if (value is string s)
        {
            if (parameter is string p)
            {
                if (p == "GetFileName")
                    return Path.GetFileName(s);    
                if (p == "GetFilenameWithoutExtension")
                    return Path.GetFileNameWithoutExtension(s);
                if (p == "GetExtension")
                    return Path.GetExtension(s);
                if (p == "GetDirectoryName")
                    return Path.GetDirectoryName(s);
                if (p == "ShortenedPath")
                {
                    var dir = Path.GetDirectoryName(s) ?? "";
                    if (dir.Length > 30)
                        dir = dir.Substring(0, 30) + "...";
                    return Path.Combine(dir, Path.GetFileName(s));
                }
            }
        }

        return "";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return BindingOperations.DoNothing;
    }
}