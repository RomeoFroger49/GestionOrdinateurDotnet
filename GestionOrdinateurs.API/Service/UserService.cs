using GestionOrdinateurs.API.Context;
using GestionOrdinateurs.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionOrdinateurs.API.Service;

public class UserService(ParcInformatiqueContext context) : IUserService
{
    public async Task<List<User>> GetAllAsync()
    {
        return await context.Users.ToListAsync();
    }

    public async Task<User?> GetByLoginAsync(string login)
    {
        return await context.Users.FirstOrDefaultAsync(u => u.Login == login);
    }
}