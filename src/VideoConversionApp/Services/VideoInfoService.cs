using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LibVLCSharp.Shared;
using VideoConversionApp.Abstractions;
using VideoConversionApp.Models;

namespace VideoConversionApp.Services;

public class VideoInfoService : IVideoInfoService
{
    private class InputVideoInfo : IInputVideoInfo
    {
        public string Filename { get; }
        public bool IsValidVideo { get; }
        public bool IsGoProMaxFormat { get; }
        public decimal DurationInSeconds { get; }
        public DateTime CreatedDateTime { get; }
        public long SizeBytes { get; }
        public string[]? ValidationIssues { get; }
        
        public InputVideoInfo(string filename, bool isValidVideo, bool isGoProMaxFormat, 
            long durationMilliseconds, DateTime createdDateTime, long sizeBytes, string[]? validationIssues)
        {
            Filename = filename;
            IsValidVideo = isValidVideo;
            IsGoProMaxFormat = isGoProMaxFormat;
            DurationInSeconds = (decimal)durationMilliseconds / 1000;
            CreatedDateTime = createdDateTime;
            SizeBytes = sizeBytes;
            ValidationIssues = validationIssues;
        }
    }
    
    private static LibVLC? _libVlc = null;
    
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
            return new InputVideoInfo(filename, false, false, 0, 
                defaultVideoCreateTime, sizeBytes, ["Media duration was reported as < 0"]);

        var validationIssues = new List<string>(); 
        var isGoProMaxFormat = ValidateGoProMaxVideo(media, filename, validationIssues);
        
        var dateString = media.Meta(MetadataType.Date) ?? defaultVideoCreateTime.ToString(CultureInfo.CurrentCulture);
        var videoCreateTime = DateTime.Parse(dateString);
        
        return new InputVideoInfo(filename, true, isGoProMaxFormat, media.Duration, videoCreateTime, 
            sizeBytes, validationIssues.ToArray());

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
        var hasTwoAudioTracks = media.Tracks.Count(t => t.TrackType == TrackType.Audio) == 2;

        if (!hasTwoAudioTracks)
            validationIssues.Add("Expected to find 2 audio tracks");
        
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