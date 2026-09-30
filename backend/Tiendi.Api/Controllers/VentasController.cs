using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tiendi.Api.DTOs;
using Tiendi.Api.Services;

namespace Tiendi.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class VentasController : ControllerBase
{
    private readonly VentasService _ventasService;

    public VentasController(VentasService ventasService)
    {
        _ventasService = ventasService;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodas()
    {
        List<VentaDto> ventas = await _ventasService.ObtenerTodasAsync();

        return Ok(ventas);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> ObtenerPorId(long id)
    {
        VentaDto? venta = await _ventasService.ObtenerPorIdAsync(id);

        if (venta is null)
        {
            return NotFound(new
            {
                mensaje = $"No existe la venta con Id {id}."
            });
        }

        return Ok(venta);
    }

    [HttpPost]
    public async Task<IActionResult> Crear(
        [FromBody] CrearVentaDto dto
    )
    {
        string? idUsuarioTexto =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(idUsuarioTexto, out int idUsuario))
        {
            return Unauthorized(new
            {
                mensaje = "No se pudo identificar al usuario autenticado."
            });
        }

        try
        {
            VentaDto creada = await _ventasService.CrearAsync(dto, idUsuario);

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { id = creada.IdVenta },
                creada
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    mensaje = "Ocurrió un error al registrar la venta."
                }
            );
        }
    }
}