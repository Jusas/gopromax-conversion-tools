using System.Text.Json;
using System.Text.Json.Nodes;
using FluentAssertions;
using VideoConversionApp.Config;
using VideoConversionApp.Services;

namespace VideoConversionApp.Tests;

public class ConfigManagerTests
{
    [Fact]
    public async Task TestLoadingDefaultConfig()
    {
        var randomConfigFilename = Path.GetRandomFileName();
        var manager = new ConfigManager();

        var result = manager.LoadConfigurations(randomConfigFilename);
        result.Should().Be(false);
        
        var pathsConfig = manager.GetConfig<PathsConfig>();
        pathsConfig.Should().NotBeNull();
        
        pathsConfig.Exiftool.Should().Be("exiftool");
        pathsConfig.Ffmpeg.Should().Be("ffmpeg");
        pathsConfig.Ffprobe.Should().Be("ffprobe");
        
        var conversionConfig = manager.GetConfig<ConversionConfig>();
        conversionConfig.Should().NotBeNull();
        
        var previewsConfig = manager.GetConfig<PreviewsConfig>();
        previewsConfig.Should().NotBeNull();
        
    }
    
    [Fact]
    public async Task TestLoadingPartialConfig()
    {
        var randomConfigFilename = Path.GetRandomFileName();
        var pathsConfig = new PathsConfig()
        {
            Exiftool = "test1",
            Ffmpeg = "test2",
            Ffprobe = "test3",
        };
        var jsonObj = new JsonObject();
        jsonObj.Add(pathsConfig.GetConfigurationKey(), pathsConfig.SerializeConfiguration());

        await File.WriteAllTextAsync(randomConfigFilename, JsonSerializer.Serialize(jsonObj));
        
        var manager = new ConfigManager();
        var result = manager.LoadConfigurations(randomConfigFilename);
        result.Should().Be(true);
        
        var loadedPaths = manager.GetConfig<PathsConfig>();
        loadedPaths.Should().NotBeNull();
        
        loadedPaths.Exiftool.Should().Be("test1");
        loadedPaths.Ffmpeg.Should().Be("test2");
        loadedPaths.Ffprobe.Should().Be("test3");
        
        var conversionConfig = manager.GetConfig<ConversionConfig>();
        conversionConfig.Should().NotBeNull();
        
        var previewsConfig = manager.GetConfig<PreviewsConfig>();
        previewsConfig.Should().NotBeNull();
        
    }
    
    [Fact]
    public async Task TestSavingConfig()
    {
        var randomConfigFilename = Path.GetRandomFileName();

        var manager = new ConfigManager();
        var result = manager.LoadConfigurations(randomConfigFilename);
        result.Should().Be(false);

        var pathsConfig = manager.GetConfig<PathsConfig>();
        pathsConfig.Exiftool = "test1";
        
        manager.SaveConfigurations(randomConfigFilename);

        var written = File.ReadAllText(randomConfigFilename);
        written.Should().Contain("test1");
        

    }
}