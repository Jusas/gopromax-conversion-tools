using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using VideoConversionApp.Abstractions;
using VideoConversionApp.Models;

namespace VideoConversionApp.Converters;

public class TimelineCropAsStringMultiConverter : IMultiValueConverter
{
    
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count == 2)
        {
            var crop = values[0] as TimelineCrop? ?? default;
            var duration = values[1] as decimal? ?? 0;
            if (duration == 0)
                return "";

            if (parameter is bool durationOnly && durationOnly == true)
                return StringifyDuration(crop, duration);
            
            return StringifyStartEnd(crop, duration);


        }

        return "";
    }

    private string StringifyDuration(TimelineCrop crop, decimal duration)
    {
        var startTime = crop.StartTimeSeconds ?? 0;
        var endTime = crop.EndTimeSeconds ?? duration;
        var span = TimeSpan.FromSeconds((double)(endTime - startTime));
        return span.ToString("hh\\:mm\\:ss");
    }

    private string StringifyStartEnd(TimelineCrop crop, decimal duration)
    {
        var cropElems = new List<string>();
        if (crop.StartTimeSeconds != null && crop.StartTimeSeconds > 0)
        {
            var startTime = crop.StartTimeSeconds;
            var endTime = crop.EndTimeSeconds ?? duration;
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