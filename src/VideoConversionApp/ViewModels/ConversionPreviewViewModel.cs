using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VideoConversionApp.Abstractions;
using VideoConversionApp.Config;
using VideoConversionApp.Models;
using VideoConversionApp.Utils;
using VideoConversionApp.ViewModels.Components;

namespace VideoConversionApp.ViewModels;

public partial class ConversionPreviewViewModel : ViewModelBase
{
    private readonly IVideoPreviewService _videoPreviewService;
    private readonly IConfigManager _configManager;
    private readonly IVideoPoolManager _videoPoolManager;
    private readonly PreviewVideoPlayerState _previewVideoPlayerState;

    public IList<IImage> SnapshotFrameImages { get; set; } = [];

    [ObservableProperty] 
    public partial IConvertableVideo Video { get; private set; }
    [ObservableProperty]
    public partial IImage? CurrentSnapshotFrameImage { get; set; }
    [ObservableProperty]
    public partial double SnapshotRenderProgress { get; set; }
    [ObservableProperty]
    public partial double KeyFrameVideoRenderProgress { get; set; }
    [ObservableProperty]
    public partial bool BlurImageVisible { get; set; }
    [ObservableProperty]
    public partial decimal MaximumCropTimelineTime { get; private set; }
    [ObservableProperty]
    public partial decimal TimelineCropStartTime { get; set; }
    [ObservableProperty]
    public partial decimal TimelineCropEndTime { get; set; }
    
    [ObservableProperty]
    public partial VideoPlayerViewModel KeyFrameVideoPlayerViewModel { get; set; }
    
    // For editing the yaw/pitch/roll transforms
    [ObservableProperty]
    public partial bool AutoRenderOnChanges { get; set; }
    [ObservableProperty]
    public partial int TransformYawValue { get; set; }
    [ObservableProperty]
    public partial int TransformPitchValue { get; set; }
    [ObservableProperty]
    public partial int TransformRollValue { get; set; }
    
    // For KeyFrameVideo Preview
    [ObservableProperty]
    public partial int PreviewYawValue { get; set; }
    [ObservableProperty]
    public partial int PreviewPitchValue { get; set; }
    [ObservableProperty]
    public partial int PreviewRollValue { get; set; }
    [ObservableProperty]
    public partial int PreviewFovValue { get; set; }
    
    private CancellationTokenSource? _snapshotGenerationCts;
    private CancellationTokenSource? _snapshotLiveUpdateCts;
    
    public ConversionPreviewViewModel(
        IVideoPreviewService videoPreviewService,
        IConfigManager configManager,
        IVideoPoolManager videoPoolManager,
        PreviewVideoPlayerState previewVideoPlayerState)
    {
        _videoPreviewService = videoPreviewService;
        _configManager = configManager;
        _videoPoolManager = videoPoolManager;
        _previewVideoPlayerState = previewVideoPlayerState;
        //Video = _conversionManager.GetPlaceholderVideo();
        SetEventListeners();
        
        KeyFrameVideoPlayerViewModel = new VideoPlayerViewModel(_previewVideoPlayerState, 
            Video, null);
        
        _ = SetPreviewedVideo(Video);
    }

    private void SetEventListeners()
    {
        _previewVideoPlayerState.ViewPointFovChanged += PreviewVideoPlayerStateOnViewPointFovChanged;
        _previewVideoPlayerState.ViewPointPitchChanged += PreviewVideoPlayerStateOnViewPointPitchChanged;
        _previewVideoPlayerState.ViewPointRollChanged += PreviewVideoPlayerStateOnViewPointRollChanged;
        _previewVideoPlayerState.ViewPointYawChanged += PreviewVideoPlayerStateOnViewPointYawChanged;
        _previewVideoPlayerState.TimelineCropStartPositionChanged += PreviewVideoPlayerStateOnTimelineCropStartPositionChanged;
        _previewVideoPlayerState.TimelineCropEndPositionChanged += PreviewVideoPlayerStateOnTimelineCropEndPositionChanged;
    }

    private void PreviewVideoPlayerStateOnTimelineCropEndPositionChanged(object? sender, PreviewVideoPlayerState.StateEventArgs<decimal> e)
    {
        if (e.Context == this)
            return;
       
        TimelineCropEndTime = e.Value;
    }

    partial void OnTimelineCropEndTimeChanged(decimal value)
    {
        if (value < TimelineCropStartTime)
            value = TimelineCropStartTime;
        
        Video.TimelineCrop = Video.TimelineCrop with { EndTimeSeconds = value };
        _previewVideoPlayerState.SetTimelineCropEndPosition(value, this);
    }

    private void PreviewVideoPlayerStateOnTimelineCropStartPositionChanged(object? sender, PreviewVideoPlayerState.StateEventArgs<decimal> e)
    {
        if (e.Context == this) 
            return;
        
        TimelineCropStartTime = e.Value;
    }

