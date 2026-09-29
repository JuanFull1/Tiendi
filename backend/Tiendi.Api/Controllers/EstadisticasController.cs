using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tiendi.Api.Services;

namespace Tiendi.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class EstadisticasController : ControllerBase
{
    private readonly EstadisticasService _estadisticasService;

    public EstadisticasController(EstadisticasService estadisticasService)
    {
        _estadisticasService = estadisticasService;
    }

    [HttpGet("resumen")]
    public async Task<IActionResult> ObtenerResumen()
    {
        var resumen = await _estadisticasService.ObtenerResumenAsync();

        return Ok(resumen);
    }

    [HttpGet("productos-mas-vendidos")]
    public async Task<IActionResult> ObtenerProductosMasVendidos()
    {
        var productos =
            await _estadisticasService.ObtenerProductosMasVendidosAsync();

        return Ok(productos);
    }

    [HttpGet("ventas-por-fecha")]
    public async Task<IActionResult> ObtenerVentasPorFecha()
    {
        var ventas =
            await _estadisticasService.ObtenerVentasPorFechaAsync();

        return Ok(ventas);
    }
}