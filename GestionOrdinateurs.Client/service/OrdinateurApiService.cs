using System.Net;
using System.Net.Http.Json;
using GestionOrdinateurs.Client.Models;
using GestionOrdinateurs.Shared.Models;

namespace GestionOrdinateurs.Client.Services;

public class OrdinateurApiService(IHttpClientFactory httpClientFactory)
{
    public Task<(List<OrdinateurListeDto>? Donnees, string? Erreur)> GetOrdinateursAsync()
    {
        return GetListeAsync<OrdinateurListeDto>("api/Ordinateurs/ordinateurs");
    }

    public Task<(List<HistoriqueReparationRecord>? Donnees, string? Erreur)> GetHistoriqueAsync()
    {
        return GetListeAsync<HistoriqueReparationRecord>("api/Reparation/historique");
    }

    private async Task<(List<T>? Donnees, string? Erreur)> GetListeAsync<T>(string url)
    {
        try
        {
            var client = httpClientFactory.CreateClient("Gateway");
            var reponse = await client.GetAsync(url);

            return reponse.StatusCode switch
            {
                HttpStatusCode.Unauthorized => (null, "Vous devez être connecté, ou votre session a expiré."),
                HttpStatusCode.Forbidden => (null, "Vous n'avez pas les droits pour accéder à ces données."),
                _ when reponse.IsSuccessStatusCode => (await reponse.Content.ReadFromJsonAsync<List<T>>() ?? [], null),
                _ => (null, $"Erreur du serveur ({(int)reponse.StatusCode}).")
            };
        }
        catch (HttpRequestException)
        {
            return (null, "Impossible de contacter le serveur.");
        }
    }
}