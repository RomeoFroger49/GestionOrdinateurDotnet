using FluentAssertions;
using GestionOrdinateurs.API.Context;
using GestionOrdinateurs.API.Service;
using GestionOrdinateurs.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GestionOrdinateurs.Tests;

public class ReparationServiceTest
{
    private readonly string _dossierTest = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

    private ParcInformatiqueContext CreerContexte()
    {
        var options = new DbContextOptionsBuilder<ParcInformatiqueContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ParcInformatiqueContext(options);
    }

    private JournalService CreerJournal()
    {
        return new JournalService(new EnvironnementTest { ContentRootPath = _dossierTest });
    }

    [Fact]
    public async Task SimulerReparation_OrdinateurExistant_CreeUneReparationTerminee()
    {
        using var context = CreerContexte();
        var ordinateur = new Ordinateur("PC-Test", "HP", "Intel i5", 8, 512);
        context.Ordinateurs.Add(ordinateur);
        await context.SaveChangesAsync();
        var service = new ReparationService(context, CreerJournal());

        await service.SimulerReparationAsync(ordinateur);

        var reparation = await context.Reparations.SingleAsync();
        reparation.OrdinateurId.Should().Be(ordinateur.Id);
        reparation.TypeReparation.Should().Be("Écran cassé");
        reparation.Statut.Should().Be("Terminée");
    }

    [Fact]
    public async Task SimulerReparation_OrdinateurExistant_EcritDansLeJournal()
    {
        using var context = CreerContexte();
        var ordinateur = new Ordinateur("PC-Journal", "DELL", "Ryzen 7", 16, 1024);
        context.Ordinateurs.Add(ordinateur);
        await context.SaveChangesAsync();
        var service = new ReparationService(context, CreerJournal());

        await service.SimulerReparationAsync(ordinateur);

        var fichier = Path.Combine(_dossierTest, "Log", "reparations", $"{DateTime.Now:yyyy-MM-dd}.log");
        File.Exists(fichier).Should().BeTrue();
        (await File.ReadAllTextAsync(fichier)).Should().Contain("PC-Journal réparé");
    }
}