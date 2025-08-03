using System;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using VideoConversionApp.Abstractions;

namespace VideoConversionApp.Config;

[ObservableObject]
public partial class LoggingConfig : ConfigurationObject<LoggingConfig>
{
    public enum LogLevels
    {
        None = 0,
        Error = 1,
        Warning = 2,
        Info = 3,
        Verbose = 4
    }
    
    public override string GetConfigurationKey() => "logging";

    public static string DefaultLoggingDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".local", "share", "MAXVideoConverter");

    [ObservableProperty]
    public partial LogLevels LogLevel { get; set; }
    [ObservableProperty]
    public partial string LogDirectory { get; set; }
    [ObservableProperty]
    public partial bool ReUseLogFile { get; set; }
    [ObservableProperty]
    public partial bool LogToStdout { get; set; }

    public LoggingConfig()
    {
        LogLevel = LogLevels.Warning;
        LogDirectory = DefaultLoggingDirectory;
        ReUseLogFile = true;
        LogToStdout = false;
    }
    
    protected override void InitializeFrom(LoggingConfig? configuration)
    {
        LogDirectory = configuration?.LogDirectory ?? DefaultLoggingDirectory;
        ReUseLogFile = configuration?.ReUseLogFile ?? true;
        LogLevel = configuration?.LogLevel ?? LogLevels.Warning;
        LogToStdout = configuration?.LogToStdout ?? false;
    }
}