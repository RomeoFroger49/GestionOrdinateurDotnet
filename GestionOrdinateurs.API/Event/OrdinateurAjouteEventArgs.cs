using GestionOrdinateurs.Shared.Models;

namespace GestionOrdinateurs.API.Event;

public class OrdinateurAjouteEventArgs(Ordinateur ordinateur): EventArgs
{
    public Ordinateur Ordinateur { get; } = ordinateur;
    public DateTime DateAjout { get; } = DateTime.Now;

}