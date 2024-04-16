using System.Text.Json.Serialization;

namespace Husqvarna2Mqtt;

public class MowerPlanner
{
    [JsonPropertyName("nextStartTimestamp")]
    public long NextStartTimestamp { get; set; }

    [JsonPropertyName("override")]
    public MowerOverride Override { get; set; }

    [JsonPropertyName("restrictedReason")]
    public string RestrictedReason { get; set; }

    [JsonPropertyName("externalReason")]
    public int ExternalReason { get; set; }
}
