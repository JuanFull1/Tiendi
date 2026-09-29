using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tiendi.Api.DTOs;
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
        _productosService =
            productosService;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos()
    {
        var productos =
            await _productosService
                .ObtenerTodosAsync();

        return Ok(productos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerPorId(
        int id)
    {
        var producto =
            await _productosService
                .ObtenerPorIdAsync(id);

        if (producto == null)
        {
            return NotFound(new
            {
                mensaje =
                    "El producto no existe."
            });
        }

        return Ok(producto);
    }

    [HttpPost]
    public async Task<IActionResult> Crear(
        [FromBody] CrearProductoDto dto)
    {
        try
        {
            int idUsuario =
                ObtenerIdUsuario();

            var producto =
                await _productosService
                    .CrearAsync(
                        dto,
                        idUsuario
                    );

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new
                {
                    id =
                        producto.IdProducto
                },
                producto
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                mensaje = ex.Message
            });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(
        int id,
        [FromBody] ActualizarProductoDto dto)
    {
        try
        {
            var producto =
                await _productosService
                    .ActualizarAsync(
                        id,
                        dto
                    );

            return Ok(producto);
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

    [HttpDelete("{id}")]
    public async Task<IActionResult> Desactivar(
        int id)
    {
        bool resultado =
            await _productosService
                .DesactivarAsync(id);

        if (!resultado)
        {
            return NotFound(new
            {
                mensaje =
                    "El producto no existe."
            });
        }

        return Ok(new
        {
            mensaje =
                "Producto desactivado correctamente."
        });
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