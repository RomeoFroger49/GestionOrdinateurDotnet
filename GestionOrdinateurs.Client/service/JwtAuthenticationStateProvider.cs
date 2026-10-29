using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
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

        var claims = jwt.Claims.ToList();
        claims.AddRange(ExtraireRoles(claims).Select(role => new Claim("role", role)));

        var identite = new ClaimsIdentity(claims, "jwt", "preferred_username", "role");
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

    private static IEnumerable<string> ExtraireRoles(List<Claim> claims)
    {
        var roles = new List<string>();

        var realmAccess = claims.FirstOrDefault(c => c.Type == "realm_access")?.Value;
        if (!string.IsNullOrEmpty(realmAccess))
        {
            using var doc = JsonDocument.Parse(realmAccess);
            if (doc.RootElement.TryGetProperty("roles", out var realmRoles))
            {
                roles.AddRange(realmRoles.EnumerateArray().Select(r => r.GetString() ?? string.Empty));
            }
        }

        var resourceAccess = claims.FirstOrDefault(c => c.Type == "resource_access")?.Value;
        if (!string.IsNullOrEmpty(resourceAccess))
        {
            using var doc = JsonDocument.Parse(resourceAccess);
            foreach (var client in doc.RootElement.EnumerateObject())
            {
                if (client.Value.TryGetProperty("roles", out var clientRoles))
                {
                    roles.AddRange(clientRoles.EnumerateArray().Select(r => r.GetString() ?? string.Empty));
                }
            }
        }

        return roles
            .Where(r => !string.IsNullOrEmpty(r))
            .Where(r => !claims.Any(c => c.Type == "role" && c.Value == r))
            .Distinct();
    }
}