using System.Text.Json.Serialization;

namespace EchelonBot.Services.WoW
{
    public class BlizzardTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }
}
