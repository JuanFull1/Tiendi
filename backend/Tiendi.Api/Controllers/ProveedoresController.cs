using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tiendi.Api.DTOs;
using Tiendi.Api.Services;

namespace Tiendi.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProveedoresController : ControllerBase
{
    private readonly ProveedoresService _service;

    public ProveedoresController(ProveedoresService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos()
    {
        var proveedores = await _service.ObtenerTodos();

        return Ok(proveedores);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var proveedor = await _service.ObtenerPorId(id);

        if (proveedor == null)
            return NotFound();

        return Ok(proveedor);
    }

    [HttpPost]
    public async Task<IActionResult> Crear(
        CrearProveedorDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return BadRequest("El nombre es obligatorio.");

        var proveedor = await _service.Crear(dto);

        return CreatedAtAction(
            nameof(ObtenerPorId),
            new { id = proveedor.IdProveedor },
            proveedor
        );
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(
        int id,
        ActualizarProveedorDto dto)
    {
        var actualizado =
            await _service.Actualizar(id, dto);

        if (!actualizado)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Desactivar(int id)
    {
        var desactivado =
            await _service.Desactivar(id);

        if (!desactivado)
            return NotFound();

        return NoContent();
    }
}