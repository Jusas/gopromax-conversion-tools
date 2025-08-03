using System;
using CommunityToolkit.Mvvm.ComponentModel;
using VideoConversionApp.Abstractions;
using VideoConversionApp.Models;

namespace VideoConversionApp.ViewModels.Components;

/// <summary>
/// Model for the VideoPlayerView.
/// The VideoView does not play very nice with bindings; they break probably because of
/// how the control kind of detaches to its own window and values do not seem to update correctly
/// to the controls inside the view.
/// This is why there is so much logic in the view's code-behind.
/// </summary>
public partial class VideoPlayerViewModel : ViewModelBase
{
    public PreviewVideoPlayerState PlayerState { get; }

    [ObservableProperty]
    public partial Uri? VideoUri { get; set; } = null;

    public IConvertableVideo? SourceVideo { get; set; } = null;
    public KeyFrameVideo KeyFrameVideo { get; set; } = null!;
    
    public VideoPlayerViewModel(PreviewVideoPlayerState playerState, IConvertableVideo? sourceVideo, KeyFrameVideo? keyFrameVideo)
    {
        PlayerState = playerState;
        SourceVideo = sourceVideo;
        if (keyFrameVideo != null)
        {
            KeyFrameVideo = keyFrameVideo;
            VideoUri = new Uri(keyFrameVideo.VideoPath);
        }
    }

}