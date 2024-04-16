using System.Text.Json.Serialization;

namespace Husqvarna2Mqtt;

public class MowerSystem
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("model")]
    public string Model { get; set; }

    [JsonPropertyName("serialNumber")]
    public int SerialNumber { get; set; }
}
