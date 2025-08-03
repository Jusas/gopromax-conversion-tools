using System;

namespace VideoConversionApp.Models;

public struct TimelineCrop : IEquatable<TimelineCrop>
{
    public decimal? StartTimeSeconds { get; set; }
    public decimal? EndTimeSeconds { get; set; }

    public override int GetHashCode() => (StartTimeSeconds, EndTimeSeconds).GetHashCode();
    public static bool operator ==(TimelineCrop lhs, TimelineCrop rhs) => lhs.Equals(rhs);
    public static bool operator !=(TimelineCrop lhs, TimelineCrop rhs) => !(lhs == rhs);
    
    public bool Equals(TimelineCrop other)
    {
        return StartTimeSeconds == other.StartTimeSeconds && EndTimeSeconds == other.EndTimeSeconds;
    }

    public override bool Equals(object? obj)
    {
        return obj is TimelineCrop other && Equals(other);
    }

}