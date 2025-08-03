using System.Threading.Tasks;
using VideoConversionApp.Models;

namespace VideoConversionApp.Abstractions;

/// <summary>
/// Service that analyzes ane extracts information from video files.
/// </summary>
public interface IVideoInfoService
{
    /// <summary>
    /// Analyzes a file, extracts the relevant video information and also validates that it is
    /// a valid .360 video file.
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    Task<IInputVideoInfo> ParseMediaAsync(string filename);
    
}