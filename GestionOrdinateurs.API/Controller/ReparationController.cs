using GestionOrdinateurs.API.Service;
using GestionOrdinateurs.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace GestionOrdinateurs.API.Controller;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReparationController : ControllerBase
{
    private readonly IReparationService _reparationService;
    private readonly IOrdinateurService _ordinateurService;
    private readonly StockService _stockService;
    private readonly ILogger<ReparationController> _logger;

    public ReparationController(IReparationService reparationService, IOrdinateurService ordinateurService, StockService stockService, ILogger<ReparationController> logger)
    {
        _reparationService = reparationService;
        _ordinateurService = ordinateurService;
        _logger = logger;
        _stockService = stockService;
    }

    [HttpPost("SimulerReparation/")]
    public async Task<IActionResult> SimulerReparation()
    {
        var ordinateurs = await _ordinateurService.GetAll();
        await Task.WhenAll(
            _reparationService.SimulerReparationAsync(ordinateurs[0]),
            _reparationService.SimulerReparationAsync(ordinateurs[1]),
            _reparationService.SimulerReparationAsync(ordinateurs[2])
        );

        return Ok("Réparations terminées");
    }

    [HttpGet("GetStockSSD")]
    [Authorize(Roles = "Admin")]
    public IActionResult GetStockSSD()
    {
        Parallel.For(0, 10, i => _stockService.ConsommerSSD());

        _logger.LogInformation($"Stock SSD : {_stockService.StockSSD}");
        return Ok(_stockService.StockSSD);
    }

    [HttpGet("historique")]
    public async Task<IActionResult> GetHistorique()
    {
        return Ok(await _reparationService.GetHistoriqueAsync());
    }

}