using GestionOrdinateurs.Shared.Enums;

namespace GestionOrdinateurs.Shared.Models;

public class Ordinateur
{
    public int Id { get; init; }
    public string Nom { get; private set; } = "";
    public Caracteristiques Caracteristiques { get; set; } = null!;
    public List<Reparation> Reparations { get; set; } = new();
    public int Stock { get; private set; } = 10;
    public int Prix { get; set; } = 500;

    public EtatOrdinateur EtatOrdinateur { get; set; }
    private Ordinateur() { }

    public string Devise { get; set; } = "EUR";
    public bool Vendu { get; set; }

    public Ordinateur(string nom, string marque, string processeur, int ramGo, int stockageGo)
    {
        Nom = nom;
        Caracteristiques = new Caracteristiques(ramGo, stockageGo, processeur, marque);
        EtatOrdinateur = EtatOrdinateur.EnService;
    }

    public bool Disponible()
    {
        return Stock > 0;
    }

    public void Renommer(string nom)
    {
        Nom = nom;
    }

    public void MettreAJourStock(int stock)
    {
        Stock = stock;
    }

    public override string ToString()
    {
        return $"[{Id}] {Nom} - {Caracteristiques.Marque} | {Caracteristiques.Processeur} | {Caracteristiques.RamGo} Go RAM | {Stock} en stock";
    }
}