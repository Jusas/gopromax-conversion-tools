using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using VideoConversionApp.Abstractions;

namespace VideoConversionApp.Config;

[ObservableObject]
public partial class ConversionConfig : ConfigurationObject<ConversionConfig>
{
    public override string GetConfigurationKey() => "conversion";

    [ObservableProperty]
    public partial string CodecAudio { get; set; }
    [ObservableProperty]
    public partial string CodecVideo { get; set; }
    [ObservableProperty]
    public partial bool OutputAudio { get; set; }
    [ObservableProperty]
    public partial bool OutputBesideOriginals { get; set; }
    [ObservableProperty]
    public partial string OutputDirectory { get; set; }
    [ObservableProperty]
    public partial string OutputFilenamePattern { get; set; }
    [ObservableProperty]
    public partial bool AllowOverwrite { get; set; }
    [ObservableProperty]
    public partial bool UseCustomEncodingSettings { get; set; }
    [ObservableProperty]
    public partial string CustomContainerName { get; set; }
    [ObservableProperty]
    public partial uint CustomResolutionWidth { get; set; }
    [ObservableProperty]
    public partial uint CustomResolutionHeight { get; set; }

    public ConversionConfig()
    {
        CodecAudio = "pcm_s16le";
        CodecVideo = "prores";
        OutputAudio = true;
        OutputBesideOriginals = true;
        OutputDirectory = "";
        OutputFilenamePattern = "%o-%c";
        UseCustomEncodingSettings = false;
        CustomResolutionWidth = 1920;
        CustomResolutionHeight = 1080;
        CustomContainerName = "mov";
        AllowOverwrite = true;
    }
    
    protected override void InitializeFrom(ConversionConfig? configuration)
    {
        if (configuration is null)
            return;
        
        CodecAudio = configuration.CodecAudio;
        CodecVideo = configuration.CodecVideo;
        OutputAudio = configuration.OutputAudio;
        OutputBesideOriginals = configuration.OutputBesideOriginals;
        OutputDirectory = configuration.OutputDirectory;
        OutputFilenamePattern = configuration.OutputFilenamePattern;
        UseCustomEncodingSettings = configuration.UseCustomEncodingSettings;
        CustomResolutionWidth = configuration.CustomResolutionWidth;
        CustomResolutionHeight = configuration.CustomResolutionHeight;
        CustomContainerName = configuration.CustomContainerName;
        AllowOverwrite = configuration.AllowOverwrite;
    }
}