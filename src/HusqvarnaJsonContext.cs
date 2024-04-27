using System.Text.Json.Serialization;
using Husqvarna2Mqtt.Models;

namespace Husqvarna2Mqtt;

[JsonSerializable(typeof(JsonApiDataListDocument))]
[JsonSerializable(typeof(JsonApiDataDocumentCommandResult))]
[JsonSerializable(typeof(Start))]
[JsonSerializable(typeof(StartInWorkArea))]
[JsonSerializable(typeof(ResumeSchedule))]
[JsonSerializable(typeof(Pause))]
[JsonSerializable(typeof(Park))]
[JsonSerializable(typeof(ParkUntilNextSchedule))]
[JsonSerializable(typeof(ParkUntilFurtherNotice))]
[JsonSerializable(typeof(GetTokenResponse))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true
    )]
public partial class HusqvarnaJsonContext : JsonSerializerContext
{

}

