using System.Text.Json.Serialization;

namespace Husqvarna2Mqtt;

public class MowerPosition
{
    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }
}
