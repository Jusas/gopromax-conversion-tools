using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using VideoConversionApp.Abstractions;

namespace VideoConversionApp.Services;

public class ConfigManager : IConfigManager
{
    private readonly Type[] _types;
    private readonly Dictionary<Type, ISerializableConfiguration> _configs = new();
    
    public ConfigManager()
    {
        _types = GetConfigTypes();
    }

    private Type[] GetConfigTypes()
    {
        var configurableType = typeof(ISerializableConfiguration);
        return GetType().Assembly.GetTypes()
            .Where(t => t.IsAssignableTo(configurableType) && !t.IsAbstract)
            .ToArray();
    }

    public event EventHandler? NewConfigLoaded;

    public bool LoadConfigurations(string filename)
    {
        if (_configs.Count > 0)
            _configs.Clear();
        
        if (!File.Exists(filename))
        {
            LoadDefaults();
            NewConfigLoaded?.Invoke(this, EventArgs.Empty);
            return false;
        }
        
        var fileContent = File.ReadAllText(filename);
        var configJsonNode = JsonNode.Parse(fileContent);
    
        foreach (var configurableType in _types)
        {
            var configurable = (ISerializableConfiguration)Activator.CreateInstance(configurableType)!;
            if (configJsonNode[configurable.GetConfigurationKey()] != null)
                configurable.DeserializeConfiguration(configJsonNode[configurable.GetConfigurationKey()].AsObject());
            
            _configs.Add(configurableType, configurable);
        }

        NewConfigLoaded?.Invoke(this, EventArgs.Empty);
        return true;
    }

    private void LoadDefaults()
    {
        foreach (var configurableType in _types)
        {
            var configurable = (ISerializableConfiguration)Activator.CreateInstance(configurableType)!;
            _configs.Add(configurableType, configurable);
        } 
    }

    public void SaveConfigurations(string filename)
    {
        var configJsonNode = new JsonObject();
        if (_configs.Count != 0)
        {
            foreach (var configurable in _configs.Values)
            {
                configJsonNode.Add(configurable.GetConfigurationKey(), configurable.SerializeConfiguration());
            }
        }
        else
        {
            foreach (var configurableType in _types)
            {
                var configurable = (ISerializableConfiguration)Activator.CreateInstance(configurableType)!;
                _configs.Add(configurableType, configurable);
                configJsonNode.Add(configurable.GetConfigurationKey(), configurable.SerializeConfiguration());
            } 
        }
        
        var json = JsonSerializer.Serialize(configJsonNode, 
            options: new JsonSerializerOptions() { WriteIndented = true });
        Directory.CreateDirectory(Path.GetDirectoryName(filename)!);
        File.WriteAllText(filename, json);
    }

    public T? GetConfig<T>() where T: ISerializableConfiguration
    {
        if (_configs.TryGetValue(typeof(T), out var config))
            return (T)config;
        return default;
    }
}