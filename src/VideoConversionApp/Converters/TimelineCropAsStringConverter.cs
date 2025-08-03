using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using VideoConversionApp.Abstractions;
using VideoConversionApp.Models;

namespace VideoConversionApp.Converters;

[Obsolete("to be removed")]
public class TimelineCropAsStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
            return "";

        if (value is IConvertableVideo video)
        {
            if (parameter is bool durationOnly || parameter is string durationOnlyString && durationOnlyString == "true")
                return StringifyDuration(video.InputVideoInfo, video.TimelineCrop);
            else
                return StringifyStartEnd(video.InputVideoInfo, video.TimelineCrop);
        }

        return "";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return BindingOperations.DoNothing;
    }

    public string StringifyDuration(IInputVideoInfo inputVideoInfo, TimelineCrop crop)
    {
        var startTime = crop.StartTimeSeconds ?? 0;
        var endTime = crop.EndTimeSeconds ?? inputVideoInfo.DurationInSeconds;
        var span = TimeSpan.FromSeconds((double)(endTime - startTime));
        return span.ToString("hh\\:mm\\:ss");
    }
    
    public string StringifyStartEnd(IInputVideoInfo inputVideoInfo, TimelineCrop crop)
    {
        var cropElems = new List<string>();
        if (crop.StartTimeSeconds != null && crop.StartTimeSeconds > 0)
        {
            var startTime = crop.StartTimeSeconds;
            var endTime = crop.EndTimeSeconds ?? inputVideoInfo.DurationInSeconds;
            cropElems.Add(TimeSpan.FromSeconds((double)startTime).ToString("hh\\:mm\\:ss"));
            cropElems.Add(TimeSpan.FromSeconds((double)endTime).ToString("hh\\:mm\\:ss"));
        }
        else if (crop.EndTimeSeconds != null && crop.EndTimeSeconds > 0)
        {
            var startTime = crop.StartTimeSeconds ?? 0;
            var endTime = crop.EndTimeSeconds;
            cropElems.Add(TimeSpan.FromSeconds((double)startTime).ToString("hh\\:mm\\:ss"));
            cropElems.Add(TimeSpan.FromSeconds((double)endTime).ToString("hh\\:mm\\:ss"));
        }

        return string.Join(" - ", cropElems);

    }
}