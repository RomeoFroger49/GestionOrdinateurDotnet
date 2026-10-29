using GestionOrdinateurs.API.Service;
using Microsoft.AspNetCore.Mvc;

namespace GestionOrdinateurs.API.Controller;

[ApiController]
[Route("api/[controller]")]
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await userService.GetAllAsync());
    }

    [HttpGet("{login}")]
    public async Task<IActionResult> GetByLogin(string login)
    {
        var user = await userService.GetByLoginAsync(login);
        return user is null ? NotFound() : Ok(user);
    }
}