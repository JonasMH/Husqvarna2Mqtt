using System.Text.Json.Serialization;

namespace Husqvarna2Mqtt;

public class MowerStatistics
{
    [JsonPropertyName("cuttingBladeUsageTime")]
    public int CuttingBladeUsageTime { get; set; }

    [JsonPropertyName("numberOfChargingCycles")]
    public int NumberOfChargingCycles { get; set; }

    [JsonPropertyName("numberOfCollisions")]
    public int NumberOfCollisions { get; set; }

    [JsonPropertyName("totalChargingTime")]
    public int TotalChargingTime { get; set; }

    [JsonPropertyName("totalCuttingTime")]
    public int TotalCuttingTime { get; set; }

    [JsonPropertyName("totalDrivenDistance")]
    public int TotalDrivenDistance { get; set; }

    [JsonPropertyName("totalRunningTime")]
    public int TotalRunningTime { get; set; }

    [JsonPropertyName("totalSearchingTime")]
    public int TotalSearchingTime { get; set; }
}
