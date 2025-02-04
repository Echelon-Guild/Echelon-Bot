using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Web;

namespace EchelonBot.Services.WoW
{
    public class WoWApiService
    {
        private readonly HttpClient _httpClient;
        private readonly BattleNetAuthService _battleNetAuthService;

        public WoWApiService(HttpClient httpClient, BattleNetAuthService battleNetAuthService)
        {
            _httpClient = httpClient;
            _battleNetAuthService = battleNetAuthService;
        }

        public async Task<T> Get<T>(string endpoint)
        {
            var token = await _battleNetAuthService.GetAccessTokenAsync();

            string uriString = $"https://us.api.blizzard.com/{endpoint}";

            Uri uri = new(uriString);

            var request = new HttpRequestMessage(HttpMethod.Get, uri);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<T>();
        }
    }
}
