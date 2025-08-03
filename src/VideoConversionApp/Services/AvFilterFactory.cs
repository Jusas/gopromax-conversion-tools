using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Avalonia.Platform;
using VideoConversionApp.Abstractions;
using VideoConversionApp.Models;

namespace VideoConversionApp.Services;

/// <summary>
/// <inheritdoc cref="IAvFilterFactory"/>
/// </summary>
public class AvFilterFactory : IAvFilterFactory
{
    private readonly string _avFilterTemplate;

    public AvFilterFactory()
    {
        // using var resourceStream = AssetLoader.Open(
        //     new Uri("avares://VideoConversionApp/Resources/transform-template.avfilter"));
        var assembly = GetType().Assembly;
        var filter = assembly
            .GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith("transform-template.avfilter"));
        var resourceStream = assembly.GetManifestResourceStream(filter!);
        using var reader = new StreamReader(resourceStream!);
        _avFilterTemplate = reader.ReadToEnd();
    }
    
    public string BuildAvFilter(AvFilterFrameSelectCondition? frameSelectCondition = null, 
        AvFilterFrameRotation? frameRotation = null)
    {
        var filter = _avFilterTemplate;
        
        // Frame selection.
        var keyFrameExpression = string.Empty;
        var frameDistanceExpression = string.Empty;
        
        var keyFramesOnly = frameSelectCondition?.KeyFramesOnly ?? false;
        var frameDistance = frameSelectCondition?.FrameDistance ?? 0;
        
        if (keyFramesOnly)
            keyFrameExpression = "eq(pict_type\\,I)";
        if (frameDistance > 0)
        {
            frameDistance = Math.Max(1, Math.Round(frameDistance, 1));
            frameDistanceExpression = $"isnan(prev_selected_t)+gte(t-prev_selected_t\\,{frameDistance})";
        }

        var expressionParts = new List<string>();
        if (frameDistanceExpression != string.Empty)
            expressionParts.Add(frameDistanceExpression);
        if (keyFrameExpression != string.Empty)
            expressionParts.Add(keyFrameExpression);
        
        var frameSelectionExpression = expressionParts.Count > 0 
            ? "select=" + string.Join("*", expressionParts) + "," 
            : string.Empty;
        filter = filter.Replace("{FRAME_SELECT_EXPRESSION}", frameSelectionExpression);
        
        // Rotation.
        var yaw = frameRotation?.Yaw ?? 0;
        var pitch = frameRotation?.Pitch ?? 0;
        var roll = frameRotation?.Roll ?? 0;
        
        filter = filter.Replace("{YAW_VALUE}", yaw.ToString(CultureInfo.InvariantCulture));
        filter = filter.Replace("{PITCH_VALUE}", pitch.ToString(CultureInfo.InvariantCulture));
        filter = filter.Replace("{ROLL_VALUE}", roll.ToString(CultureInfo.InvariantCulture));
        
        return filter;
    }

}