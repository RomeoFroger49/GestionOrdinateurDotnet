using System.Text.Json;
using System.Text.Json.Serialization;

namespace AuthentificationApi.Services
{
    public class KeycloakAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public KeycloakAuthService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<TokenResponse> LoginAsync(string username, string password)
        {
            var baseUrl = _configuration["Keycloak:BaseUrl"];
            var realm = _configuration["Keycloak:Realm"];
            var tokenEndpoint = $"{baseUrl}/realms/{realm}/protocol/openid-connect/token";

            var requestBody = new Dictionary<string, string>
            {
                { "grant_type", "password" },
                { "client_id", _configuration["Keycloak:ClientId"]! },
                { "client_secret", _configuration["Keycloak:ClientSecret"]! },
                { "username", username },
                { "password", password }
            };

            var response = await _httpClient.PostAsync(tokenEndpoint, new FormUrlEncodedContent(requestBody));

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new UnauthorizedAccessException($"Échec de l'authentification Keycloak : {error}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TokenResponse>(responseContent)!;
        }
    }

    public class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}