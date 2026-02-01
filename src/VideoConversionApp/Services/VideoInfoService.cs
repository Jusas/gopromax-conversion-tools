using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LibVLCSharp.Shared;
using VideoConversionApp.Abstractions;
using VideoConversionApp.Config;

namespace VideoConversionApp.Services;

public class VideoInfoService : IVideoInfoService
{
    private class InputVideoInfo : IInputVideoInfo
    {
        public string Filename { get; }
        public bool IsValidVideo { get; }
        public bool IsGoProMaxFormat { get; }
        public int FirstVideoTrack { get; }
        public int SecondVideoTrack { get; }
        public int FirstAudioTrack { get; }
        public int SecondAudioTrack { get; }
        public decimal DurationInSeconds { get; }
        public DateTime CreatedDateTime { get; }
        public long SizeBytes { get; }
        public string[]? ValidationIssues { get; }
        
        public InputVideoInfo(string filename, bool isValidVideo, bool isGoProMaxFormat, int firstVideoTrack, int secondVideoTrack, int firstAudioTrack, int secondAudioTrack, 
            long durationMilliseconds, DateTime createdDateTime, long sizeBytes, string[]? validationIssues)
        {
            Filename = filename;
            IsValidVideo = isValidVideo;
            IsGoProMaxFormat = isGoProMaxFormat;
            DurationInSeconds = (decimal)durationMilliseconds / 1000;
            CreatedDateTime = createdDateTime;
            SizeBytes = sizeBytes;
            ValidationIssues = validationIssues;
            FirstVideoTrack = firstVideoTrack;
            SecondVideoTrack = secondVideoTrack;
            FirstAudioTrack = firstAudioTrack;
            SecondAudioTrack = secondAudioTrack;
        }
    }

    // Corresponding to ffprobe output json structure
    private class ProbedStreamInfo
    {
        public int Index  { get; set; }
        public string? CodecType { get; set; }
        public string? CodecName { get; set; }
        public Dictionary<string, string> Tags { get; set; }
    }
    
    // Corresponding to ffprobe output json structure
    private class ProbeInfo
    {
        public ProbedStreamInfo[] Streams { get; set; }
    }
    
    private static LibVLC? _libVlc = null;
    private readonly IConfigManager _configManager;
    private readonly ILogger _logger;

    public VideoInfoService(IConfigManager configManager, ILogger logger)
    {
        _configManager = configManager;
        _logger = logger;
    }
    
    public async Task<IInputVideoInfo> ParseMediaAsync(string filename)
    {
        if (!File.Exists(filename))
        {
            throw new FileNotFoundException(filename);
        }

        try
        {
            if (_libVlc == null)
                _libVlc = new LibVLC();
        }
        catch (VLCException e)
        {
            throw;
        }

        var defaultVideoCreateTime = File.GetCreationTime(filename);
        var sizeBytes = new FileInfo(filename).Length;
        
        using var media = new Media(_libVlc, filename);
        await media.Parse(MediaParseOptions.ParseLocal, 2000);

        if (media.Duration < 0)
            return new InputVideoInfo(filename, false, false, -1, -1, -1, -1, 0, 
                defaultVideoCreateTime, sizeBytes, ["Media duration was reported as < 0"]);

        var validationIssues = new List<string>(); 
        var isGoProMaxFormat = ValidateGoProMaxVideo(media, filename, validationIssues);
        
        var dateString = media.Meta(MetadataType.Date) ?? defaultVideoCreateTime.ToString(CultureInfo.CurrentCulture);
        var videoCreateTime = DateTime.Parse(dateString);

        var firstVideoTrack = -1;
        var secondVideoTrack = -1;
        var firstAudioTrack = -1;
        var secondAudioTrack = -1;

        if (isGoProMaxFormat)
            (firstVideoTrack, secondVideoTrack, firstAudioTrack, secondAudioTrack) = await GetTrackInfoWithFfprobe(filename);
        
        return new InputVideoInfo(filename, true, isGoProMaxFormat, firstVideoTrack, 
            secondVideoTrack, firstAudioTrack, secondAudioTrack, media.Duration, videoCreateTime, 
            sizeBytes, validationIssues.ToArray());

    }

