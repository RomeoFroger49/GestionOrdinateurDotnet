using GestionOrdinateurs.API.Event;
using GestionOrdinateurs.Shared.Models;

namespace GestionOrdinateurs.API.Service;

public interface IOrdinateurService
{
    public bool Add(Ordinateur ordinateur);

    public Ordinateur? GetById(int id);

    public bool Supprimer(int id);

    public Ordinateur? Update(int id, Ordinateur ordinateurUpdate);

    public Task<List<Ordinateur>> GetAll();

    public List<Ordinateur> GetByMarque(string marque);

    public Task<List<Ordinateur>> GetAvailableOrdinateur();
    public Task<string> Diagnostiquer(int id);
    public Task<(int TotalOrdinateur, int OrdiDisponibles, decimal ValeurStock, int NbReparations)> CalculerStatistiques();

    public event EventHandler<OrdinateurAjouteEventArgs>? OrdinateurAjoute;

    public event EventHandler<OrdinateurSupprimeEventArgs>? OrdinateurSupprime;

    public event EventHandler<OrdinateurMisAJourEventArgs>? OrdinateurMisAJour;

}