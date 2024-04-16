using System.Text.Json.Serialization;

namespace Husqvarna2Mqtt;

public class MowerCalendar
{
    [JsonPropertyName("tasks")]
    public List<MowerTask> Tasks { get; set; }
}