    partial void OnTimelineCropStartTimeChanged(decimal value)
    {
        if (value > TimelineCropEndTime)
            value = TimelineCropEndTime;
        
        Video.TimelineCrop = Video.TimelineCrop with { StartTimeSeconds = value };
        _previewVideoPlayerState.SetTimelineCropStartPosition(value, this);
    }
    
    private void PreviewVideoPlayerStateOnViewPointYawChanged(object? sender, PreviewVideoPlayerState.StateEventArgs<float> e)
    {
        if (e.Context != this)
            PreviewYawValue = (int)e.Value;
    }

    partial void OnPreviewYawValueChanged(int value)
    {
        _previewVideoPlayerState.SetViewPointYaw(value, this);
    }

    private void PreviewVideoPlayerStateOnViewPointRollChanged(object? sender, PreviewVideoPlayerState.StateEventArgs<float> e)
    {
        if (e.Context != this)
            PreviewRollValue = (int)e.Value;
    }

    partial void OnPreviewRollValueChanged(int value)
    {
        _previewVideoPlayerState.SetViewPointRoll(value, this);
    }

    private void PreviewVideoPlayerStateOnViewPointPitchChanged(object? sender, PreviewVideoPlayerState.StateEventArgs<float> e)
    {
        if (e.Context != this)
            PreviewPitchValue = (int)e.Value;
    }

    partial void OnPreviewPitchValueChanged(int value)
    {
        _previewVideoPlayerState.SetViewPointPitch(value, this);
    }

    private void PreviewVideoPlayerStateOnViewPointFovChanged(object? sender, PreviewVideoPlayerState.StateEventArgs<float> e)
    {
        if (e.Context != this)
            PreviewFovValue = (int)e.Value;
    }
    
    partial void OnPreviewFovValueChanged(int value)
    {
        _previewVideoPlayerState.SetViewPointFov(value, this);
    }

    partial void OnAutoRenderOnChangesChanged(bool value)
    {
        if (value)
            _ = LiveUpdateSnapshot();
    }

    partial void OnTransformYawValueChanged(int value)
    {
        Video.FrameRotation = Video.FrameRotation with { Yaw = value };
        if (AutoRenderOnChanges)
            _ = LiveUpdateSnapshot();
    }

    partial void OnTransformPitchValueChanged(int value)
    {
        Video.FrameRotation = Video.FrameRotation with { Pitch = value };
        if (AutoRenderOnChanges)
            _ = LiveUpdateSnapshot();
    }

    partial void OnTransformRollValueChanged(int value)
    {
        Video.FrameRotation = Video.FrameRotation with { Roll = value };
        if (AutoRenderOnChanges)
            _ = LiveUpdateSnapshot();
    }

    public async Task SetPreviewedVideo(IConvertableVideo? video)
    {
        if (video == null)
            video = _videoPoolManager.GetPlaceholderVideo();
        
        IImage? thumb = null;
        var thumbBytes = _videoPreviewService.GetCachedThumbnail(video.InputVideoInfo);
        if (thumbBytes != null)
            thumb = thumbBytes.ToBitmap();

        await SetActiveVideoModelAsync(video, thumb);
    }

    /// <summary>
    /// Sets the active video in the main preview pane.
    /// Triggers the generation of snapshot frames.
    /// </summary>
    /// <param name="video"></param>
    /// <param name="initialPreviewImage">Image to show initially in the snapshot image control before
    /// the snapshot frames get generated.</param>
    public async Task SetActiveVideoModelAsync(IConvertableVideo? video, IImage? initialPreviewImage)
    {
        if (video == Video)
            return;
        
        BlurImageVisible = true;
        Video = video;

        if (video == null && initialPreviewImage == null)
            return;

        if (initialPreviewImage != null)
        {
            CurrentSnapshotFrameImage = initialPreviewImage;
            SnapshotFrameImages = [initialPreviewImage];
        }
        
        KeyFrameVideoPlayerViewModel = new VideoPlayerViewModel(_previewVideoPlayerState, video, null);

        if (video == null || video == _videoPoolManager.GetPlaceholderVideo())
            return;

        
        var prevAutoRenderSetting = AutoRenderOnChanges;
        AutoRenderOnChanges = false;
        TransformPitchValue = video.FrameRotation.Pitch;
        TransformYawValue = video.FrameRotation.Yaw;
        TransformRollValue = video.FrameRotation.Roll;
        AutoRenderOnChanges = prevAutoRenderSetting;

        MaximumCropTimelineTime = video.InputVideoInfo.DurationInSeconds;
        TimelineCropStartTime = video.TimelineCrop.StartTimeSeconds ?? 0;
        TimelineCropEndTime = video.TimelineCrop.EndTimeSeconds ?? video.InputVideoInfo.DurationInSeconds;

        ResetPreviewFov();
        ResetPreviewPitch();
        ResetPreviewRoll();
        ResetPreviewYaw();
        
        await RenderSnapshotFramesAsync();
        NextFrame();
    }

