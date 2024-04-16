using System.Text.Json.Serialization;

namespace Husqvarna2Mqtt;

public class Mower
{
    [JsonPropertyName("system")]
    public MowerSystem System { get; set; }

    [JsonPropertyName("battery")]
    public MowerBattery Battery { get; set; }

    [JsonPropertyName("capabilities")]
    public MowerCapabilities Capabilities { get; set; }

    [JsonPropertyName("mower")]
    public MowerInfo MowerInfo { get; set; }

    [JsonPropertyName("calendar")]
    public MowerCalendar Calendar { get; set; }

    [JsonPropertyName("planner")]
    public MowerPlanner Planner { get; set; }

    [JsonPropertyName("metadata")]
    public MowerMetadata Metadata { get; set; }

    [JsonPropertyName("positions")]
    public List<MowerPosition> Positions { get; set; }

    [JsonPropertyName("settings")]
    public MowerSettings Settings { get; set; }

    [JsonPropertyName("statistics")]
    public MowerStatistics Statistics { get; set; }

    [JsonPropertyName("stayOutZones")]
    public MowerStayOutZones StayOutZones { get; set; }

    [JsonPropertyName("workAreas")]
    public List<MowerWorkArea> WorkAreas { get; set; }
}
