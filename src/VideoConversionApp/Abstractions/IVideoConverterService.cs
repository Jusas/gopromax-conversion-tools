using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VideoConversionApp.Models;

namespace VideoConversionApp.Abstractions;

/// <summary>
/// The service that performs GoPro Max .360 video to equirectangular video conversion.
/// </summary>
public interface IVideoConverterService
{
    event EventHandler RenderingQueueProcessingStarted;
    event EventHandler RenderingQueueProcessingFinished;
    event EventHandler<VideoRenderQueueEntry> RenderingFailed;
    event EventHandler<VideoRenderQueueEntry> RenderingSucceeded;
    event EventHandler<VideoRenderQueueEntry> RenderingCanceled;
    
    IReadOnlyList<CodecEntry> GetAvailableVideoCodecs();
    IReadOnlyList<CodecEntry> GetAvailableAudioCodecs();
    IReadOnlyList<CodecEntry> GetAvailableContainers();
    string GetFilenameFromPattern(IConvertableVideo video, string pattern);
    
    /// <summary>
    /// Convert videos in a rendering queue using the settings retrieved from <see cref="IConfigManager"/>.
    /// </summary>
    /// <param name="renderingQueue">The queue to render</param>
    /// <param name="renderAll">Render all in queue, including those that have already been rendered.</param>
    /// <returns></returns>
    Task<bool> ConvertVideosAsync(IList<VideoRenderQueueEntry> renderingQueue, bool renderAll);

    /// <summary>
    /// Signals cancellation for all videos currently being rendered or queued for rendering.
    /// </summary>
    void SignalCancellation();
    
    /// <summary>
    /// Signals cancellation for a single render queue entry.
    /// </summary>
    /// <param name="renderQueueEntry"></param>
    void SignalCancellation(VideoRenderQueueEntry renderQueueEntry);

}