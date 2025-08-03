using System;
using System.Diagnostics.CodeAnalysis;

namespace VideoConversionApp.Models;

/// <summary>
/// Frame rotation parameters for our complex AV filter. 
/// </summary>
public struct AvFilterFrameRotation : IEquatable<AvFilterFrameRotation>
{
    public int Yaw { get; set; }
    public int Pitch { get; set; }
    public int Roll { get; set; }

    public static AvFilterFrameRotation Zero => new AvFilterFrameRotation();

    public override int GetHashCode() => (Yaw, Pitch, Roll).GetHashCode();
    public static bool operator ==(AvFilterFrameRotation lhs, AvFilterFrameRotation rhs) => lhs.Equals(rhs);
    public static bool operator !=(AvFilterFrameRotation lhs, AvFilterFrameRotation rhs) => !(lhs == rhs);
    
    public override bool Equals(object? obj)
    {
        return obj is AvFilterFrameRotation other && Equals(other);
    }
    
    public bool Equals(AvFilterFrameRotation other)
    {
        return Yaw == other.Yaw && Pitch == other.Pitch && Roll == other.Roll;
    }

    public override string ToString()
    {
        return $"Yaw: {Yaw} | Pitch: {Pitch} | Roll: {Roll}";
    }
}