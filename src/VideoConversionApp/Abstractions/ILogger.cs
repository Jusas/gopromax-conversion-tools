namespace VideoConversionApp.Abstractions;

public interface ILogger
{
    void LogError(string? message);
    void LogWarning(string? message);
    void LogInformation(string? message);
    void LogVerbose(string? message);
    void Flush();
}