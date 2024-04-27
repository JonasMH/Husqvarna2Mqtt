using Husqvarna2Mqtt.Models;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;

namespace Husqvarna2Mqtt;

public class HusqvarnaAutomoverClient(HttpClient httpClient, IOptions<HusqvarnaAutomoverClientOptions> options)
{
    private JwtSecurityToken? _token;
    private JwtSecurityTokenHandler _handler = new();

    private async Task AppendAuthAsync(HttpRequestMessage message)
    {
        var accessToken = (await GetTokenAsync()).RawData;

        message.Headers.Add("Authorization", $"Bearer {accessToken}");
        message.Headers.Add("Authorization-Provider", "husqvarna");
        message.Headers.Add("X-Api-Key", options.Value.ClientId);
    }

    public async Task<JwtSecurityToken> GetTokenAsync()
    {
        if (_token == null || _token.ValidTo < (DateTime.UtcNow - TimeSpan.FromMinutes(5)))
        {
            _token = await GetNewTokenAsync();
        }

        return _token;
    }

    private async Task<JwtSecurityToken> GetNewTokenAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.authentication.husqvarnagroup.dev/v1/oauth2/token");
        request.Content = new FormUrlEncodedContent([
            new KeyValuePair<string,string>("grant_type", "client_credentials" ),
            new KeyValuePair<string,string>("client_id", options.Value.ClientId ),
            new KeyValuePair<string,string>("client_secret", options.Value.ClientSecret ),
        ]);

        var response = await httpClient.SendAsync(request);

        response.EnsureSuccessStatusCode();

        var tokenResponse = (await response.Content.ReadFromJsonAsync(HusqvarnaJsonContext.Default.GetTokenResponse))!;

        return _handler.ReadJwtToken(tokenResponse.AccessToken);
    }

    public async Task<ICollection<Mower>> GetMowersAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.amc.husqvarna.dev/v1/mowers");

        await AppendAuthAsync(request);


        var response = await httpClient.SendAsync(request);


        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync(HusqvarnaJsonContext.Default.JsonApiDataListDocument))!.Data;
    }

    public async Task<JsonApiDataDocumentCommandResult> ActionAsync<T>(Guid mowerId, T action) where T : JsonApiAction
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"https://api.amc.husqvarna.dev/v1/mowers/{mowerId}/actions");
        request.Content = JsonContent.Create(new ActionRequestBody<T> { Data = action }, HusqvarnaJsonContext.Default.GetTypeInfo(typeof(ActionRequestBody<T>))!, new MediaTypeHeaderValue("application/vnd.api+json"));

        await AppendAuthAsync(request);

        var response = await httpClient.SendAsync(request);

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync(HusqvarnaJsonContext.Default.JsonApiDataDocumentCommandResult))!;
    }
}

public class ActionRequestBody<T> where T : JsonApiAction
{
    [JsonPropertyName("data")]
    public T Data { get; set; }
}
