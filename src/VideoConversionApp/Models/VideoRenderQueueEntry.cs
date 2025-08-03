using System.Threading;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using VideoConversionApp.Abstractions;

namespace VideoConversionApp.Models;

public partial class VideoRenderQueueEntry : ObservableObject
{
    [ObservableProperty]
    public partial IConvertableVideo Video { get; set; }
    [ObservableProperty]
    public partial double Progress { get; set; } = 0;
    [ObservableProperty]
    public partial VideoRenderingState RenderingState { get; set; } = VideoRenderingState.Queued;
    [ObservableProperty]
    public partial Bitmap? Thumbnail { get; set; }
    [ObservableProperty]
    public partial string[] Errors { get; set; }
    
    public CancellationTokenSource CancellationTokenSource { get; private set; } = new();
    
    public VideoRenderQueueEntry(IConvertableVideo video)
    {
        Video = video;
        Errors = new string[0];
        if (Design.IsDesignMode)
        {
            Errors = new[]
            {
                "Error test",
                "Error test 2"
                //"Warning CS8604 : Possible null reference argument for parameter 'item' in 'int IListIImage.IndexOf(IImage item)' Warning CS8604 : Possible null reference argument for parameter 'item' in 'int IListIImage.IndexOf(IImage item)' Warning CS8604 : Possible null reference argument for parameter 'item' in 'int IListIImage.IndexOf(IImage item)'"
            };
        }
    }

    public void ResetStatus()
    {
        Progress = 0;
        RenderingState = VideoRenderingState.Queued;
        Errors = new string[0];
        CancellationTokenSource = new();
    }
}