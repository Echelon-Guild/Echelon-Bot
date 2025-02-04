using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace EchelonBot.Services.WoW
{
    public class BattleNetAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly string _clientId = Environment.GetEnvironmentVariable("BATTLENET_CLIENT_ID");
        private readonly string _clientSecret = Environment.GetEnvironmentVariable("BATTLENET_CLIENT_SECRET");
        private string? _accessToken;
        private DateTime _tokenExpiration;

        public BattleNetAuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetAccessTokenAsync()
        {
            if (!string.IsNullOrEmpty(_accessToken) && _tokenExpiration > DateTime.UtcNow)
            {
                return _accessToken; // Return cached token if still valid
            }

            var request = new HttpRequestMessage(HttpMethod.Post, "https://oauth.battle.net/token");
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{_clientId}:{_clientSecret}")));

            request.Content = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("grant_type", "client_credentials")
        });

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode(); // Throws if request fails

            var content = await response.Content.ReadFromJsonAsync<BlizzardTokenResponse>();
            _accessToken = content.AccessToken;
            _tokenExpiration = DateTime.UtcNow.AddSeconds(content.ExpiresIn - 30); // Buffer before expiry

            return _accessToken;
        }
    }
}

