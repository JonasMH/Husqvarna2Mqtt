using System.Text.Json.Serialization;
using Husqvarna2Mqtt.Models;

namespace Husqvarna2Mqtt;

[JsonSerializable(typeof(JsonApiDataListDocument))]
[JsonSerializable(typeof(JsonApiDataDocumentCommandResult))]
[JsonSerializable(typeof(ActionRequestBody<Start>))]
[JsonSerializable(typeof(ActionRequestBody<StartInWorkArea>))]
[JsonSerializable(typeof(ActionRequestBody<ResumeSchedule>))]
[JsonSerializable(typeof(ActionRequestBody<Pause>))]
[JsonSerializable(typeof(ActionRequestBody<Park>))]
[JsonSerializable(typeof(ActionRequestBody<ParkUntilNextSchedule>))]
[JsonSerializable(typeof(ActionRequestBody<ParkUntilFurtherNotice>))]
[JsonSerializable(typeof(GetTokenResponse))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true
    )]
public partial class HusqvarnaJsonContext : JsonSerializerContext
{

}

