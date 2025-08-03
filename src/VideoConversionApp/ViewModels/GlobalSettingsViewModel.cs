using System.Collections.Generic;
using System.Reflection;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using VideoConversionApp.Abstractions;
using VideoConversionApp.Config;

namespace VideoConversionApp.ViewModels;

public partial class GlobalSettingsViewModel : ViewModelBase
{
    private readonly IConfigManager _configManager;
    
    public string AppVersion => GetType().Assembly.GetName().Version.ToString();
    
    [ObservableProperty]
    public partial string ExiftoolPath { get; set; }
    [ObservableProperty]
    public partial string FfmpegPath { get; set; }
    [ObservableProperty]
    public partial string FfprobePath { get; set; }
    
    [ObservableProperty]
    public partial string LogDirectory { get; set; }
    [ObservableProperty]
    public partial LoggingConfig.LogLevels LogLevel { get; set; }
    [ObservableProperty]
    public partial bool OverwriteLogFile { get; set; }
    [ObservableProperty]
    public partial bool LogToStdout { get; set; }

    [ObservableProperty]
    public partial uint NumberOfSnapshotFrames { get; set; }
    [ObservableProperty]
    public partial uint NumberOfThumbnailThreads { get; set; }
    [ObservableProperty]
    public partial uint ThumbnailTimePosition { get; set; }

    public List<LoggingConfig.LogLevels> AvailableLogLevels => new()
    {
        LoggingConfig.LogLevels.Error,
        LoggingConfig.LogLevels.Warning,
        LoggingConfig.LogLevels.Info,
        LoggingConfig.LogLevels.Verbose
    };

    public GlobalSettingsViewModel(IConfigManager configManager)
    {
        _configManager = configManager;
        if (Design.IsDesignMode)
            return;
        Initialize();
    }

    void Initialize()
    {
        ExiftoolPath = _configManager.GetConfig<PathsConfig>()!.Exiftool;
        FfmpegPath = _configManager.GetConfig<PathsConfig>()!.Ffmpeg;
        FfprobePath = _configManager.GetConfig<PathsConfig>()!.Ffprobe;
        LogDirectory = _configManager.GetConfig<LoggingConfig>()!.LogDirectory;
        LogLevel = _configManager.GetConfig<LoggingConfig>()!.LogLevel;
        OverwriteLogFile = _configManager.GetConfig<LoggingConfig>()!.ReUseLogFile;
        LogToStdout = _configManager.GetConfig<LoggingConfig>()!.LogToStdout;
        NumberOfSnapshotFrames = _configManager.GetConfig<PreviewsConfig>()!.NumberOfSnapshotFrames;
        NumberOfThumbnailThreads = _configManager.GetConfig<PreviewsConfig>()!.NumberOfThumbnailThreads;
        ThumbnailTimePosition = _configManager.GetConfig<PreviewsConfig>()!.ThumbnailTimePositionPcnt;
    }

    partial void OnExiftoolPathChanged(string value) => _configManager.GetConfig<PathsConfig>()!.Exiftool = value;
    partial void OnFfmpegPathChanged(string value) => _configManager.GetConfig<PathsConfig>()!.Ffmpeg = value;
    partial void OnFfprobePathChanged(string value) => _configManager.GetConfig<PathsConfig>()!.Ffprobe = value;
    partial void OnLogDirectoryChanged(string value) => _configManager.GetConfig<LoggingConfig>()!.LogDirectory = value;
    partial void OnLogLevelChanged(LoggingConfig.LogLevels value) => _configManager.GetConfig<LoggingConfig>()!.LogLevel = value;
    partial void OnOverwriteLogFileChanged(bool value) => _configManager.GetConfig<LoggingConfig>()!.ReUseLogFile = value;
    partial void OnLogToStdoutChanged(bool value) => _configManager.GetConfig<LoggingConfig>()!.LogToStdout = value;
    partial void OnNumberOfSnapshotFramesChanged(uint value) => _configManager.GetConfig<PreviewsConfig>()!.NumberOfSnapshotFrames = value;
    partial void OnNumberOfThumbnailThreadsChanged(uint value) => _configManager.GetConfig<PreviewsConfig>()!.NumberOfThumbnailThreads = value;
    partial void OnThumbnailTimePositionChanged(uint value) => _configManager.GetConfig<PreviewsConfig>()!.ThumbnailTimePositionPcnt = value;
    
}