    [RelayCommand]
    public void NextFrame()
    {
        if (SnapshotFrameImages.Count == 0)
            return;
        
        var currentIndex = SnapshotFrameImages.IndexOf(CurrentSnapshotFrameImage);
        var nextIndex = currentIndex + 1;
        if (nextIndex >= SnapshotFrameImages.Count)
            nextIndex = 0;
        CurrentSnapshotFrameImage = SnapshotFrameImages[nextIndex];
    }

    [RelayCommand]
    public void PreviousFrame()
    {
        if (SnapshotFrameImages.Count == 0)
            return;
        
        var currentIndex = SnapshotFrameImages.IndexOf(CurrentSnapshotFrameImage);
        var prevIndex = currentIndex - 1;
        if (prevIndex < 0)
            prevIndex = SnapshotFrameImages.Count - 1;
        CurrentSnapshotFrameImage = SnapshotFrameImages[prevIndex];
    }

    /// <summary>
    /// Renders/generates the snapshot frames from the currently active video.
    /// Removes the blur image upon success.
    /// </summary>
    public async Task RenderSnapshotFramesAsync()
    {
        // If there's already this operation in progress, cancel the previous one.
        if (_snapshotGenerationCts != null && !_snapshotGenerationCts.Token.IsCancellationRequested)
            _snapshotGenerationCts.Cancel();

        var myCts = new CancellationTokenSource();
        _snapshotGenerationCts = myCts;

        var previewConfig = _configManager.GetConfig<PreviewsConfig>()!;
        var frameCount = (int)previewConfig.NumberOfSnapshotFrames;
        
        
        try
        {
            var transformationSettings = new SnapshotFrameTransformationSettings()
            {
                Rotation = Video.FrameRotation
            };
            var bitmapBytes = await _videoPreviewService.GenerateSnapshotFramesAsync(Video.InputVideoInfo,
                transformationSettings, frameCount, (progress) => SnapshotRenderProgress = progress, myCts.Token);

            var bitmaps = new IImage[bitmapBytes.Count];
            for (int i = 0; i < bitmapBytes.Count; i++)
                bitmaps[i] = bitmapBytes[i].ToBitmap();

            SnapshotFrameImages = bitmaps.ToList();
            BlurImageVisible = false;
        }
        catch (TaskCanceledException)
        {
            SnapshotRenderProgress = 0;
        }
        catch (Exception e)
        {
        }

    }

    
    
    [RelayCommand]
    public async Task OnRenderSnapshotFramesAsync()
    {
        var frameIndex = SnapshotFrameImages.IndexOf(CurrentSnapshotFrameImage);
        await RenderSnapshotFramesAsync();
        if (frameIndex != -1 && frameIndex <= SnapshotFrameImages.Count)
            CurrentSnapshotFrameImage = SnapshotFrameImages[frameIndex];
        else
            NextFrame();
    }

    [RelayCommand]
    public void ResetTransformYaw()
    {
        TransformYawValue = 0;
    }
    
    [RelayCommand]
    public void ResetTransformPitch()
    {
        TransformPitchValue = 0;
    }
    
    [RelayCommand]
    public void ResetTransformRoll()
    {
        TransformRollValue = 0;
    }
    
    [RelayCommand]
    public void ResetPreviewYaw()
    {
        PreviewYawValue = 0;
    }
    
    [RelayCommand]
    public void ResetPreviewPitch()
    {
        PreviewPitchValue = 0;
    }
    
    [RelayCommand]
    public void ResetPreviewRoll()
    {
        PreviewRollValue = 0;
    }
    
    [RelayCommand]
    public void ResetPreviewFov()
    {
        PreviewFovValue = 80;
    }

    [RelayCommand]
    public void ResetVideoTimelineCrop()
    {
        if (Video == null)
            return;

        Video.TimelineCrop = new TimelineCrop();
        TimelineCropStartTime = 0;
        TimelineCropEndTime = Video.InputVideoInfo.DurationInSeconds;
       
    }

    private async Task LiveUpdateSnapshot()
    {
        if (_snapshotLiveUpdateCts != null && !_snapshotLiveUpdateCts.Token.IsCancellationRequested)
            _snapshotLiveUpdateCts.Cancel();

        _snapshotLiveUpdateCts = new CancellationTokenSource();
        try
        {
            await Task.Delay(250, _snapshotLiveUpdateCts.Token);
        }
        catch (TaskCanceledException)
        {
            return;
        }

        await OnRenderSnapshotFramesAsync();
    }

    [RelayCommand]
    public async Task RenderKeyFrameVideoAsync()
    {
        try
        {
            var keyFrameVideo = await _videoPreviewService.GenerateKeyFrameVideoAsync(Video,
                (progress) => KeyFrameVideoRenderProgress = progress, CancellationToken.None);
            
            KeyFrameVideoPlayerViewModel = new VideoPlayerViewModel(_previewVideoPlayerState, Video, keyFrameVideo);
        }
        catch (Exception e)
        {
        }
        
    }
    
}