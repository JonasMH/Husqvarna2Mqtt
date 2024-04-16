using System.Text.Json.Serialization;

namespace Husqvarna2Mqtt;

public class MowerTask
{
    [JsonPropertyName("start")]
    public int Start { get; set; }

    [JsonPropertyName("duration")]
    public int Duration { get; set; }

    [JsonPropertyName("monday")]
    public bool Monday { get; set; }

    [JsonPropertyName("tuesday")]
    public bool Tuesday { get; set; }

    [JsonPropertyName("wednesday")]
    public bool Wednesday { get; set; }

    [JsonPropertyName("thursday")]
    public bool Thursday { get; set; }

    [JsonPropertyName("friday")]
    public bool Friday { get; set; }

    [JsonPropertyName("saturday")]
    public bool Saturday { get; set; }

    [JsonPropertyName("sunday")]
    public bool Sunday { get; set; }
}