    private async Task<(int v1, int v2, int a1, int a2)> GetTrackInfoWithFfprobe(string filename)
    {
        var pathsConfig = _configManager.GetConfig<PathsConfig>()!;
        
        var argsList = new List<string>
        {
            "-v", "quiet",
            "-print_format", "json",
            "-show_streams", 
            filename
        };
        
        var firstVideoTrack = -1;
        var secondVideoTrack = -1;
        var firstAudioTrack = -1;
        var secondAudioTrack = -1;
        
        var processStartInfo = new ProcessStartInfo(pathsConfig.Ffprobe, argsList)
        {
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true
        };

        try
        {
            var process = new Process()
            {
                StartInfo = processStartInfo
            };
            
            _logger.LogVerbose($"ffprobe probing video track information for " + filename);
            process.Start();
            await process!.WaitForExitAsync();
            
            var processOutput = process.StandardOutput.ReadToEnd();

            try
            {
                var probeInfo = JsonSerializer.Deserialize<ProbeInfo>(processOutput, new JsonSerializerOptions()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });
                if (probeInfo!.Streams.Length == 0)
                    return (-1, -1, -1, 1);

                foreach (var stream in probeInfo.Streams)
                {
                    if (stream.CodecType == "video" && firstVideoTrack == -1)
                    {
                        firstVideoTrack = stream.Index;
                        continue;
                    }

                    if (stream.CodecType == "video" && secondVideoTrack == -1)
                    {
                        secondVideoTrack = stream.Index;
                        continue;
                    }

                    if (stream.CodecType == "audio" && firstAudioTrack == -1)
                    {
                        firstAudioTrack = stream.Index;
                        continue;
                    }

                    if (stream.CodecType == "audio" && secondAudioTrack == -1)
                    {
                        secondAudioTrack = stream.Index;
                    }
                }
                
                return (firstVideoTrack, secondVideoTrack, firstAudioTrack, secondAudioTrack);
            }
            catch (JsonException e)
            {
                _logger.LogError($"{nameof(GetTrackInfoWithFfprobe)}: failure in json deserialize of ffprobe output; {e}");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(GetTrackInfoWithFfprobe)}: failure in deserializing ffprobe output; {e}");
                throw;
            }
            
        }
        catch (Exception e)
        {
            _logger.LogError($"{nameof(GetTrackInfoWithFfprobe)}: failed to run ffprobe; {e}");
            throw new Exception("Failed to get video track information using ffprobe", e);
        }

    }
    

    /// <summary>
    /// Rudimentary check using LibVLC to determine whether this is a valid .360 GoPro MAX video.
    /// We could do more thorough check with FFMPEG, but for now this will do.
    /// </summary>
    /// <param name="media"></param>
    /// <param name="filename"></param>
    /// <param name="validationIssues"></param>
    /// <returns></returns>
    private static bool ValidateGoProMaxVideo(Media media, string filename, List<string> validationIssues)
    {
        // Ends with .360, simplest check.
        if (!filename.EndsWith(".360"))
            validationIssues.Add("Filename extension is not .360");
        
        // Frame size that we are able to handle is 4096 x 1344.
        const uint supportedVideoWidth = 4096;
        const uint supportedVideoHeight = 1344;
        
        // GoPro MAX videos have 6 tracks, but 4 media tracks (2 video, 2 audio).
        // LibVLC only lists the 4 media tracks.
        var hasTwoVideoTracks = media.Tracks.Count(t => t.TrackType == TrackType.Video) == 2;

        if (!hasTwoVideoTracks)
            validationIssues.Add("Expected to find 2 video tracks");
        
        var videoTracks = media.Tracks.Where(t => t.TrackType == TrackType.Video);
        int i = 0;
        foreach (var track in videoTracks)
        {
            if (track.Data.Video.Width != supportedVideoWidth || track.Data.Video.Height != supportedVideoHeight)
                validationIssues.Add($"Video track {i} size expected to be {supportedVideoWidth}x{supportedVideoHeight}, " +
                                     $"but found {track.Data.Video.Width}x{track.Data.Video.Height}");
            i++;
        }

        return validationIssues.Count == 0;
    }
}