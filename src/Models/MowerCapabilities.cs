using System.Text.Json.Serialization;

namespace Husqvarna2Mqtt;

public class MowerCapabilities
{
    [JsonPropertyName("position")]
    public bool Position { get; set; }

    [JsonPropertyName("headlights")]
    public bool Headlights { get; set; }

    [JsonPropertyName("workAreas")]
    public bool WorkAreas { get; set; }

    [JsonPropertyName("stayOutZones")]
    public bool StayOutZones { get; set; }
}
