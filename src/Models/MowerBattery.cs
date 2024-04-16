using System.Text.Json.Serialization;

namespace Husqvarna2Mqtt;

public class MowerBattery
{
    [JsonPropertyName("batteryPercent")]
    public int BatteryPercent { get; set; }
}
