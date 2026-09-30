using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tiendi.Api.DTOs;
using Tiendi.Api.Services;

namespace Tiendi.Api.Controllers;

[ApiController]
[Route("api/inventario")]
[Authorize]
public class InventarioController : ControllerBase
{
    private readonly InventarioService _inventarioService;

    public InventarioController(
        InventarioService inventarioService)
    {
        _inventarioService =
            inventarioService;
    }

    [HttpGet("categorias")]
    public async Task<IActionResult>
        ObtenerCategorias()
    {
        var categorias =
            await _inventarioService
                .ObtenerCategoriasAsync();

        return Ok(categorias);
    }

    [HttpGet("stock-bajo")]
    public async Task<IActionResult>
        ObtenerStockBajo()
    {
        var productos =
            await _inventarioService
                .ObtenerStockBajoAsync();

        return Ok(productos);
    }

    [HttpGet("movimientos")]
    public async Task<IActionResult>
        ObtenerMovimientos(
            [FromQuery] int? idProducto)
    {
        var movimientos =
            await _inventarioService
                .ObtenerMovimientosAsync(
                    idProducto
                );

        return Ok(movimientos);
    }

    [HttpPost("ajuste")]
    public async Task<IActionResult>
        AjustarStock(
            [FromBody] AjusteStockDto dto)
    {
        try
        {
            int idUsuario =
                ObtenerIdUsuario();

            var movimiento =
                await _inventarioService
                    .AjustarStockAsync(
                        dto,
                        idUsuario
                    );

            return Ok(new
            {
                mensaje =
                    "Stock actualizado correctamente.",
                movimiento
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                mensaje =
                    ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                mensaje =
                    ex.Message
            });
        }
    }

    private int ObtenerIdUsuario()
    {
        string? valor =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

        if (!int.TryParse(
            valor,
            out int idUsuario))
        {
            throw new UnauthorizedAccessException(
                "No se pudo identificar al usuario."
            );
        }

        return idUsuario;
    }
}