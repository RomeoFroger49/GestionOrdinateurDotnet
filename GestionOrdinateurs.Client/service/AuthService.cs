using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Blazored.SessionStorage;

namespace GestionOrdinateurs.Client.Services;

public class KeycloakTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;
}

public class AuthService(IHttpClientFactory httpClientFactory, ISessionStorageService sessionStorage, JwtAuthenticationStateProvider authProvider)
{
    public async Task<string?> ConnecterAsync(string login, string motDePasse)
    {
        if (string.IsNullOrWhiteSpace(login))
            return "Veuillez saisir un utilisateur.";

        if (string.IsNullOrWhiteSpace(motDePasse))
            return "Veuillez saisir un mot de passe.";

        try
        {
            var apiAuth = httpClientFactory.CreateClient("ApiAuth");
            var reponse = await apiAuth.PostAsJsonAsync("api/Auth/login", new
            {
                username = login,
                password = motDePasse
            });

            if (reponse.StatusCode == HttpStatusCode.Unauthorized)
                return "Identifiant ou mot de passe incorrect.";

            if (reponse.StatusCode == HttpStatusCode.ServiceUnavailable)
                return "Le serveur d'authentification (Keycloak) est injoignable.";

            reponse.EnsureSuccessStatusCode();

            var resultat = await reponse.Content.ReadFromJsonAsync<KeycloakTokenResponse>();

            if (string.IsNullOrWhiteSpace(resultat?.AccessToken))
                return "Réponse invalide du serveur d'authentification.";

            await sessionStorage.SetItemAsync("token", resultat.AccessToken);
            authProvider.NotifierConnexion();

            return null;
        }
        catch (HttpRequestException)
        {
            return "Impossible de contacter le serveur. Vérifie que les API sont lancées.";
        }
    }

    public async Task DeconnecterAsync()
    {
        await sessionStorage.RemoveItemAsync("token");
        authProvider.NotifierDeconnexion();
    }
}