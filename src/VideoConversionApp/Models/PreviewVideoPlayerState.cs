using System;
// ReSharper disable CompareOfFloatsByEqualityOperator

namespace VideoConversionApp.Models;

/// <summary>
/// A state object for keeping tabs on the video player state and to have better control
/// over the event handling.
/// </summary>
public class PreviewVideoPlayerState
{
    public class StateEventArgs<T>
    {
        public object? Context { get; init; }
        public T Value { get; init; }
    }
    
    public static readonly float DefaultFov = 80.0f;
    
    public float ViewPointYaw { get; private set; }
    public float ViewPointPitch { get; private set; }
    public float ViewPointRoll { get; private set; }
    public float ViewPointFov { get; private set; } = DefaultFov;
    
    public decimal TimelineCropStartPosition { get; private set; }
    public decimal TimelineCropEndPosition { get; private set; }

    public bool ControlsEnabled { get; private set; } = true;
    
    public event EventHandler<StateEventArgs<float>>? ViewPointYawChanged;
    public event EventHandler<StateEventArgs<float>>? ViewPointPitchChanged;
    public event EventHandler<StateEventArgs<float>>? ViewPointRollChanged;
    public event EventHandler<StateEventArgs<float>>? ViewPointFovChanged;
    public event EventHandler<StateEventArgs<decimal>>? TimelineCropStartPositionChanged;
    public event EventHandler<StateEventArgs<decimal>>? TimelineCropEndPositionChanged;
    public event EventHandler<StateEventArgs<bool>>? ControlsEnabledChanged;

    public PreviewVideoPlayerState()
    {
    }
    
    public void SetViewPointYaw(float value, object context, bool raiseEvent = true)
    {
        if (ViewPointYaw == value) 
            return;
        
        ViewPointYaw = value;
        if (raiseEvent)
            ViewPointYawChanged?.Invoke(this, new StateEventArgs<float>() { Context = context, Value = value });
    }
    
    public void SetViewPointPitch(float value, object context, bool raiseEvent = true)
    {
        if (ViewPointPitch == value) 
            return;
        
        ViewPointPitch = value;
        if (raiseEvent)
            ViewPointPitchChanged?.Invoke(this, new StateEventArgs<float>() { Context = context, Value = value });
    }
    
    public void SetViewPointRoll(float value, object context, bool raiseEvent = true)
    {
        if (ViewPointRoll == value) 
            return;
        
        ViewPointRoll = value;
        if (raiseEvent)
            ViewPointRollChanged?.Invoke(this, new StateEventArgs<float>() { Context = context, Value = value });
    }
    
    public void SetViewPointFov(float value, object context, bool raiseEvent = true)
    {
        if (ViewPointFov == value) 
            return;
        
        ViewPointFov = value;
        if (raiseEvent)
            ViewPointFovChanged?.Invoke(this, new StateEventArgs<float>() { Context = context, Value = value });
    }
    
    public void SetTimelineCropStartPosition(decimal value, object context, bool raiseEvent = true)
    {
        if (TimelineCropStartPosition == value) 
            return;
        
        TimelineCropStartPosition = value;
        if (raiseEvent)
            TimelineCropStartPositionChanged?.Invoke(this, new StateEventArgs<decimal>() { Context = context, Value = value });
    }
    
    public void SetTimelineCropEndPosition(decimal value, object context, bool raiseEvent = true)
    {
        if (TimelineCropEndPosition == value) 
            return;
        
        TimelineCropEndPosition = value;
        if (raiseEvent)
            TimelineCropEndPositionChanged?.Invoke(this, new StateEventArgs<decimal>() { Context = context, Value = value });
    }

    public void SetControlsEnabled(bool value, object context, bool raiseEvent = true)
    {
        if (ControlsEnabled == value)
            return;
        
        ControlsEnabled = value;
        if (raiseEvent)
            ControlsEnabledChanged?.Invoke(this, new StateEventArgs<bool>() { Context = context, Value = value });
    }

}