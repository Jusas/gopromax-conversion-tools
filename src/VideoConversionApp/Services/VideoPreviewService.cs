using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Platform;
using VideoConversionApp.Abstractions;
using VideoConversionApp.Config;
using VideoConversionApp.Models;
using VideoConversionApp.Utils;
using File = System.IO.File;

namespace VideoConversionApp.Services;

public class VideoPreviewService : IVideoPreviewService
{
    /// <summary>
    /// A job class for our queues.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    private class ThreadWorkItem<T, TResult>
    {
        public T WorkItem;
        public TResult Result;
        public CancellationToken CancellationToken;
    }

    private IConfigManager _configManager;
    private readonly IAvFilterFactory _avFilterFactory;
    private readonly ILogger _logger;
    private List<Thread> _thumbnailQueueProcessingThreads = new();
    private ConcurrentDictionary<string, byte[]> _thumbnailCache = new();

    private ConcurrentQueue<ThreadWorkItem<(IInputVideoInfo mediaInfo, long timePosMs), TaskCompletionSource<byte[]?>>> 
        _thumbnailGenerationQueue = new();

    
    public VideoPreviewService(IConfigManager configManager,
        IAvFilterFactory avFilterFactory,
        ILogger logger)
    {
        _configManager = configManager;
        _avFilterFactory = avFilterFactory;
        _logger = logger;

        CachePlaceholderThumbnail();
    }

    private void CachePlaceholderThumbnail()
    {
        using var defaultThumb = AssetLoader.Open(new Uri("avares://VideoConversionApp/Images/placeholder_snapframe.png"));
        var buffer = new byte[defaultThumb.Length];
        defaultThumb.ReadExactly(buffer, 0, buffer.Length);
        _thumbnailCache.TryAdd(IInputVideoInfo.PlaceHolderFilename, buffer);
    }
    
    public Task<byte[]?> QueueThumbnailGenerationAsync(IInputVideoInfo inputVideo, long timePositionMilliseconds)
    {
        var threadWorkItem = new ThreadWorkItem<(IInputVideoInfo, long), TaskCompletionSource<byte[]?>>
        {
            WorkItem = (inputVideo, timePositionMilliseconds),
            Result = new TaskCompletionSource<byte[]?>(),
            CancellationToken = CancellationToken.None
        };
        _thumbnailGenerationQueue.Enqueue(threadWorkItem);
        // Run immediately.
        RunThumbnailProcessingThreads();
        return threadWorkItem.Result.Task;
    }

    /// <summary>
    /// Creates/starts a new processing thread, as many as are allowed concurrently, and cleans up completed ones.
    /// </summary>
    private void RunThumbnailProcessingThreads()
    {
        var threadCount = _configManager.GetConfig<PreviewsConfig>()!.NumberOfThumbnailThreads;
        lock (_thumbnailQueueProcessingThreads)
        {
            for (var i = _thumbnailQueueProcessingThreads.Count - 1; i >= 0; i--)
            {
                if(!_thumbnailQueueProcessingThreads[i].IsAlive)
                    _thumbnailQueueProcessingThreads.RemoveAt(i);
            }

            if (_thumbnailQueueProcessingThreads.Count < threadCount)
            {
                var thread = new Thread(ProcessThumbnailQueue);
                _thumbnailQueueProcessingThreads.Add(thread);
                thread.Start();
            }
        }
    }
    
