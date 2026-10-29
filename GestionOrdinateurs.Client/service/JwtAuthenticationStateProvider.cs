using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace GestionOrdinateurs.Client.Services;

public class JwtAuthenticationStateProvider(ISessionStorageService sessionStorage) : AuthenticationStateProvider
{
    private static readonly AuthenticationState Anonyme = new(new ClaimsPrincipal(new ClaimsIdentity()));

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await sessionStorage.GetItemAsync<string>("token");

        if (string.IsNullOrWhiteSpace(token))
            return Anonyme;

        JwtSecurityToken jwt;
        try
        {
            jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        }
        catch (ArgumentException)
        {
            await sessionStorage.RemoveItemAsync("token");
            return Anonyme;
        }

        if (jwt.ValidTo < DateTime.UtcNow)
        {
            await sessionStorage.RemoveItemAsync("token");
            return Anonyme;
        }

        var identite = new ClaimsIdentity(jwt.Claims, "jwt", "unique_name", "role");
        return new AuthenticationState(new ClaimsPrincipal(identite));
    }

    public void NotifierConnexion()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public void NotifierDeconnexion()
    {
        NotifyAuthenticationStateChanged(Task.FromResult(Anonyme));
    }
}