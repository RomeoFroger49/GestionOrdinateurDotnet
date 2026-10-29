using GestionOrdinateurs.Shared.Models;

namespace GestionOrdinateurs.API.Service;

public interface IUserService
{
    Task<List<User>> GetAllAsync();
    Task<User?> GetByLoginAsync(string login);
}