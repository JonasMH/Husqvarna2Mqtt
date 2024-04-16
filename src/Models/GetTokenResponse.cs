using System.Text.Json.Serialization;

namespace Husqvarna2Mqtt;

public class GetTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }
}
