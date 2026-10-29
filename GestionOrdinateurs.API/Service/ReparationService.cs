using GestionOrdinateurs.API.Context;
using GestionOrdinateurs.Shared.Enums;
using GestionOrdinateurs.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionOrdinateurs.API.Service;

public class ReparationService(ParcInformatiqueContext context, JournalService journal) : IReparationService
{
    private readonly SemaphoreSlim _verrou = new(1, 1);

    public async Task SimulerReparationAsync(Ordinateur ordinateur)
    {
        var reparation = new Reparation(ordinateur.Id, "Écran cassé", DateTime.Now, "En réparation", "Thierry");

        await _verrou.WaitAsync();
        try
        {
            context.Reparations.Add(reparation);
            await context.SaveChangesAsync();
        }
        finally
        {
            _verrou.Release();
        }

        await Task.Delay(2000);

        await _verrou.WaitAsync();
        try
        {
            reparation.Statut = "Terminée";
            await context.SaveChangesAsync();
        }
        finally
        {
            _verrou.Release();
        }

        journal.Ecrire(CategorieJournal.Reparations, $"{ordinateur.Nom} réparé");
    }

    public async Task<List<HistoriqueReparationRecord>> GetHistoriqueAsync()
    {
        return await context.Reparations
            .OrderByDescending(r => r.DateDebut)
            .Select(r => new HistoriqueReparationRecord(
                r.OrdinateurId,
                r.UnOrdinateur!.Nom,
                r.Technicien,
                r.TypeReparation,
                r.DateDebut))
            .ToListAsync();
    }
}