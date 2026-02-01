using Moq;
using VideoConversionApp.Abstractions;
using VideoConversionApp.Config;
using VideoConversionApp.Services;

namespace VideoConversionApp.Tests;

public class VideoInfoServiceTests
{
    [Fact]
    public async Task TestGetMediaInfo()
    {
        var mediaFile = "/drive/data/Media/VideoEditing/ProjectDirs/Snouk Season 24-25/GS010176.360";
        var tmpDir = Path.Combine(Path.GetTempPath(), "videoConversionTests");
        
        var mockLogger = new Mock<ILogger>();
        var configManager = new ConfigManager();
        configManager.LoadConfigurations("test.config");
        configManager.GetConfig<ConversionConfig>()!.OutputBesideOriginals = false;
        configManager.GetConfig<ConversionConfig>()!.OutputDirectory = tmpDir;
        var service = new VideoInfoService(configManager, mockLogger.Object);
        
        var info = await service.ParseMediaAsync(mediaFile);
        
    }
}