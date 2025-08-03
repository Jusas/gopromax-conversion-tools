using System.Collections.Generic;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace VideoConversionApp.Utils;

public static class AvaloniaHelpers
{
    public static T? FindControlByName<T>(IEnumerable<Visual> parent, string name) where T : Visual
    {
        foreach (Visual? child in parent)
        {
            if (child.GetType() == typeof(T) && ((StyledElement)child).Name == name)
                return (T)child;

            // Panel is the base class that contains the `Children` property 
            if (typeof(Panel).GetTypeInfo().IsAssignableFrom(child.GetType()))
            {
                // recursion :(
                var x = FindControlByName<T>(((Panel)child).GetVisualChildren(), name);
                if (x is not null)
                    return x;
            }
        }
        return null;
    }
}