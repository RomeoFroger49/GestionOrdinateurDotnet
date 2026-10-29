using GestionOrdinateurs.Shared.Models;

namespace GestionOrdinateurs.API.Service;

public interface IReparationService
{
    public Task SimulerReparationAsync(Ordinateur ordinateur);
    public Task<List<HistoriqueReparationRecord>> GetHistoriqueAsync();
}