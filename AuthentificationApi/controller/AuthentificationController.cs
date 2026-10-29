using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GestionOrdinateurs.Shared.Enums;
using Microsoft.AspNetCore.Mvc;

namespace AuthentificationApi.Controller;

[ApiController]
[Route("api/[controller]")]
public class AuthentificationController(IConfiguration configuration) : ControllerBase
{
    [HttpPost("login")]
    public IActionResult Login(string login, RoleEnum role)
    {
        var jwtKey = configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key manquante dans appsettings.json");
        var key = Encoding.ASCII.GetBytes(jwtKey);
        var expiration = DateTime.UtcNow.AddMinutes(15);

        return Ok(new
        {
            Token = GenererToken(key, login, role, expiration),
            Expiration = expiration
        });
    }

    private static string GenererToken(byte[] key, string login, RoleEnum role, DateTime expiration)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, login),
            new Claim(ClaimTypes.Role, role.ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiration,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
    }
}