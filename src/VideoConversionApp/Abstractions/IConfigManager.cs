using System;

namespace VideoConversionApp.Abstractions;

public interface IConfigManager
{
    event EventHandler? NewConfigLoaded;
    
    bool LoadConfigurations(string filename);
    void SaveConfigurations(string filename);
    T? GetConfig<T>() where T: ISerializableConfiguration;
}
