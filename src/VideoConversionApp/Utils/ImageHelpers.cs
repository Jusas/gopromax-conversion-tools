using System.IO;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace VideoConversionApp.Utils;

public static class ImageHelpers
{
    public static IImage ToBitmap(this byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        return new Bitmap(stream);
    }
}