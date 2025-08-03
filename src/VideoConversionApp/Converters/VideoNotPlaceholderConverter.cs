using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using VideoConversionApp.Abstractions;

namespace VideoConversionApp.Converters;

/// <summary>
/// "Video is not placeholder" value converter.
/// </summary>
public class VideoNotPlaceholderConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
            return false;

        if (value is IConvertableVideo video)
        {
            return !IInputVideoInfo.IsPlaceholderFile(video.InputVideoInfo.Filename);
        }

        return true;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return BindingOperations.DoNothing;
    }
}