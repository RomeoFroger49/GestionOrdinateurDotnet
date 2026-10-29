using GestionOrdinateurs.Shared.Models;
using Microsoft.EntityFrameworkCore;
using GestionOrdinateurs.API.Context;
using GestionOrdinateurs.API.Event;
using GestionOrdinateurs.Shared.Enums;

namespace GestionOrdinateurs.API.Service;

public class OrdinateurService(ParcInformatiqueContext context): IOrdinateurService
{

    private readonly DbSet<Ordinateur> _ordinateurs = context.Ordinateurs;
    public event EventHandler<OrdinateurAjouteEventArgs>? OrdinateurAjoute;
    public event EventHandler<OrdinateurSupprimeEventArgs>? OrdinateurSupprime;
    public event EventHandler<OrdinateurMisAJourEventArgs>? OrdinateurMisAJour;


    public bool Add(Ordinateur ordinateur)
    {
        if (ordinateur is null) return false;

        _ordinateurs.Add(ordinateur);
        context.SaveChanges();
        
        OrdinateurAjoute?.Invoke(this, new OrdinateurAjouteEventArgs(ordinateur));
        return true;
    }

    public Ordinateur? GetById(int id)
    {
        return _ordinateurs.Find(id);
    }

    public bool Supprimer(int id)
    {
        var ordinateur = _ordinateurs.Find(id);
        if (ordinateur is null) return false;

        _ordinateurs.Remove(ordinateur);
        context.SaveChanges();
        OrdinateurSupprime?.Invoke(this, new OrdinateurSupprimeEventArgs(ordinateur));
        return true;
    }

    public Ordinateur? Update(int id, Ordinateur ordinateurUpdate)
    {
        var ordinateur = _ordinateurs.Find(id);
        if (ordinateur is null) return null;

        ordinateur.Renommer(ordinateurUpdate.Nom);
        ordinateur.Caracteristiques.MettreAJourStockage(ordinateurUpdate.Caracteristiques.StockageGo);
        ordinateur.Caracteristiques.MettreAJourRam(ordinateurUpdate.Caracteristiques.RamGo);
        ordinateur.MettreAJourStock(ordinateurUpdate.Stock);

        context.SaveChanges();
        OrdinateurMisAJour?.Invoke(this, new OrdinateurMisAJourEventArgs(ordinateur));
        return ordinateur;
    }

    public async Task<List<Ordinateur>> GetAll()
    {
        return [.. _ordinateurs];
    }

    public List<Ordinateur> GetByMarque(string marque)
    {
        if (marque == null)
        {
            return [];
        }

        return [.. _ordinateurs.Where(o => o.Caracteristiques.Marque == marque)];
    }

    public async Task<List<Ordinateur>> GetAvailableOrdinateur()
    {
        var ordinateurs =
            await context.Ordinateurs.Where(o => o.Stock > 0).ToListAsync();

        return ordinateurs;
    }

    public async Task<string> Diagnostiquer(int id)
    {
        var ordinateur = await _ordinateurs.FindAsync(id);

        return ordinateur switch
        {
            {
                EtatOrdinateur: EtatOrdinateur.HorsService
            }
                => "Remplacement",

            {
                EtatOrdinateur: EtatOrdinateur.Lent,
                Caracteristiques.RamGo:
            < 8
            }
                => "Ajouter RAM",

            _
                => "Aucune action"
        };
    }

    public async Task<(int TotalOrdinateur, int OrdiDisponibles, decimal ValeurStock, int NbReparations)> CalculerStatistiques()
    {
        var totalOrdinateur = await _ordinateurs.CountAsync();
        var ordiDisponibles = await _ordinateurs.CountAsync(o => o.Stock > 0);
        var valeurStock = await _ordinateurs.SumAsync(o => o.Prix * o.Stock);
        var nbReparations = await context.Reparations.CountAsync();

        return (totalOrdinateur, ordiDisponibles, valeurStock, nbReparations);
    }
}