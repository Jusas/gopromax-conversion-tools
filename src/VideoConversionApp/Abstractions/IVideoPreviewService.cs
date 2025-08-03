using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VideoConversionApp.Models;

namespace VideoConversionApp.Abstractions;

/// <summary>
/// Service for generating media previews (thumbnails, snapshot frames, keyframe videos).
/// </summary>
public interface IVideoPreviewService
{
    /// <summary>
    /// Generate a number of snapshot frames from a GoPro Max 360 video.
    /// </summary>
    /// <param name="inputVideo">Source media info</param>
    /// <param name="numberOfFrames">Number of frames to generate</param>
    /// <param name="progressCallback">Optional callback for receiving progress info</param>
    /// <param name="cancellationToken">Token for cancelling the process</param>
    /// <returns>A list of generated frames (images as byte arrays) when generation is complete</returns>
    Task<IList<byte[]>> GenerateSnapshotFramesAsync(IInputVideoInfo inputVideo, SnapshotFrameTransformationSettings settings, 
        int numberOfFrames, Action<double>? progressCallback = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Queues the generation of a video thumbnail of a GoPro Max 360 video.
    /// </summary>
    /// <param name="inputVideo">Source media info</param>
    /// <param name="timePositionMilliseconds">The time position of the thumbnail</param>
    /// <returns>The task which when completed will provide the thumbnail image as bytes in the result.
    /// Note that since the task is queued, awaiting for it may take a long time.
    /// </returns>
    Task<byte[]?> QueueThumbnailGenerationAsync(IInputVideoInfo inputVideo, long timePositionMilliseconds);

    /// <summary>
    /// Generates a keyframe video out of the specified input.
    /// </summary>
    /// <param name="video">Video model with conversion settings</param>
    /// <param name="progressCallback">Optional callback for receiving progress info</param>
    /// <param name="cancellationToken">Token for cancelling the process</param>
    /// <returns>Generated video information</returns>
    Task<KeyFrameVideo> GenerateKeyFrameVideoAsync(IConvertableVideo video,
        Action<double>? progressCallback = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a cached thumbnail for the given media.
    /// </summary>
    /// <param name="inputVideo"></param>
    /// <returns>Null if the media doesn't have a thumbnail cached, otherwise the image bytes.</returns>
    byte[]? GetCachedThumbnail(IInputVideoInfo inputVideo);
    
    /// <summary>
    /// Returns a cached thumbnail for the given media.
    /// </summary>
    /// <param name="videoFilename"></param>
    /// <returns>Null if the media doesn't have a thumbnail cached, otherwise the image bytes.</returns>
    byte[]? GetCachedThumbnail(string videoFilename);
}