using GestionOrdinateurs.Shared.Enums;

namespace GestionOrdinateurs.API.Service;

public class JournalService
{
    private readonly object _verrou = new();
    private readonly string _dossierLogs;

    public JournalService(IWebHostEnvironment environnement)
    {
        _dossierLogs = Path.Combine(environnement.ContentRootPath, "Log");

        foreach (var categorie in Enum.GetValues<CategorieJournal>())
        {
            Directory.CreateDirectory(Path.Combine(_dossierLogs, categorie.ToString().ToLower()));
        }
    }

    public void Ecrire(CategorieJournal categorie, string message)
    {
        var dossier = Path.Combine(_dossierLogs, categorie.ToString().ToLower());
        var fichier = Path.Combine(dossier, $"{DateTime.Now:yyyy-MM-dd}.log");

        lock (_verrou)
        {
            using var writer = new StreamWriter(fichier, append: true);
            writer.WriteLine($"{DateTime.Now:dd/MM/yyyy} - {message}");
        }
    }
}