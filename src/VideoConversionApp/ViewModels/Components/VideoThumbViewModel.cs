using System;
using System.Globalization;
using System.IO;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VideoConversionApp.Abstractions;
using VideoConversionApp.Models;
using VideoConversionApp.Utils;

namespace VideoConversionApp.ViewModels.Components;

public partial class VideoThumbViewModel : ViewModelBase
{

    // For visual representation; these do not affect anything except the visual presentation in the
    // UserControl.
    [ObservableProperty] public partial string FullFileName { get; set; } = "";
    [ObservableProperty] public partial Bitmap ThumbnailImage { get; set; } = null!;
    [ObservableProperty] public partial string PreviewFileName { get; set; } = "";
    [ObservableProperty] public partial long FileSize { get; set; } = 0;
    [ObservableProperty] public partial double VideoLengthSeconds { get; set; } = 0;
    [ObservableProperty] public partial DateTime VideoDateTime { get; set; } = DateTime.MinValue;
    [ObservableProperty] public partial bool HasLoadingThumbnail { get; set; } = true;
    [ObservableProperty] public partial bool ShowAsSelectedForConversion { get; set; } = false;
    [ObservableProperty] public partial bool HasProblems { get; set; } = false;
    [ObservableProperty] public partial string ToolTipMessage { get; set; } = null!;
    [ObservableProperty] public partial bool HasConversionSettingsModified { get; set; } = false;

    // TODO could just use converters that we have now
    public string VideoDateTimeString => VideoDateTime.ToString(DataFormattingHelpers.TryResolveActiveCulture());
    public string FileSizeString => FileSize.AsDataQuantityString(2);
    public string VideoLengthString => TimeSpan.FromSeconds(VideoLengthSeconds).ToString(@"hh\:mm\:ss");
    
    /// <summary>
    /// The linked data model.
    /// </summary>
    public IConvertableVideo? LinkedVideo { get; set; }

    private static readonly Bitmap DefaultThumbnail = new (AssetLoader.Open(new Uri("avares://VideoConversionApp/Images/videostrip.png")));
    
    public ICommand OnCloseClickCommand { get; set; }
    public IRelayCommand<bool> OnVideoCheckedChangedCommand { get; set; }
    
    public VideoThumbViewModel()
    {
        if (Design.IsDesignMode)
        {
            ThumbnailImage = DefaultThumbnail;
            //ThumbnailImage = new Bitmap(AssetLoader.Open(new Uri("avares://VideoConversionApp/Images/sample-thn.jpg")));
            FullFileName = "/tmp/GS010146.360";
            PreviewFileName = Path.GetFileName(FullFileName);
            FileSize = 180_556_782;
            VideoDateTime = DateTime.Now;
            VideoLengthSeconds = 127.5;
            return;
        }

        ThumbnailImage = DefaultThumbnail;
    }
    
    
    
}