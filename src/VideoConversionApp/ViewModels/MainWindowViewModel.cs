using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using VideoConversionApp.Abstractions;
using VideoConversionApp.Models;
using VideoConversionApp.Services;

namespace VideoConversionApp.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{

    [ObservableProperty]
    public partial MediaSelectionViewModel? MediaSelectionViewModel { get; set; }
    [ObservableProperty]
    public partial ConversionPreviewViewModel? ConversionPreviewViewModel { get; set; }
    [ObservableProperty]
    public partial RenderSettingsViewModel? RenderSettingsViewModel { get; set; }
    [ObservableProperty]
    public partial RenderQueueViewModel? RenderQueueViewModel { get; set; }
    [ObservableProperty]
    public partial RenderProcessControlViewModel? RenderProcessControlViewModel { get; set; }
    [ObservableProperty]
    public partial GlobalSettingsViewModel? GlobalSettingsViewModel { get; set; }
    
    public List<string> MisconfigurationMessages { get; set; } = new();
    
    public MainWindowViewModel(IServiceProvider? serviceProvider)
    {
        // Hook to listen to misconfiguration events immediately.
        if (!Design.IsDesignMode)
        {
            var configManager = serviceProvider?.GetRequiredService<IConfigManager>();
            configManager!.MisconfigurationDetected += (sender, s) => { MisconfigurationMessages.Add(s); };
        }

        MediaSelectionViewModel = serviceProvider?.GetRequiredService<MediaSelectionViewModel>();
        ConversionPreviewViewModel = serviceProvider?.GetRequiredService<ConversionPreviewViewModel>();
        RenderSettingsViewModel = serviceProvider?.GetRequiredService<RenderSettingsViewModel>();
        RenderQueueViewModel = serviceProvider?.GetRequiredService<RenderQueueViewModel>();
        RenderProcessControlViewModel = serviceProvider?.GetRequiredService<RenderProcessControlViewModel>();
        GlobalSettingsViewModel = serviceProvider?.GetRequiredService<GlobalSettingsViewModel>();
        
        
        if (Design.IsDesignMode)
        {
            ConversionPreviewViewModel = new ConversionPreviewViewModel(null!, null!, null!, new PreviewVideoPlayerState());
            MediaSelectionViewModel = new MediaSelectionViewModel(null!, null!, 
                null!, null!, null!, new BitmapCache(), ConversionPreviewViewModel);
            RenderSettingsViewModel = new RenderSettingsViewModel(null!, null!, null!, null!);
            RenderQueueViewModel = new RenderQueueViewModel(null!, null!, new BitmapCache(), null!);
            RenderProcessControlViewModel = new RenderProcessControlViewModel(null!, null!);
            GlobalSettingsViewModel = new GlobalSettingsViewModel(null!);

        }
        

    }

}