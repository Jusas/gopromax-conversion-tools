using Moq;
using VideoConversionApp.Abstractions;
using VideoConversionApp.Config;
using VideoConversionApp.Models;
using VideoConversionApp.Services;

namespace VideoConversionApp.Tests;

public class VideoConversionTests
{
    [Fact]
    public async Task ShouldConvertVideos()
    {
        var videoDir = "/drive/data/Media/VideoEditing/ProjectDirs/TestingVideos";
        var tmpDir = Path.Combine(Path.GetTempPath(), "videoConversionTests");
        Directory.CreateDirectory(tmpDir);
        
        var filterFactory = new AvFilterFactory();
        var poolManager = new VideoPoolManager();
        var videoInfoService = new VideoInfoService();
        var vids = Directory.GetFiles(videoDir, "*.360");

        foreach (var vid in vids)
        {
            var maxVideo = await videoInfoService.ParseMediaAsync(vid);
            poolManager.AddVideoToPool(maxVideo);
        }
        
        var mockLogger = new Mock<ILogger>();
        
        var configManager = new ConfigManager();
        configManager.LoadConfigurations("test.config");
        configManager.GetConfig<ConversionConfig>()!.OutputBesideOriginals = false;
        configManager.GetConfig<ConversionConfig>()!.OutputDirectory = tmpDir;

        var queueEntries = poolManager.VideoPool.Select(x => new VideoRenderQueueEntry(x)).ToList();
        var conversionService = new VideoConverterService(configManager, filterFactory, mockLogger.Object);
        await conversionService.ConvertVideosAsync(queueEntries, true);

    }
}