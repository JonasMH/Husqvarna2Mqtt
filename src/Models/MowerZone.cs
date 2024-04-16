using System.Text.Json.Serialization;

namespace Husqvarna2Mqtt;

public class MowerZone
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }
}
