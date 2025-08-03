using System;
using System.Collections.Generic;
using VideoConversionApp.Models;

namespace VideoConversionApp.Abstractions;

/// <summary>
/// Manager of the conversion process pipeline.
/// Contains the methods to control the conversion queue.
/// </summary>
public interface IVideoPoolManager 
{
    event EventHandler<IConvertableVideo>? VideoAddedToPool;
    event EventHandler<IConvertableVideo>? VideoRemovedFromPool;

    IReadOnlyList<IConvertableVideo> VideoPool { get; }
    
    IConvertableVideo GetPlaceholderVideo();
    IConvertableVideo GetDummyVideo();
    IConvertableVideo AddVideoToPool(IInputVideoInfo inputVideoInfo);
    void RemoveVideoFromPool(IConvertableVideo video);
}