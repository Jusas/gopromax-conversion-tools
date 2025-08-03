using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using VideoConversionApp.Abstractions;

namespace VideoConversionApp.Config;

[ObservableObject]
public partial class PathsConfig : ConfigurationObject<PathsConfig>
{
    public override string GetConfigurationKey() => "paths";
    
    [ObservableProperty]
    public partial string Exiftool { get; set; }
    [ObservableProperty]
    public partial string Ffmpeg { get; set; }
    [ObservableProperty]
    public partial string Ffprobe { get; set; }

    public PathsConfig()
    {
        Exiftool = "exiftool";
        Ffmpeg = "ffmpeg";
        Ffprobe = "ffprobe";
    }

    protected override void InitializeFrom(PathsConfig? configuration)
    {
        if (configuration is null)
            return;
        
        Exiftool = configuration.Exiftool;
        Ffmpeg = configuration.Ffmpeg;
        Ffprobe = configuration.Ffprobe;
    }
}