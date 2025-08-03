using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Media.Imaging;
using VideoConversionApp.Abstractions;
using VideoConversionApp.Utils;

namespace VideoConversionApp.Services;

public class BitmapCache : IBitmapCache
{
    private readonly Dictionary<string, Bitmap> _cache = new();
    private readonly Dictionary<string, Dictionary<string, Bitmap>> _groupCache = new();
    
    public Bitmap Add(string key, byte[] fromBytes, bool overwrite = true)
    {
        return AddSingleInternal(key, null, fromBytes, overwrite);
    }

    public Bitmap AddToGroup(string key, string group, byte[] fromBytes, bool overwrite = true)
    {
        return AddToGroupInternal(key, group, null, fromBytes, overwrite);
    }

    public Bitmap Add(string key, Bitmap bitmap, bool overwrite = true)
    {
        return AddSingleInternal(key, bitmap, null, overwrite);
    }

    public Bitmap AddToGroup(string key, string group, Bitmap bitmap, bool overwrite = true)
    {
        return AddToGroupInternal(key, group, bitmap, null, overwrite);
    }
    
    private Bitmap AddSingleInternal(string key, Bitmap? bitmap, byte[]? fromBytes, bool overwrite)
    {
        lock (_cache)
        {
            if (!overwrite && _cache.TryGetValue(key, out var existing))
                return existing;

            bitmap = bitmap ?? (Bitmap)fromBytes!.ToBitmap();

            _cache.Remove(key);
            _cache.Add(key, bitmap);
            return bitmap;
        }
    }
    
    private Bitmap AddToGroupInternal(string key, string group, Bitmap? bitmap, byte[]? fromBytes, bool overwrite)
    {
        lock (_groupCache)
        {
            Dictionary<string, Bitmap>? existingGroup;
            if (!overwrite && _groupCache.TryGetValue(group, out existingGroup))
            {
                if (existingGroup.TryGetValue(key, out var existingBitmap))
                    return existingBitmap;
            }

            bitmap = bitmap ?? (Bitmap)fromBytes!.ToBitmap();

            _groupCache.TryGetValue(group, out existingGroup);
            if (existingGroup == null)
            {
                existingGroup = new();
                _groupCache.Add(group, existingGroup);
            }

            if (!existingGroup.TryAdd(key, bitmap))
                existingGroup[key] = bitmap;

            return bitmap;
        }
    }


    public Bitmap? Get(string key)
    {
        _cache.TryGetValue(key, out var bitmap);
        return bitmap;
    }

    public IReadOnlyDictionary<string, Bitmap>? GetGroup(string group)
    {
        _groupCache.TryGetValue(group, out var bitmapGroup);
        return bitmapGroup;
    }

    public void Remove(string key)
    {
        _cache.Remove(key);
    }

    public void RemoveFromGroup(string group, string key)
    {
        if (_groupCache.TryGetValue(group, out var existingGroup))
            existingGroup.Remove(key);
    }

    public void RemoveGroup(string group)
    {
        if (_groupCache.TryGetValue(group, out var bitmapGroup))
            bitmapGroup.Clear();
        _groupCache.Remove(group);
    }

    public void Clear()
    {
        _cache.Clear();
        _groupCache.Keys.ToList().ForEach(x =>
        {
            _groupCache[x].Clear();
            _groupCache.Remove(x);
        });
    }
}