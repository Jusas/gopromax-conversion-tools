using System;
using System.Globalization;

namespace VideoConversionApp.Utils;

public static class DataFormattingHelpers
{
    public static double AsMillisecondsDouble(this decimal d)
    {
        return (double)(d * 1000);
    }
    
    public static string AsDataQuantityString(this long bytes, int decimalPlaces = 2)
    {
        string[] sizeSuffixes = ["bytes", "KiB", "MiB", "GiB", "TiB"];

        if (bytes < 0)
            return "0";

        int i = 0;
        decimal d = bytes;
        while (Math.Round(d, decimalPlaces) >= 1000 && i < sizeSuffixes.Length - 1)
        {
            d /= 1024;
            i++;
        }

        if (bytes < 1024)
            decimalPlaces = 0;

        var format = "{0:n" + decimalPlaces + "} {1}";
        return string.Format(format, d, sizeSuffixes[i]);
    
    }
    
    public static CultureInfo TryResolveActiveCulture()
    {
        var lcTime = Environment.GetEnvironmentVariable("LC_TIME")
                     ?? Environment.GetEnvironmentVariable("LC_ALL");
        
        if (lcTime == null || lcTime == "C") 
            return CultureInfo.CurrentCulture;

        var culturePrefix = lcTime.Split(".")[0];
        try
        {
            return CultureInfo.GetCultureInfo(culturePrefix);
        }
        catch
        {
            return CultureInfo.InstalledUICulture;
        }


    }
}