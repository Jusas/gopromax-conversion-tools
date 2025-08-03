using System;

namespace VideoConversionApp.Abstractions;

public interface IInputVideoInfo
{
    public string Filename { get; }
    public bool IsValidVideo { get; }
    public bool IsGoProMaxFormat { get; }
    public decimal DurationInSeconds { get; }
    public DateTime CreatedDateTime { get; }
    public long SizeBytes { get; }
    public string[]? ValidationIssues { get; }
    
    public static readonly string PlaceHolderFilename = ":placeholder:";
    public static bool IsPlaceholderFile(string filename) => filename == PlaceHolderFilename;
}