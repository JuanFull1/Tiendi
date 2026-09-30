using Microsoft.EntityFrameworkCore;
using Tiendi.Api.Data;
using Tiendi.Api.DTOs;
using Tiendi.Api.Models;

namespace Tiendi.Api.Services;

public class ClientesService
{
    private readonly TiendiDbContext _context;

    public ClientesService(TiendiDbContext context)
    {
        _context = context;
    }

    public async Task<List<ClienteDto>> ListarActivosAsync()
    {
        return await _context.Clientes
            .Where(c => c.Activo)
            .OrderBy(c => c.Nombre)
            .ThenBy(c => c.Apellido)
            .Select(c => MapearADto(c))
            .ToListAsync();
    }

    public async Task<ClienteDto?> ObtenerPorIdAsync(int id)
    {
        Cliente? cliente = await _context.Clientes
            .FirstOrDefaultAsync(c => c.IdCliente == id);

        return cliente is null
            ? null
            : MapearADto(cliente);
    }

    public async Task<List<ClienteDto>> BuscarAsync(string? termino)
    {
        if (string.IsNullOrWhiteSpace(termino))
        {
            return await ListarActivosAsync();
        }

        string t = termino.Trim().ToLower();

        return await _context.Clientes
            .Where(c =>
                c.Activo &&
                (
                    c.Nombre.ToLower().Contains(t) ||
                    (c.Apellido != null &&
                        c.Apellido.ToLower().Contains(t)) ||
                    (c.Identificacion != null &&
                        c.Identificacion.ToLower().Contains(t)) ||
                    (c.Email != null &&
                        c.Email.ToLower().Contains(t)) ||
                    (c.Telefono != null &&
                        c.Telefono.ToLower().Contains(t))
                )
            )
            .OrderBy(c => c.Nombre)
            .Select(c => MapearADto(c))
            .ToListAsync();
    }

    public async Task<ClienteDto> CrearAsync(CrearClienteDto dto)
    {
        if (!string.IsNullOrWhiteSpace(dto.Identificacion))
        {
            bool existe = await _context.Clientes
                .AnyAsync(c =>
                    c.Identificacion == dto.Identificacion);

            if (existe)
            {
                throw new InvalidOperationException(
                    "Ya existe un cliente con esa identificacion."
                );
            }
        }

        Cliente cliente = new()
        {
            Nombre = dto.Nombre.Trim(),

            Apellido = dto.Apellido?.Trim(),

            Identificacion = dto.Identificacion?.Trim(),

            Telefono = dto.Telefono?.Trim(),

            Email = dto.Email?.Trim(),

            Direccion = dto.Direccion?.Trim(),

            Activo = true,

            FechaCreacion = DateTime.Now
        };

        _context.Clientes.Add(cliente);

        await _context.SaveChangesAsync();

        return MapearADto(cliente);
    }

    public async Task<ClienteDto?> ActualizarAsync(
        int id,
        ActualizarClienteDto dto
    )
    {
        Cliente? cliente = await _context.Clientes
            .FirstOrDefaultAsync(c => c.IdCliente == id);

        if (cliente is null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(dto.Identificacion))
        {
            bool existe = await _context.Clientes
                .AnyAsync(c =>
                    c.Identificacion == dto.Identificacion &&
                    c.IdCliente != id);

            if (existe)
            {
                throw new InvalidOperationException(
                    "Ya existe otro cliente con esa identificacion."
                );
            }
        }

        cliente.Nombre = dto.Nombre.Trim();

        cliente.Apellido = dto.Apellido?.Trim();

        cliente.Identificacion = dto.Identificacion?.Trim();

        cliente.Telefono = dto.Telefono?.Trim();

        cliente.Email = dto.Email?.Trim();

        cliente.Direccion = dto.Direccion?.Trim();

        cliente.Activo = dto.Activo;

        await _context.SaveChangesAsync();

        return MapearADto(cliente);
    }

    public async Task<bool> DesactivarAsync(int id)
    {
        Cliente? cliente = await _context.Clientes
            .FirstOrDefaultAsync(c => c.IdCliente == id);

        if (cliente is null)
        {
            return false;
        }

        cliente.Activo = false;

        await _context.SaveChangesAsync();

        return true;
    }

    private static ClienteDto MapearADto(Cliente c) => new()
    {
        IdCliente = c.IdCliente,

        Nombre = c.Nombre,

        Apellido = c.Apellido,

        Identificacion = c.Identificacion,

        Telefono = c.Telefono,

        Email = c.Email,

        Direccion = c.Direccion,

        Activo = c.Activo,

        FechaCreacion = c.FechaCreacion
    };
}