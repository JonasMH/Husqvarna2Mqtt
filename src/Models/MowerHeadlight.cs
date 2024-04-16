using System.Text.Json.Serialization;

namespace Husqvarna2Mqtt;

public class MowerHeadlight
{
    [JsonPropertyName("mode")]
    public string Mode { get; set; }
}
