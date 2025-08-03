using System;

namespace VideoConversionApp.Abstractions;

public interface IConfigManager
{
    event EventHandler<string>? NewConfigLoaded;
    event EventHandler<string>? MisconfigurationDetected;
    
    public string LastLoadedConfigFilePath { get; }
    
    bool LoadConfigurations(string filename);
    void SaveConfigurations(string filename);
    void NotifyMisconfigurationDetected(string reason);
    T? GetConfig<T>() where T: ISerializableConfiguration;
}
