using System;
using System.Diagnostics;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Themes.Simple;
using Microsoft.Extensions.DependencyInjection;
using VideoConversionApp.Abstractions;
using VideoConversionApp.Services;
using VideoConversionApp.ViewModels;

namespace VideoConversionApp;

public partial class App : Application
{
    private static string _configFilePath = null!;
    public static ILogger? Logger = null;

    public static string ConfigFilePath
    {
        get
        {
            if (string.IsNullOrEmpty(_configFilePath))
            {
                // Get user home directory/.config
                var userHomeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                var configDirectory = Path.Combine(userHomeDirectory, ".config", "MAXVideoConverter");
                _configFilePath = Path.Combine(configDirectory, "config.json");
            }

            return _configFilePath;
        }
    }
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        var t = typeof(SimpleTheme); // AOT stripping
        t.ToString();
    }

    
    
    public override void OnFrameworkInitializationCompleted()
    {
        // If you use CommunityToolkit, line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);

        // Register all the services needed for the application to run
        var collection = new ServiceCollection();
        collection.AddCommonServices();
        
        // Creates a ServiceProvider containing services from the provided IServiceCollection
        var services = collection.BuildServiceProvider();
        
        // Load settings at the beginning.
        var configManager = services.GetRequiredService<IConfigManager>();
        if(!configManager.LoadConfigurations(ConfigFilePath))
            configManager.SaveConfigurations(ConfigFilePath);

        Logger = services.GetRequiredService<ILogger>();
        Logger.LogInformation("Application starting");
        var vm = services.GetRequiredService<MainWindowViewModel>();
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new Views.MainWindow
            {
                DataContext = vm
            };
            ((StorageServiceProvider)services.GetRequiredService<IStorageServiceProvider>())
                .UseProviderWindow(desktop.MainWindow);

            desktop.MainWindow.Closing += (sender, args) =>
            {
                configManager.SaveConfigurations(ConfigFilePath);
            };
        }
        
        base.OnFrameworkInitializationCompleted();
    }

}