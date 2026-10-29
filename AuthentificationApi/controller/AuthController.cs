using AuthentificationApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuthentificationApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly KeycloakAuthService _keycloakAuthService;

        public AuthController(KeycloakAuthService keycloakAuthService)
        {
            _keycloakAuthService = keycloakAuthService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var token = await _keycloakAuthService.LoginAsync(request.Username, request.Password);
                return Ok(token);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, $"Keycloak injoignable : {ex.Message}");
            }
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}