    /// <summary>
    /// The thumbnail processing thread function.
    /// Processes the queue.
    /// </summary>
    private void ProcessThumbnailQueue()
    {
        var pathsConfig = _configManager.GetConfig<PathsConfig>()!;
        
        var avFilterString = _avFilterFactory.BuildAvFilter(
            new AvFilterFrameSelectCondition() { KeyFramesOnly = true }, AvFilterFrameRotation.Zero);
        
        while (_thumbnailGenerationQueue.TryDequeue(out var threadWorkItem))
        {
            var (mediaInfo, timePosMs) = threadWorkItem.WorkItem;
            var tmpThumbFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".jpg");
            var thumbTimePosition = timePosMs / 1000.0;
            
            var processStartInfo = new ProcessStartInfo(pathsConfig.Ffmpeg,
                [
                    "-loglevel", "8",
                    "-discard", "nokey",
                    "-y",
                    "-ss", $"{TimeSpan.FromSeconds(thumbTimePosition):hh\\:mm\\:ss}",
                    "-i", mediaInfo.Filename,
                    "-vsync", "0",
                    "-filter_complex", avFilterString,
                    "-map", "[OUTPUT_FRAME]",
                    "-frames:v", "1",
                    "-f", "image2",
                    "-s", "336x199",
                    tmpThumbFilePath
                ])
                {
                    CreateNoWindow = true,
                    UseShellExecute = false
                };

                
            try
            {
                var process = new Process()
                {
                    StartInfo = processStartInfo
                };
            
                process.Start();
                process!.WaitForExit();
                if (process.ExitCode == 0 && File.Exists(tmpThumbFilePath))
                {
                    var imgBytes = File.ReadAllBytes(tmpThumbFilePath);
                    _thumbnailCache.AddOrUpdate(mediaInfo.Filename, imgBytes, (_, _) => imgBytes);
                    File.Delete(tmpThumbFilePath);
                    threadWorkItem.Result.TrySetResult(imgBytes);
                }
                else
                {
                    threadWorkItem.Result.TrySetResult(null);
                }
            }
            catch (Exception e)
            {
                threadWorkItem.Result.TrySetResult(null);
            }
        }
            
    }

    public async Task<IList<byte[]>> GenerateSnapshotFramesAsync(IInputVideoInfo inputVideo, 
        SnapshotFrameTransformationSettings settings, int numberOfFrames, 
        Action<double>? progressCallback = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"Generating snapshot frames for {inputVideo.Filename}");
        if (numberOfFrames < 2)
            throw new ArgumentException("Number of frames must be at least 2");
     
        var pathsConfig = _configManager.GetConfig<PathsConfig>()!;
        var skipLength = inputVideo.DurationInSeconds / (numberOfFrames - 1);

        var avFilterString = _avFilterFactory.BuildAvFilter(new AvFilterFrameSelectCondition()
        {
            FrameDistance = (double)Math.Round(skipLength, 3),
            KeyFramesOnly = true
        }, settings.Rotation);
        
        // TODO caching
        var genId = Guid.NewGuid();
        var tempPath = Path.Combine(Path.GetTempPath(), genId.ToString());
        Directory.CreateDirectory(tempPath);
        
        var tmpFrameFilePath = Path.Combine(tempPath, "snapshot-%06d.jpg");

        var argsList = new List<string>
        {
            "-loglevel", "8",
            "-discard", "nokey",
            "-y",
            "-progress", "pipe:1",
            "-stats_period", "0.25",
            "-vsync", "0",
            "-filter_complex", avFilterString,
            "-i", inputVideo.Filename,
            "-map", "[OUTPUT_FRAME]",
            "-f", "image2",
            "-s", "672x398",
            tmpFrameFilePath
        };
        _logger.LogVerbose($"ffmpeg args: {string.Join(" ", argsList)}");
        var processStartInfo = new ProcessStartInfo(pathsConfig.Ffmpeg, argsList)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true
            };
            
        try
        {
            var process = new Process()
            {
                StartInfo = processStartInfo,
                EnableRaisingEvents = true
            };
            process.Start();
            cancellationToken.Register(() => process.Kill());

            process.BeginOutputReadLine();
            progressCallback?.Invoke(5);

            process!.OutputDataReceived += (sender, args) =>
            {
                _logger.LogVerbose(args.Data);
                if (!string.IsNullOrEmpty(args.Data) && Regex.IsMatch(args.Data, @"^frame=\d"))
                {
                    var renderedFrameNumber = int.Parse(args.Data.Split("=")[1], CultureInfo.InvariantCulture);
                    progressCallback?.Invoke(Math.Round((double)renderedFrameNumber / numberOfFrames * 100.0, 2));
                }
                if (!string.IsNullOrEmpty(args.Data) && Regex.IsMatch(args.Data, @"^progress=end"))
                {
                    progressCallback?.Invoke(100.0);
                }
            };
            await process!.WaitForExitAsync(cancellationToken);
            if (process.ExitCode == 0 && Directory.Exists(tempPath))
            {
                _logger.LogInformation($"Generated snapshot frames successfully");
                var frames = Directory.GetFiles(tempPath, "*.jpg");
                var frameBytes = new byte[frames.Length][];

                for (var i = 0; i < frames.Length; i++)
                {
                    frameBytes[i] = await File.ReadAllBytesAsync(frames[i]);
                    File.Delete(frames[i]);
                }
                
                return frameBytes;
            }
            else
            {
                throw new Exception($"Ffmpeg process returned {process.ExitCode}");
            }
        }
        catch (TaskCanceledException e)
        {
            _logger.LogVerbose($"ffmpeg process cancelled: {e.Message}");
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError($"Generating snapshot frames failed: {e}");
            throw;
        }
        
        
    }

    public async Task<KeyFrameVideo> GenerateKeyFrameVideoAsync(IConvertableVideo video,
        Action<double>? progressCallback = null,
        CancellationToken cancellationToken = default)
    {
        var videoInfo = video.InputVideoInfo;
        var rotation = video.FrameRotation;
        var pathsConfig = _configManager.GetConfig<PathsConfig>()!;
        
        _logger.LogInformation($"Generating keyframe video for {videoInfo.Filename}");

        var avFilterString = _avFilterFactory.BuildAvFilter(new AvFilterFrameSelectCondition()
        {
            KeyFramesOnly = true
        }, rotation);
        
        var genId = Guid.NewGuid();
        var tempPath = Path.Combine(Path.GetTempPath(), "kfv-" + genId.ToString());
        Directory.CreateDirectory(tempPath);
        
        var tmpVideoFilePath = Path.Combine(tempPath, "keyframevideo.mp4");

        var argsList = new List<string>
        {
            "-loglevel", "8",
            "-discard", "nokey",
            "-y",
            "-progress", "pipe:1",
            "-stats_period", "0.25",
            "-vsync", "0",
            "-filter_complex", avFilterString,
            "-i", videoInfo.Filename,
            "-map", "[OUTPUT_FRAME]",
            "-c:v", "prores",
            "-f", "mov",
            "-s", "672x398",
            "-movflags", "+faststart",
            tmpVideoFilePath
        };
        _logger.LogVerbose($"ffmpeg args: {string.Join(" ", argsList)}");
        var processStartInfo = new ProcessStartInfo(pathsConfig.Ffmpeg, argsList)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true
            };
            
        try
        {
            var process = new Process()
            {
                StartInfo = processStartInfo,
                EnableRaisingEvents = true
            };
            process.Start();
            cancellationToken.Register(() => process.Kill());

            process.BeginOutputReadLine();
            progressCallback?.Invoke(2);

            process!.OutputDataReceived += (sender, args) =>
            {
                _logger.LogVerbose(args.Data);
                if (!string.IsNullOrEmpty(args.Data) && Regex.IsMatch(args.Data, @"^out_time=\d"))
                {
                    var frameTime = TimeSpan.Parse(args.Data.Split("=")[1], CultureInfo.InvariantCulture);
                    var progress = frameTime.TotalMilliseconds / videoInfo.DurationInSeconds.AsMillisecondsDouble();
                    progressCallback?.Invoke(Math.Round(progress * 100.0, 2));
                }
                if (!string.IsNullOrEmpty(args.Data) && Regex.IsMatch(args.Data, @"^progress=end"))
                {
                    progressCallback?.Invoke(98.0);
                }
            };
            await process!.WaitForExitAsync(cancellationToken);
            if (process.ExitCode == 0 && File.Exists(tmpVideoFilePath))
            {
                _logger.LogInformation($"Generated keyframe video successfully, tagging video with exiftool");
                var taggingProcessStartInfo = new ProcessStartInfo(pathsConfig.Exiftool,
                    [
                        "-api", "LargeFileSupport=1",
                        "-overwrite_original",
                        "-XMP-GSpherical:Spherical=true",
                        "-XMP-GSpherical:Stitched=true",
                        "-XMP-GSpherical:StitchingSoftware=MAXVideoConverter",
                        "-XMP-GSpherical:ProjectionType=equirectangular",
                        tmpVideoFilePath
                    ])
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };
                var taggingProcess = new Process()
                {
                    StartInfo = taggingProcessStartInfo,
                    EnableRaisingEvents = true
                };
                taggingProcess.Start();
                cancellationToken.Register(() => taggingProcess.Kill());
                
                await taggingProcess!.WaitForExitAsync(cancellationToken);
                progressCallback?.Invoke(100.0);
                if (taggingProcess.ExitCode == 0)
                {
                    _logger.LogInformation($"Video tagged successfully");
                    return new KeyFrameVideo()
                    {
                        VideoPath = tmpVideoFilePath,
                        SourceVideo = video
                    };
                }
                throw new Exception($"ExifTool process returned {taggingProcess.ExitCode}");
            }
            else
            {   
                throw new Exception($"Ffmpeg process returned {process.ExitCode}");
            }
        }
        catch (TaskCanceledException e)
        {
            _logger.LogVerbose($"ffmpeg process cancelled: {e.Message}");
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError($"Generating keyframe video failed: {e}");
            throw;
        }
        
    }

    public byte[]? GetCachedThumbnail(IInputVideoInfo inputVideo)
    {
        // TODO use the bitmap cache?
        return _thumbnailCache.GetValueOrDefault(inputVideo.Filename);
    }

    public byte[]? GetCachedThumbnail(string videoFilename)
    {
        // TODO use the bitmap cache?
        return _thumbnailCache.GetValueOrDefault(videoFilename);
    }
}