using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tiendi.Api.Services;

namespace Tiendi.Api.Controllers;

[ApiController]
[Route("api/productos")]
[Authorize]
public class ProductosController : ControllerBase
{
    private readonly ProductosService _productosService;

    public ProductosController(
        ProductosService productosService)
    {
        _productosService = productosService;
    }

    [HttpPost("{id}/imagen")]
    public async Task<IActionResult> SubirImagen(
        int id,
        [FromForm] IFormFile imagen)
    {
        try
        {
            string imagenUrl =
                await _productosService
                    .GuardarImagenAsync(
                        id,
                        imagen
                    );

            return Ok(new
            {
                mensaje =
                    "Imagen guardada correctamente.",
                imagenUrl
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                mensaje = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                mensaje = ex.Message
            });
        }
    }

    [HttpDelete("{id}/imagen")]
    public async Task<IActionResult> EliminarImagen(
        int id)
    {
        try
        {
            bool eliminada =
                await _productosService
                    .EliminarImagenAsync(id);

            if (!eliminada)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El producto no tiene imagen."
                });
            }

            return Ok(new
            {
                mensaje =
                    "Imagen eliminada correctamente."
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                mensaje = ex.Message
            });
        }
    }
}