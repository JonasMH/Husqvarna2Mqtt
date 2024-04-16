using System.Text.Json.Serialization;

namespace Husqvarna2Mqtt;

public class MowerOverride
{
    [JsonPropertyName("action")]
    public string Action { get; set; }
}
