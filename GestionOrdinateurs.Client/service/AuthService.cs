using System.Net;
using System.Net.Http.Json;
using Blazored.SessionStorage;
using GestionOrdinateurs.Shared.Models;

namespace GestionOrdinateurs.Client.Services;

public record TokenResponse(string Token, DateTime Expiration);

public class AuthService(IHttpClientFactory httpClientFactory, ISessionStorageService sessionStorage, JwtAuthenticationStateProvider authProvider)
{
    public async Task<string?> ConnecterAsync(string login)
    {
        if (string.IsNullOrWhiteSpace(login))
            return "Veuillez saisir un utilisateur.";

        try
        {
            var apiOrdinateurs = httpClientFactory.CreateClient("ApiOrdinateurs");
            var reponseUser = await apiOrdinateurs.GetAsync($"api/Users/{Uri.EscapeDataString(login)}");

            if (reponseUser.StatusCode == HttpStatusCode.NotFound)
                return $"L'utilisateur « {login} » n'existe pas.";

            reponseUser.EnsureSuccessStatusCode();
            var user = await reponseUser.Content.ReadFromJsonAsync<User>();

            var apiAuth = httpClientFactory.CreateClient("ApiAuth");
            var reponseToken = await apiAuth.PostAsync(
                $"api/Authentification/login?login={Uri.EscapeDataString(user!.Login)}&role={user.Role}", null);

            reponseToken.EnsureSuccessStatusCode();
            var resultat = await reponseToken.Content.ReadFromJsonAsync<TokenResponse>();

            await sessionStorage.SetItemAsync("token", resultat!.Token);
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