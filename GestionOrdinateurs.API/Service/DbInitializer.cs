using GestionOrdinateurs.API.Context;
using GestionOrdinateurs.Shared.Models;

namespace GestionOrdinateurs.API.Service;

public static class DbInitializer
{
    public static void Initialize(ParcInformatiqueContext context)
    {
        //Ajout des ordinateurs
        context.Ordinateurs.AddRange([
            new Ordinateur("ROGUE","ASUS","Ryzen", 16, 512),
            new Ordinateur("ROGUE I","ASUS","AMD", 16, 256),
            new Ordinateur("MacBook Pro","APPLE","M1", 32, 1024),
            new Ordinateur("ROGUE II","ASUS","Ryzen", 16, 512),
            new Ordinateur("Test","DELL","Ryzen", 16, 128),
        ]);
        context.SaveChanges();
    }
}