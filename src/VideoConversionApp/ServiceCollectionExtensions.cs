using Microsoft.Extensions.DependencyInjection;
using VideoConversionApp.Abstractions;
using VideoConversionApp.Models;
using VideoConversionApp.Services;
using VideoConversionApp.ViewModels;

namespace VideoConversionApp;

public static class ServiceCollectionExtensions
{
    public static void AddCommonServices(this IServiceCollection collection)
    {

        collection.AddSingleton<ILogger, Logger>();
        collection.AddSingleton<IStorageServiceProvider, StorageServiceProvider>();
        
        collection.AddSingleton<IBitmapCache, BitmapCache>();
        collection.AddSingleton<IVideoInfoService, VideoInfoService>();
        collection.AddSingleton<IVideoPreviewService, VideoPreviewService>();
        collection.AddSingleton<IConfigManager, ConfigManager>();
        collection.AddSingleton<IVideoPoolManager, VideoPoolManager>();
        collection.AddSingleton<IAvFilterFactory, AvFilterFactory>();
        collection.AddSingleton<IVideoConverterService, VideoConverterService>();
        
        collection.AddSingleton<PreviewVideoPlayerState>();
        
        collection.AddSingleton<MainWindowViewModel>();
        collection.AddSingleton<MediaSelectionViewModel>();
        collection.AddSingleton<ConversionPreviewViewModel>();
        collection.AddSingleton<RenderSettingsViewModel>();
        collection.AddSingleton<RenderQueueViewModel>();
        collection.AddSingleton<RenderProcessControlViewModel>();
        collection.AddSingleton<GlobalSettingsViewModel>();
    }
}