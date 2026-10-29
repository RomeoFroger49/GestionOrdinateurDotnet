using FluentAssertions;
using GestionOrdinateurs.API.Context;
using GestionOrdinateurs.API.Service;
using GestionOrdinateurs.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GestionOrdinateurs.Tests;

public class OrdinateurServiceTest
{
    private ParcInformatiqueContext CreerContexte()
    {
        var options = new DbContextOptionsBuilder<ParcInformatiqueContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ParcInformatiqueContext(options);
    }

    [Fact]
    public void Add_OrdinateurValide_EstAjouteEnBase()
    {
        using var context = CreerContexte();
        var service = new OrdinateurService(context);
        var ordinateur = new Ordinateur("PC-Test", "HP", "Intel i5", 8, 512);

        var resultat = service.Add(ordinateur);

        resultat.Should().BeTrue();
        service.GetById(ordinateur.Id).Should().NotBeNull();
    }

    [Fact]
    public void Add_OrdinateurNull_RetourneFalse()
    {
        using var context = CreerContexte();
        var service = new OrdinateurService(context);

        var resultat = service.Add(null!);

        resultat.Should().BeFalse();
    }

    [Fact]
    public void Supprimer_OrdinateurExistant_RetourneTrueEtLeRetire()
    {
        using var context = CreerContexte();
        var service = new OrdinateurService(context);
        var ordinateur = new Ordinateur("PC-ASupprimer", "Lenovo", "Ryzen 5", 16, 512);
        service.Add(ordinateur);

        var resultat = service.Supprimer(ordinateur.Id);

        resultat.Should().BeTrue();
        service.GetById(ordinateur.Id).Should().BeNull();
    }

    [Fact]
    public void Supprimer_OrdinateurInexistant_RetourneFalse()
    {
        using var context = CreerContexte();
        var service = new OrdinateurService(context);

        var resultat = service.Supprimer(999);

        resultat.Should().BeFalse();
    }
}