using System.Collections.Generic;
using Avalonia.Media.Imaging;

namespace VideoConversionApp.Abstractions;

/// <summary>
/// For caching and referencing bitmaps (pretty much thumbnails) we use across several views.
/// </summary>
public interface IBitmapCache
{
    /// <summary>
    /// Add a single ungrouped Bitmap to cache.
    /// The bitmap is created from the input bytes.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="fromBytes"></param>
    /// <param name="overwrite"></param>
    /// <returns></returns>
    Bitmap Add(string key, byte[] fromBytes, bool overwrite = true);
    
    /// <summary>
    /// Add a single Bitmap to a group of cached images.
    /// The bitmap is created from the input bytes.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="group"></param>
    /// <param name="fromBytes"></param>
    /// <param name="overwrite"></param>
    /// <returns></returns>
    Bitmap AddToGroup(string key, string group, byte[] fromBytes, bool overwrite = true);
    
    /// <summary>
    /// Add a single ungrouped Bitmap to cache.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="bitmap"></param>
    /// <param name="overwrite"></param>
    /// <returns></returns>
    Bitmap Add(string key, Bitmap bitmap, bool overwrite = true);
    
    /// <summary>
    /// Add a single Bitmap to a group of cached images.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="group"></param>
    /// <param name="bitmap"></param>
    /// <param name="overwrite"></param>
    /// <returns></returns>
    Bitmap AddToGroup(string key, string group, Bitmap bitmap, bool overwrite = true);
    
    /// <summary>
    /// Get a single cached Bitmap (from ungrouped cached bitmaps).
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    Bitmap? Get(string key);
    
    /// <summary>
    /// Returns a cached bitmap group.
    /// </summary>
    /// <param name="group"></param>
    /// <returns></returns>
    IReadOnlyDictionary<string, Bitmap>? GetGroup(string group);
    
    /// <summary>
    /// Removes a single ungrouped cached bitmap from the cache.
    /// </summary>
    /// <param name="key"></param>
    void Remove(string key);
    
    /// <summary>
    /// Removes a group of cached bitmaps from the cache.
    /// </summary>
    /// <param name="group"></param>
    void RemoveGroup(string group);
    
    /// <summary>
    /// Removes a single bitmap from a cached group of bitmaps.
    /// </summary>
    /// <param name="group"></param>
    /// <param name="key"></param>
    void RemoveFromGroup(string group, string key);
    
    /// <summary>
    /// Clears the entire bitmap cache.
    /// </summary>
    void Clear();
}