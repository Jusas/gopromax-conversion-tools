using VideoConversionApp.Models;

namespace VideoConversionApp.Abstractions;

/// <summary>
/// For generating FFMPEG complex AV filters. 
/// </summary>
public interface IAvFilterFactory
{
    /// <summary>
    /// Builds an AV filter to be used with FFMPEG -filter_complex based on given parameters.
    /// </summary>
    /// <param name="videoTrack1Index">Index of the first video stream in the .360 file, these differ depending on
    /// whether the video has audio tracks.</param>
    /// <param name="videoTrack2Index">Index of the second video stream in the .360 file, these differ depending on
    /// whether the video has audio tracks.</param>
    /// <param name="frameSelectCondition">Limit frame input/output to this selection</param>
    /// <param name="frameRotation">Rotation parameters</param>
    /// <returns>Filter string to be used in -filter_complex</returns>
    string BuildAvFilter(int videoTrack1Index, int videoTrack2Index, AvFilterFrameSelectCondition? frameSelectCondition = null,
        AvFilterFrameRotation? frameRotation = null);
}