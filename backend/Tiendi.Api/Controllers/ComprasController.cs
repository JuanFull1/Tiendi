using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tiendi.Api.DTOs;
using Tiendi.Api.Services;

namespace Tiendi.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ComprasController : ControllerBase
{
    private readonly ComprasService _service;

    public ComprasController(ComprasService service)
    {
        _service = service;
    }

    // =========================================================
    // GET /api/compras
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> ObtenerTodas()
    {
        var compras =
            await _service.ObtenerTodas();

        return Ok(compras);
    }

    // =========================================================
    // POST /api/compras
    // =========================================================

    [HttpPost]
    public async Task<IActionResult> Crear(
        [FromBody] CrearCompraDto dto)
    {
        try
        {
            // ---------------------------------------------
            // OBTENER USUARIO DESDE EL TOKEN JWT
            // ---------------------------------------------

            string? idUsuarioTexto =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                );

            if (!int.TryParse(
                    idUsuarioTexto,
                    out int idUsuario))
            {
                return Unauthorized(new
                {
                    mensaje =
                        "No se pudo identificar al usuario autenticado."
                });
            }

            // ---------------------------------------------
            // CREAR COMPRA
            // ---------------------------------------------

            var resultado =
                await _service.Crear(
                    dto,
                    idUsuario
                );

            return Ok(resultado);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                mensaje = ex.Message
            });
        }
        catch (Exception)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    mensaje =
                        "Ocurrió un error al registrar la compra."
                }
            );
        }
    }
}