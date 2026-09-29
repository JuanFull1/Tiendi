using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tiendi.Api.DTOs;
using Tiendi.Api.Services;

namespace Tiendi.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly ClientesService _clientesService;

    public ClientesController(ClientesService clientesService)
    {
        _clientesService = clientesService;
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        List<ClienteDto> clientes =
            await _clientesService.ListarActivosAsync();

        return Ok(clientes);
    }

    [HttpGet("buscar")]
    public async Task<IActionResult> Buscar(
        [FromQuery] string? termino
    )
    {
        List<ClienteDto> clientes =
            await _clientesService.BuscarAsync(termino);

        return Ok(clientes);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        ClienteDto? cliente =
            await _clientesService.ObtenerPorIdAsync(id);

        if (cliente is null)
        {
            return NotFound(new
            {
                mensaje = $"No existe el cliente con Id {id}."
            });
        }

        return Ok(cliente);
    }

    [HttpPost]
    public async Task<IActionResult> Crear(
        [FromBody] CrearClienteDto dto
    )
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            ClienteDto creado =
                await _clientesService.CrearAsync(dto);

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { id = creado.IdCliente },
                creado
            );
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(
        int id,
        [FromBody] ActualizarClienteDto dto
    )
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            ClienteDto? actualizado =
                await _clientesService.ActualizarAsync(id, dto);

            if (actualizado is null)
            {
                return NotFound(new
                {
                    mensaje = $"No existe el cliente con Id {id}."
                });
            }

            return Ok(actualizado);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Desactivar(int id)
    {
        bool ok = await _clientesService.DesactivarAsync(id);

        if (!ok)
        {
            return NotFound(new
            {
                mensaje = $"No existe el cliente con Id {id}."
            });
        }

        return NoContent();
    }
}