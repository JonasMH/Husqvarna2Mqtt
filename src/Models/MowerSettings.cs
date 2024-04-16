using System.Text.Json.Serialization;

namespace Husqvarna2Mqtt;

public class MowerSettings
{
    [JsonPropertyName("cuttingHeight")]
    public int CuttingHeight { get; set; }

    [JsonPropertyName("headlight")]
    public MowerHeadlight Headlight { get; set; }
}
