using System.Net.Http.Headers;
using Blazored.SessionStorage;

namespace GestionOrdinateurs.Client.Services;

public class JwtHandler(ISessionStorageService sessionStorage) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await sessionStorage.GetItemAsync<string>("token");

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}