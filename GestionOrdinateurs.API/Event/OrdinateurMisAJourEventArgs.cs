using GestionOrdinateurs.Shared.Models;

namespace GestionOrdinateurs.API.Event;

public class OrdinateurSupprimeEventArgs(Ordinateur ordinateur)
{
    public Ordinateur Ordinateur { get; } = ordinateur;
    public DateTime DateAjout { get; } = DateTime.Now;

}