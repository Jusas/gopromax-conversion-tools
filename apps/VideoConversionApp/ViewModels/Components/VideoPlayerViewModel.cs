using System;
using Avalonia.Controls;
using LibVLCSharp.Shared;

namespace VideoConversionApp.ViewModels.Components;

public class VideoPlayerViewModel : ViewModelBase
{
    private readonly LibVLC _libVlc = new LibVLC();
        
    public MediaPlayer MediaPlayer { get; }

    public VideoPlayerViewModel()
    {
        MediaPlayer = new MediaPlayer(_libVlc);
    }
    
    public void Play()
    {
        if (Design.IsDesignMode)
        {
            return;
        }
            
        using var media = new Media(_libVlc, new Uri("http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4"));
        //media.Duration
        
        MediaPlayer.Play(media);
    }
    
    public void Stop()
    {            
        MediaPlayer.Stop();
    }


    public void Dispose()
    {
        MediaPlayer?.Dispose();
        _libVlc?.Dispose();
        
    }
}