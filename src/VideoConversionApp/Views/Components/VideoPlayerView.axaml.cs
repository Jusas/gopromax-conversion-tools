using System;
using System.ComponentModel;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using LibVLCSharp.Shared;
using VideoConversionApp.Abstractions;
using VideoConversionApp.Models;
using VideoConversionApp.ViewModels.Components;

namespace VideoConversionApp.Views.Components;

/// <summary>
/// The video player, with play/pause controls and crop markers.
/// Has a lot of going on, because of ViewModel binding issues and because there are
/// also controls outside the player view that can control it - so the player is
/// also controlled using the PreviewVideoPlayerState object's events.
/// </summary>
public partial class VideoPlayerView : UserControl, IDisposable
{

    private bool _isPanning = false;
    private bool _isRolling = false;
    private bool _isFoving = false;
    
    private Point _previousMousePosition;
    private float _fov = 80;
    private readonly float _defaultFov = 80;
    
    private readonly LibVLC _libVlc = new ();
    private readonly MediaPlayer _mediaPlayer;

    private VideoPlayerViewModel? ViewModel => DataContext as VideoPlayerViewModel;
    
    public VideoPlayerView()
    {
        InitializeComponent();
        
        // Workaround: when the Tab is changed, the player is detached from the visual tree and does not run Attach()
        // internal method which it should, in order to anchor it to the window. If we don't do this, it will open
        // up as a popup after tab change.
        Player.AttachedToVisualTree += PlayerOnAttachedToVisualTree;
        
        _mediaPlayer = new MediaPlayer(_libVlc) { EnableHardwareDecoding = true };
        Player.MediaPlayer = _mediaPlayer;
        _mediaPlayer.TimeChanged += MediaPlayerOnTimeChanged;
        
        Player.SizeChanged += PlayerOnSizeChanged;
    }

    // Move crop markers to according to player width.
    private void PlayerOnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (ViewModel == null)
            return;

