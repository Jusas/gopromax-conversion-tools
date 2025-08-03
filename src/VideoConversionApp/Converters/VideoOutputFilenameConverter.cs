using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using VideoConversionApp.Abstractions;

namespace VideoConversionApp.Converters;

public class VideoOutputFilenameConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count == 3)
        {
            var video = values[0] as IConvertableVideo;
            var mediaConverterService = values[1] as IVideoConverterService;
            var pattern = values[2] as string;

            if (video == null || mediaConverterService == null || pattern == null) 
                return "";

            return mediaConverterService.GetFilenameFromPattern(video, pattern);
        }

        return "";
    }
}