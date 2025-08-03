using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using VideoConversionApp.Abstractions;

namespace VideoConversionApp.Config;

[ObservableObject]
public partial class PreviewsConfig : ConfigurationObject<PreviewsConfig>
{
    public override string GetConfigurationKey() => "previews";
    
    [ObservableProperty]
    public partial uint NumberOfSnapshotFrames { get; set; }
    [ObservableProperty]
    public partial uint NumberOfThumbnailThreads { get; set; }
    [ObservableProperty]
    public partial uint ThumbnailTimePositionPcnt { get; set; }

    public PreviewsConfig()
    {
        NumberOfSnapshotFrames = 10;
        NumberOfThumbnailThreads = 3;
        ThumbnailTimePositionPcnt = 50;
    }
    
    protected override void InitializeFrom(PreviewsConfig? configuration)
    {
        if (configuration is null)
            return;
        
        NumberOfSnapshotFrames = configuration.NumberOfSnapshotFrames;
        NumberOfThumbnailThreads = configuration.NumberOfThumbnailThreads;
        ThumbnailTimePositionPcnt = configuration.ThumbnailTimePositionPcnt;
    }
}