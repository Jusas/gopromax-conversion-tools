namespace VideoConversionApp.Models;

public struct CodecEntry
{
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsVideo { get; set; }
    public bool IsAudio { get; set; }
    public bool IsContainer { get; set; }
    public bool EncodingSupported { get; set; }
    public bool DecodingSupported { get; set; }
    public bool IsLossy { get; set; }
    public bool IsLossless { get; set; }
}