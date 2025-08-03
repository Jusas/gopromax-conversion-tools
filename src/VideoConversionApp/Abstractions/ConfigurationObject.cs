using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace VideoConversionApp.Abstractions;

public abstract class ConfigurationObject<T> : ISerializableConfiguration<T> where T:class
{
    private static readonly JsonSerializerOptions SerializerOptions = new ()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public abstract string GetConfigurationKey();

    public JsonObject? SerializeConfiguration()
    {
        return JsonSerializer.SerializeToNode(this, GetType(), SerializerOptions) as JsonObject;
    }
    
    protected abstract void InitializeFrom(T? configuration);

    public void DeserializeConfiguration(JsonObject jsonObject)
    {
        var config = jsonObject.Deserialize<T>(SerializerOptions);
        InitializeFrom(config);
    }
}