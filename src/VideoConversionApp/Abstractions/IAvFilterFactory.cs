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
    /// <param name="frameSelectCondition">Limit frame input/output to this selection</param>
    /// <param name="frameRotation">Rotation parameters</param>
    /// <returns>Filter string to be used in -filter_complex</returns>
    string BuildAvFilter(AvFilterFrameSelectCondition? frameSelectCondition = null,
        AvFilterFrameRotation? frameRotation = null);
}