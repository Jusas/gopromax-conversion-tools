using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace VideoConversionApp.Abstractions;

public interface ISerializableConfiguration
{
    string GetConfigurationKey();
    JsonObject? SerializeConfiguration();
    void DeserializeConfiguration(JsonObject jsonObject);
}

public interface ISerializableConfiguration<T> : ISerializableConfiguration where T:class
{

}