        var playerState = ViewModel.PlayerState;
        CalculateAndSetCropMarkerPositions(playerState.TimelineCropStartPosition, playerState.TimelineCropEndPosition);
    }

    // Keep the scrubber synced to video player.
    private void MediaPlayerOnTimeChanged(object? sender, MediaPlayerTimeChangedEventArgs e)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            Scrubber.Value = e.Time / 1000.0;
        });
    }

    // Workaround for the player.
    private void PlayerOnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        var attachMethod = Player.GetType().GetMethod("Attach", BindingFlags.Instance | BindingFlags.NonPublic);
        attachMethod!.Invoke(Player, []);
    }
    
    private void OnDataContextChanged(object sender, EventArgs e)
    {
        if (DataContext is VideoPlayerViewModel vm)
        {
            if(_mediaPlayer.Media != null)
                _mediaPlayer.Media.Dispose();

            SetEventListeners();
            var enableControls = vm.SourceVideo != null && !IInputVideoInfo.IsPlaceholderFile(vm.SourceVideo.InputVideoInfo.Filename);
            vm.PlayerState.SetControlsEnabled(enableControls, this);

            if (vm.VideoUri != null)
            {
                // Duration is not available until we play, but we have that information
                // in the KeyFrameVideo class.
                Scrubber.Maximum = (double)vm.KeyFrameVideo.SourceVideo.InputVideoInfo.DurationInSeconds;
                
                var media = new Media(_libVlc, vm.VideoUri);
                _mediaPlayer.Media = media;
                _mediaPlayer.Play();
                _mediaPlayer.SetPause(true);
            }
            else
            {
                _mediaPlayer.Media = null;
                _mediaPlayer.Stop();
            }
        }
    }

    // Update the video player viewport and markers when they're updated from outside.
    private void SetEventListeners()
    {
        if (ViewModel == null)
            return;
        
        var playerState = ViewModel.PlayerState;
        
        // As context changes, ensure we're adding ourselves as listener but not adding multiple times.
        playerState.ViewPointFovChanged -= PlayerStateOnViewPointFovChanged;
        playerState.ViewPointPitchChanged -= PlayerStateOnViewPointPitchChanged;
        playerState.ViewPointRollChanged -= PlayerStateOnViewPointRollChanged;
        playerState.ViewPointYawChanged -= PlayerStateOnViewPointYawChanged;
        playerState.TimelineCropEndPositionChanged -= PlayerStateOnTimelineCropEndPositionChanged;
        playerState.TimelineCropStartPositionChanged -= PlayerStateOnTimelineCropStartPositionChanged;
        playerState.ControlsEnabledChanged -= PlayerStateOnControlsEnabledChanged;
        
        playerState.ViewPointFovChanged += PlayerStateOnViewPointFovChanged;
        playerState.ViewPointPitchChanged += PlayerStateOnViewPointPitchChanged;
        playerState.ViewPointRollChanged += PlayerStateOnViewPointRollChanged;
        playerState.ViewPointYawChanged += PlayerStateOnViewPointYawChanged;
        playerState.TimelineCropEndPositionChanged += PlayerStateOnTimelineCropEndPositionChanged;
        playerState.TimelineCropStartPositionChanged += PlayerStateOnTimelineCropStartPositionChanged;
        playerState.ControlsEnabledChanged += PlayerStateOnControlsEnabledChanged;
    }

    private void PlayerStateOnControlsEnabledChanged(object? sender, PreviewVideoPlayerState.StateEventArgs<bool> e)
    {
        ControlsPanel.IsEnabled = e.Value;
    }

    private void PlayerStateOnTimelineCropStartPositionChanged(object? sender, PreviewVideoPlayerState.StateEventArgs<decimal> e)
    {
        if (ReferenceEquals(e.Context, this))
            return;

        var playerState = sender as PreviewVideoPlayerState;
        CalculateAndSetCropMarkerPositions(e.Value, playerState!.TimelineCropEndPosition);
    }

    private void PlayerStateOnTimelineCropEndPositionChanged(object? sender, PreviewVideoPlayerState.StateEventArgs<decimal> e)
    {
        if (ReferenceEquals(e.Context, this))
            return;
        
        var playerState = sender as PreviewVideoPlayerState;
        CalculateAndSetCropMarkerPositions(playerState!.TimelineCropStartPosition, e.Value);
    }

    private void PlayerStateOnViewPointYawChanged(object? sender, PreviewVideoPlayerState.StateEventArgs<float> e)
    {
        if (ReferenceEquals(e.Context, this))
            return;
        
        var vp = Player.MediaPlayer!.Viewpoint;
        Player.MediaPlayer.UpdateViewpoint(e.Value, vp.Pitch, vp.Roll, _fov);
    }

    private void PlayerStateOnViewPointPitchChanged(object? sender, PreviewVideoPlayerState.StateEventArgs<float> e)
    {
        if (ReferenceEquals(e.Context, this))
            return;
        
        var vp = Player.MediaPlayer!.Viewpoint;
        Player.MediaPlayer.UpdateViewpoint(vp.Yaw, e.Value, vp.Roll, _fov);
    }
    
    private void PlayerStateOnViewPointRollChanged(object? sender, PreviewVideoPlayerState.StateEventArgs<float> e)
    {
        if (ReferenceEquals(e.Context, this))
            return;
        
        var vp = Player.MediaPlayer!.Viewpoint;
        Player.MediaPlayer.UpdateViewpoint(vp.Yaw, vp.Pitch, e.Value, _fov);
    }

    private void PlayerStateOnViewPointFovChanged(object? sender, PreviewVideoPlayerState.StateEventArgs<float> e)
    {
        if (ReferenceEquals(e.Context, this))
            return;
        
        var vp = Player.MediaPlayer!.Viewpoint;
        Player.MediaPlayer.UpdateViewpoint(vp.Yaw, vp.Pitch, vp.Roll, e.Value);
    }

    private void VideoViewOnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var isRightClick = e.GetCurrentPoint(this).Properties.IsRightButtonPressed;
        var isLeftClick = e.GetCurrentPoint(this).Properties.IsLeftButtonPressed;
        var isMiddleClick = e.GetCurrentPoint(this).Properties.IsMiddleButtonPressed;
        
        if (isLeftClick)
        {
            _isPanning = true;
        }

        if (isRightClick)
        {
            _isRolling = true;
        }

        if (isMiddleClick)
        {
            _isFoving = true;
        }

        _previousMousePosition = e.GetCurrentPoint(this).Position;
    }

    private void VideoViewOnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _isPanning = false;
        _isRolling = false;
        _isFoving = false;
    }

    private void ControlsPanel_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (Player?.MediaPlayer == null)
            return;
        if (ViewModel == null)
            return;
        
        var playerState = ViewModel.PlayerState;
            
        var currentPosition = e.GetCurrentPoint(this).Position;
        var movement = currentPosition - _previousMousePosition;
        _previousMousePosition = currentPosition;

        double range = Bounds.Width;
        
        // Left mouse button down drag: pitch and yaw.
        if (_isPanning)
        {
            _fov = Player.MediaPlayer.Viewpoint.Fov;
            float yaw = (float)(_fov * -movement.X / range);
            float pitch = (float)(_fov * -movement.Y / range);

            var vp = Player.MediaPlayer.Viewpoint;
            playerState.SetViewPointYaw(vp.Yaw + yaw, this);
            playerState.SetViewPointPitch(vp.Pitch + pitch, this);
            Player.MediaPlayer.UpdateViewpoint(vp.Yaw + yaw, vp.Pitch + pitch, vp.Roll, _fov);
        }

        // Right mouse button down: roll
        if (_isRolling)
        {
            _fov = Player.MediaPlayer.Viewpoint.Fov;
            float roll = (float)(_fov * -movement.X / range);
            
            var vp = Player.MediaPlayer.Viewpoint;
            playerState.SetViewPointRoll(vp.Roll + roll, this);
            Player.MediaPlayer.UpdateViewpoint(vp.Yaw, vp.Pitch, vp.Roll + roll, _fov);
        }

        // Middle mouse button down: fov
        if (_isFoving)
        {
            _fov = Player.MediaPlayer.Viewpoint.Fov;
            _fov = Math.Min(Math.Max(5, _fov + (float)(_fov * -movement.X / range)), 180);
            
            var vp = Player.MediaPlayer.Viewpoint;
            playerState.SetViewPointFov(_fov, this);
            Player.MediaPlayer.UpdateViewpoint(vp.Yaw, vp.Pitch, vp.Roll, _fov);
        }
    }

    private void StopClicked(object? sender, RoutedEventArgs e)
    {
        if (Player?.MediaPlayer == null)
            return;
        
        Player.MediaPlayer.SeekTo(TimeSpan.Zero);
        Player.MediaPlayer.Stop();
        PlayPauseButton.IsChecked = false;
    }

    private void OnPlayPauseToggled(object? sender, RoutedEventArgs e)
    {
        if (Player?.MediaPlayer == null)
            return;

        var newCheckedState = PlayPauseButton.IsChecked;
        
        if (Player.MediaPlayer.IsPlaying && newCheckedState == false)
            Player.MediaPlayer.Pause();
        else if (!Player.MediaPlayer.IsPlaying && newCheckedState == true)
            Player.MediaPlayer.Play();

    }

    public void Dispose()
    {
        // TODO this is not called automatically, see https://github.com/AvaloniaUI/Avalonia/discussions/6556
        // https://github.com/videolan/libvlcsharp/blob/3.x/docs/best_practices.md
        _libVlc.Dispose();
        _mediaPlayer.Dispose();
    }

    private void ResetRotationClicked(object? sender, RoutedEventArgs e)
    {
        if (Player?.MediaPlayer == null)
            return;

        if (ViewModel == null)
            return;

        _fov = _defaultFov;
        Player.MediaPlayer.UpdateViewpoint(0, 0, 0, _fov);
        
        var playerState = ViewModel.PlayerState;
        playerState.SetViewPointYaw(0, this);
        playerState.SetViewPointPitch(0, this);
        playerState.SetViewPointRoll(0, this);
        playerState.SetViewPointFov(_fov, this);
    }
    
    private void Scrubber_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (!_mediaPlayer.IsPlaying)
        {
            Console.WriteLine("Seeking to " + e.NewValue);
            _mediaPlayer.SeekTo(TimeSpan.FromSeconds(e.NewValue));
        }
        else
            Console.WriteLine("Scrubber value updated to " + e.NewValue);
    }

    private void CropTimelineStartButtonClick(object? sender, RoutedEventArgs e)
    {
        if (ViewModel == null)
            return;
        
        var timePositionMs = Math.Round((decimal)Scrubber.Value, 3);
        var playerState = ViewModel.PlayerState;
        CalculateAndSetCropMarkerPositions(timePositionMs, playerState.TimelineCropEndPosition);
        playerState.SetTimelineCropStartPosition(timePositionMs, this);
    }

    private void CropTimelineEndButtonClick(object? sender, RoutedEventArgs e)
    {
        if (ViewModel == null)
            return;
        
        var timePositionMs = Math.Round((decimal)Scrubber.Value, 3);
        var playerState = ViewModel.PlayerState;
        CalculateAndSetCropMarkerPositions(playerState.TimelineCropStartPosition, timePositionMs);
        playerState.SetTimelineCropEndPosition(timePositionMs, this);
    }
    
    private void CalculateAndSetCropMarkerPositions(decimal cropStart, decimal cropEnd)
    {
        var timeLineLength = ViewModel?.SourceVideo?.InputVideoInfo.DurationInSeconds ?? 1;
        if (timeLineLength <= 0)
            timeLineLength = 1;
        var startMarkerPos = (double)cropStart / (double)timeLineLength * CropTimelineCanvas.Bounds.Width;
        var endMarkerPos = (double)cropEnd / (double)timeLineLength * CropTimelineCanvas.Bounds.Width;

        Canvas.SetLeft(CropStartMarker, startMarkerPos);
        Canvas.SetLeft(CropEndMarker, endMarkerPos - CropEndMarker.Bounds.Width);
    }
}