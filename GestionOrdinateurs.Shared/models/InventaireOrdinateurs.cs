namespace GestionOrdinateurs.Shared.Models;

public class InventaireOrdinateurs
{
    public List<Ordinateur> Ordinateurs { get; set; } = new();

    public Ordinateur? this[int ordinateurId]
    {
        get => Ordinateurs.FirstOrDefault(o => o.Id == ordinateurId);
    }

    public Ordinateur? this[string nom]
    {
        get => Ordinateurs.FirstOrDefault(o => o.Nom.Equals(nom, StringComparison.OrdinalIgnoreCase));
    }
}