using System.Text.Json.Serialization;

namespace Husqvarna2Mqtt;

public class MowerMetadata
{
    [JsonPropertyName("connected")]
    public bool Connected { get; set; }

    [JsonPropertyName("statusTimestamp")]
    public long StatusTimestamp { get; set; }
}
