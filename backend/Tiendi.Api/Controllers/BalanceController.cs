using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tiendi.Api.DTOs;
using Tiendi.Api.Services;

namespace Tiendi.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class BalanceController : ControllerBase
{
    private readonly BalanceService _balanceService;

    public BalanceController(BalanceService balanceService)
    {
        _balanceService = balanceService;
    }

    // GET /api/balance/resumen?desde=2026-09-01&hasta=2026-09-30
    [HttpGet("resumen")]
    public async Task<IActionResult> Resumen(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta
    )
    {
        try
        {
            BalanceResumenDto resumen =
                await _balanceService.ObtenerResumenAsync(desde, hasta);

            return Ok(resumen);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    // GET /api/balance/gastos?desde=2026-09-01&hasta=2026-09-30
    [HttpGet("gastos")]
    public async Task<IActionResult> ListarGastos(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta
    )
    {
        try
        {
            List<GastoDto> gastos =
                await _balanceService.ListarGastosAsync(desde, hasta);

            return Ok(gastos);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    // POST /api/balance/gastos
    [HttpPost("gastos")]
    public async Task<IActionResult> RegistrarGasto(
        [FromBody] CrearGastoDto dto
    )
    {
        string? idTexto =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(idTexto, out int idUsuario))
        {
            return Unauthorized(new
            {
                mensaje = "No se pudo identificar al usuario."
            });
        }

        try
        {
            GastoDto gasto =
                await _balanceService.RegistrarGastoAsync(dto, idUsuario);

            return StatusCode(StatusCodes.Status201Created, gasto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    // DELETE /api/balance/gastos/5  (anula el gasto, no lo borra)
    [HttpDelete("gastos/{id:long}")]
    public async Task<IActionResult> AnularGasto(long id)
    {
        bool anulado =
            await _balanceService.AnularGastoAsync(id);

        if (!anulado)
        {
            return NotFound(new
            {
                mensaje = "El gasto no existe o ya fue anulado."
            });
        }

        return NoContent();
    }
}
