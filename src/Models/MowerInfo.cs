using System.Text.Json.Serialization;

namespace Husqvarna2Mqtt;

public class MowerInfo
{
    [JsonPropertyName("mode")]
    public string Mode { get; set; }

    [JsonPropertyName("activity")]
    public string Activity { get; set; }

    [JsonPropertyName("state")]
    public string State { get; set; }

    [JsonPropertyName("errorCode")]
    public int ErrorCode { get; set; }

    [JsonPropertyName("errorCodeTimestamp")]
    public long ErrorCodeTimestamp { get; set; }
}
