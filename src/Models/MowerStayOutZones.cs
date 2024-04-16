using System.Text.Json.Serialization;

namespace Husqvarna2Mqtt;

public class MowerStayOutZones
{
    [JsonPropertyName("dirty")]
    public bool Dirty { get; set; }

    [JsonPropertyName("zones")]
    public List<MowerZone> Zones { get; set; }
}
