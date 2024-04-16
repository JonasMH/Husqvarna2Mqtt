using System.Text.Json.Serialization;

namespace Husqvarna2Mqtt;

public class MowerWorkArea
{
    [JsonPropertyName("workAreaId")]
    public int WorkAreaId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("cuttingHeight")]
    public int CuttingHeight { get; set; }
}
