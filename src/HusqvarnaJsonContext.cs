using System.Text.Json.Serialization;

namespace Husqvarna2Mqtt;

[JsonSerializable(typeof(HusqvarnaDataEntity<Mower>))]
[JsonSerializable(typeof(HusqvarnaDataResponse<List<HusqvarnaDataEntity<Mower>>>))]
[JsonSerializable(typeof(GetTokenResponse))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true
    )]
public partial class HusqvarnaJsonContext : JsonSerializerContext
{

}

