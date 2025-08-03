using VideoConversionApp.Services;

namespace VideoConversionApp.Tests;

public class VideoInfoServiceTests
{
    [Fact]
    public async Task TestGetMediaInfo()
    {
        var mediaFile = "/drive/data/Media/VideoEditing/ProjectDirs/Snouk Season 24-25/GS010176.360";
        var service = new VideoInfoService();
        
        var info = await service.ParseMediaAsync(mediaFile);
        
    }
}