using VideoConversionApp.Abstractions;

namespace VideoConversionApp.Models;

public class KeyFrameVideo
{
    public string VideoPath { get; set; }
    public IConvertableVideo SourceVideo { get; set; } 
}