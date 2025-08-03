using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using VideoConversionApp.Abstractions;
using VideoConversionApp.Config;

namespace VideoConversionApp.Services;

public class Logger : ILogger
{
    private LoggingConfig _config;
    private ConcurrentQueue<string> _writeQueue = new ();
    private readonly object _mutex = new();
    private readonly int _queueCapacity = 100;
    private readonly string _sessionLogFile;
    private Encoding _encoding;
    private bool _initialized = false;

    public Logger(IConfigManager configManager)
    {
        _config = configManager.GetConfig<LoggingConfig>()!;
        _sessionLogFile = $"log-{DateTime.Now:yyyyMMdd-HHmmss}.txt";
        _encoding = new UTF8Encoding(false);
    }
    
    public void LogError(string? message)
    {
        if (_config.LogLevel < LoggingConfig.LogLevels.Error)
            return;
        
        LogAny("ERROR", message);
    }

    public void LogWarning(string? message)
    {
        if (_config.LogLevel < LoggingConfig.LogLevels.Warning)
            return;
        
        LogAny("WARN", message);
    }

    public void LogInformation(string? message)
    {
        if (_config.LogLevel < LoggingConfig.LogLevels.Info)
            return;
        
        LogAny("INFO", message);
    }

    public void LogVerbose(string? message)
    {
        if (_config.LogLevel < LoggingConfig.LogLevels.Verbose)
            return;
        
        LogAny("VERBOSE", message);
    }

    private void LogAny(string prefix, string? message)
    {
        var time = DateTime.Now.ToString("HH:mm:ss.fff");
        message = $"[{time}] [{prefix}] {message}";
        
        if (_config.LogToStdout)
            Console.WriteLine(message);
        
        _writeQueue.Enqueue(message + Environment.NewLine);
        if (_writeQueue.Count > _queueCapacity)
        {
            lock (_mutex)
            {
                Flush();
            }
        }
        
    }

    public void Flush()
    {
        var logFile = _config.ReUseLogFile ? "log.txt" : _sessionLogFile;
        var logDir = string.IsNullOrEmpty(_config.LogDirectory) 
            ? LoggingConfig.DefaultLoggingDirectory
            : _config.LogDirectory;
        logFile = Path.Combine(logDir, logFile);
        
        if (!Directory.Exists(logDir))
            Directory.CreateDirectory(logDir);
        
        var fileMode = FileMode.Append;
        if (!_initialized && _config.ReUseLogFile)
        {
            fileMode = FileMode.Create;
            _initialized = true;
        }

        using var stream = File.Open(logFile, fileMode, FileAccess.Write, FileShare.Read);
        while (_writeQueue.TryDequeue(out var message))
        {
            var bytes = _encoding.GetBytes(message);
            stream.Write(bytes, 0, bytes.Length);
        }
    }
}