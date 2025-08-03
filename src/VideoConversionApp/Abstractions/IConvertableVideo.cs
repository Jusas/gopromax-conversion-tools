using System;
using System.ComponentModel;
using VideoConversionApp.Models;

namespace VideoConversionApp.Abstractions;

public interface IConvertableVideo : INotifyPropertyChanged
{
    public IInputVideoInfo InputVideoInfo { get; }
    public AvFilterFrameRotation FrameRotation { get; set; }
    public TimelineCrop TimelineCrop { get; set; }
    public bool IsEnabledForConversion { get; set; }
    public bool HasNonDefaultSettings { get; }
}