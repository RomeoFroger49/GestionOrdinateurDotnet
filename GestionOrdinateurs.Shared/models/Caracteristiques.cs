using GestionOrdinateurs.Shared.Enums;

namespace GestionOrdinateurs.Shared.Models;

public class Caracteristiques(int ramGo, int stockageGo, string processeur, string marque)
{
    public int RamGo { get; set; } = ramGo;
    public TypeOrdinateur TypeOrdi { get; set; }
    public int StockageGo { get; set; } = stockageGo;
    public string Processeur { get; init; } = processeur;
    public string Marque { get; init; } = marque;

    public void MettreAJourStockage(int stockageGo)
    {
        StockageGo = stockageGo;
    }

    public void MettreAJourRam(int ramGo)
    {
        RamGo = ramGo;
    }
}