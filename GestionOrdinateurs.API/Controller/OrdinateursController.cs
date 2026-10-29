using GestionOrdinateurs.Shared.Models;
using GestionOrdinateurs.API.Service;
using Microsoft.AspNetCore.Mvc;
using GestionOrdinateurs.API.Event;
using GestionOrdinateurs.Shared.Enums;
using Microsoft.AspNetCore.Authorization;

namespace GestionOrdinateurs.API.Controller;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdinateursController : ControllerBase, IDisposable
{
    private readonly IOrdinateurService _ordinateurService;
    private readonly ILogger<OrdinateursController> _logger;
    public readonly IConfiguration _configuration;
    private readonly JournalService _journal;


    public OrdinateursController(IOrdinateurService ordinateurService, ILogger<OrdinateursController> logger, IConfiguration configuration, JournalService journal)
    {
        _ordinateurService = ordinateurService;
        _logger = logger;
        _configuration = configuration;
        _journal = journal;

        _ordinateurService.OrdinateurAjoute += OnOrdinateurAjoute;
        _ordinateurService.OrdinateurSupprime += OnOrdinateurSupprime;
        _ordinateurService.OrdinateurMisAJour += OnOrdinateurMisAJour;
    }

    /*
    si un contrôleur implémente IDisposable, alors ASP.NET ppelle automatiquement 
    sa méthode Dispose() à la fin de chaque requête (info trouvé via Claude).
    */
    public void Dispose()
    {
        _ordinateurService.OrdinateurAjoute -= OnOrdinateurAjoute;
        _ordinateurService.OrdinateurSupprime -= OnOrdinateurSupprime;
        _ordinateurService.OrdinateurMisAJour -= OnOrdinateurMisAJour;
    }

    /*---------GET----------*/
    [HttpGet("ordinateurs/{id}")]
    public IActionResult GetOrdinateurById(int id)
    {
        var ordinateur = _ordinateurService.GetById(id);
        if (ordinateur is null)
        {
            _logger.LogInformation("null");
            return NotFound();
        }
        _logger.LogInformation(ordinateur.ToString());
        return Ok(ordinateur);
    }

    [HttpGet("ordinateurs")]
    public async Task<IActionResult> GetAllOrdinateur()
    {
        var ordinateurs = await _ordinateurService.GetAll();
        ordinateurs.ForEach((ordinateur) => _logger.LogInformation(ordinateur.ToString()));
        return Ok(ordinateurs);
    }

    [HttpGet("ordinateurs/marque/{marque}")]
    public IActionResult GetOrdinateurByMarque(string marque)
    {
        var ordinateurs = _ordinateurService.GetByMarque(marque);
        ordinateurs.ForEach((ordinateur) => _logger.LogInformation(ordinateur.ToString()));
        return Ok(ordinateurs);
    }

    [HttpGet("ordinateurs/info")]
    public IActionResult GetInfo()
    {
        return Ok(new
        {
            Nom = _configuration["Application:Name"],
            Version = _configuration["Application:Version"]
        });
    }

    [HttpGet("ordinateurs/available")]
    public async Task<IActionResult> GetAvailableOrdinateur()
    {
        var ordinateurs = await _ordinateurService.GetAvailableOrdinateur();
        ordinateurs.ForEach((ordinateur) => _logger.LogInformation(ordinateur.ToString()));
        return Ok(ordinateurs);
    }

    [HttpGet("ordinateurs/{id}/diagnostic")]
    public async Task<IActionResult> GetOrdiDiagnosticById(int id)
    {
        var resp = await _ordinateurService.Diagnostiquer(id);
        _logger.LogInformation($"l'ordinateur {id} est {resp}");
        return Ok(resp);
    }

    [HttpGet("ordinateurs/statistiques")]
    public async Task<IActionResult> GetStatistiques()
    {
        var (total, enStock, valeurTotal, enReparation) = await _ordinateurService.CalculerStatistiques();
        _logger.LogInformation($"ordis total = {total} | ordis dispo = {enStock}  | valeur total = {valeurTotal} | ordis en réparation = {enReparation}");
        return Ok($"ordis total = {total} | ordis dispo = {enStock} | valeur total = {valeurTotal} | ordis en réparation = {enReparation}");

    }

    [HttpGet("inventaire/{nom}")]
    public async Task<IActionResult> ChercherParNom(string nom)
    {
        var inventaire = new InventaireOrdinateurs
        {
            Ordinateurs = await _ordinateurService.GetAll()
        };

        var ordinateur = inventaire[nom];
        return ordinateur is null ? NotFound() : Ok(ordinateur);
    }

    [HttpGet("inventaire/id/{id:int}")]
    public async Task<IActionResult> ChercherParId(int id)
    {
        var inventaire = new InventaireOrdinateurs
        {
            Ordinateurs = await _ordinateurService.GetAll()
        };

        var ordinateur = inventaire[id];
        return ordinateur is null ? NotFound() : Ok(ordinateur);
    }


    /*---------POST----------*/
    [HttpPost("ordinateurs")]
    public IActionResult PostOrdinateur(Ordinateur ordinateur)
    {
        var resp = _ordinateurService.Add(ordinateur);
        return resp ? Created() : BadRequest();
    }
    /*---------PUT----------*/
    [HttpPut("ordinateurs/{id}")]
    public IActionResult PutOrdinateur(int id, Ordinateur ordinateur)
    {
        var ordinateurPut = _ordinateurService.Update(id, ordinateur);
        return ordinateurPut == null ? NotFound() : Ok(ordinateurPut);
    }
    /*---------DELETE----------*/
    [HttpDelete("ordinateurs/{id}")]
    [Authorize(Roles = "Admin")]
    public IActionResult DeleteOrdinateur(int id)
    {
        return _ordinateurService.Supprimer(id) ? NoContent() : NotFound();

    }

    /*---------EventAction----------*/
    private void OnOrdinateurAjoute(object? sender, OrdinateurAjouteEventArgs e)
    {
        _logger.LogInformation($"Evenement : Ordinateur ajouté -> {e.Ordinateur.Nom}");
        _journal.Ecrire(CategorieJournal.Ordinateurs, $"{e.Ordinateur.Nom} ajouté");
    }

    private void OnOrdinateurSupprime(object? sender, OrdinateurSupprimeEventArgs e)
    {
        _logger.LogInformation($"Evenement : Ordinateur supprimé -> {e.Ordinateur.Nom}");
        _journal.Ecrire(CategorieJournal.Ordinateurs, $"{e.Ordinateur.Nom} supprimé");
    }

    private void OnOrdinateurMisAJour(object? sender, OrdinateurMisAJourEventArgs e)
    {
        _logger.LogInformation($"Evenement : Ordinateur mis à jour -> {e.Ordinateur.Nom}");
        _journal.Ecrire(CategorieJournal.Ordinateurs, $"{e.Ordinateur.Nom} modifié");
    }

}