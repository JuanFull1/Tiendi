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

    public EstadisticasController(
        EstadisticasService estadisticasService
    )
    {
        _estadisticasService = estadisticasService;
    }


    [HttpGet("resumen")]
    public async Task<IActionResult> ObtenerResumen()
    {
        var resumen =
            await _estadisticasService.ObtenerResumenAsync();

        return Ok(resumen);
    }


    [HttpGet("productos-mas-vendidos")]
    public async Task<IActionResult>
        ObtenerProductosMasVendidos(
            [FromQuery] int top = 10
        )
    {
        if (top < 1 || top > 100)
        {
            return BadRequest(new
            {
                mensaje =
                    "El parámetro top debe estar entre 1 y 100."
            });
        }

        var productos =
            await _estadisticasService
                .ObtenerProductosMasVendidosAsync(top);

        return Ok(productos);
    }


    [HttpGet("ventas-por-fecha")]
    public async Task<IActionResult>
        ObtenerVentasPorFecha(
            [FromQuery] DateTime? desde,
            [FromQuery] DateTime? hasta
        )
    {
        if (
            desde.HasValue &&
            hasta.HasValue &&
            desde.Value.Date > hasta.Value.Date
        )
        {
            return BadRequest(new
            {
                mensaje =
                    "La fecha desde no puede ser mayor que la fecha hasta."
            });
        }

        var ventas =
            await _estadisticasService
                .ObtenerVentasPorFechaAsync(
                    desde,
                    hasta
                );

        return Ok(ventas);
    }
}