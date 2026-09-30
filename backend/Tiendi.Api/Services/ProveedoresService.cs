using Microsoft.EntityFrameworkCore;
using Tiendi.Api.Data;
using Tiendi.Api.DTOs;
using Tiendi.Api.Models;

namespace Tiendi.Api.Services;

public class ProveedoresService
{
    private readonly TiendiDbContext _context;

    public ProveedoresService(TiendiDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProveedorDto>> ObtenerTodos()
    {
        return await _context.Proveedores
            .Where(p => p.Activo)
            .Select(p => new ProveedorDto
            {
                IdProveedor = p.IdProveedor,
                Nombre = p.Nombre,
                Identificacion = p.Identificacion,
                Telefono = p.Telefono,
                Email = p.Email,
                Direccion = p.Direccion,
                Activo = p.Activo,
                FechaCreacion = p.FechaCreacion
            })
            .ToListAsync();
    }

    public async Task<ProveedorDto?> ObtenerPorId(int id)
    {
        return await _context.Proveedores
            .Where(p => p.IdProveedor == id)
            .Select(p => new ProveedorDto
            {
                IdProveedor = p.IdProveedor,
                Nombre = p.Nombre,
                Identificacion = p.Identificacion,
                Telefono = p.Telefono,
                Email = p.Email,
                Direccion = p.Direccion,
                Activo = p.Activo,
                FechaCreacion = p.FechaCreacion
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ProveedorDto> Crear(CrearProveedorDto dto)
    {
        var proveedor = new Proveedor
        {
            Nombre = dto.Nombre,
            Identificacion = dto.Identificacion,
            Telefono = dto.Telefono,
            Email = dto.Email,
            Direccion = dto.Direccion,
            Activo = true,
            FechaCreacion = DateTime.Now
        };

        _context.Proveedores.Add(proveedor);

        await _context.SaveChangesAsync();

        return new ProveedorDto
        {
            IdProveedor = proveedor.IdProveedor,
            Nombre = proveedor.Nombre,
            Identificacion = proveedor.Identificacion,
            Telefono = proveedor.Telefono,
            Email = proveedor.Email,
            Direccion = proveedor.Direccion,
            Activo = proveedor.Activo,
            FechaCreacion = proveedor.FechaCreacion
        };
    }

    public async Task<bool> Actualizar(
        int id,
        ActualizarProveedorDto dto)
    {
        var proveedor = await _context.Proveedores
            .FirstOrDefaultAsync(p => p.IdProveedor == id);

        if (proveedor == null)
            return false;

        proveedor.Nombre = dto.Nombre;
        proveedor.Identificacion = dto.Identificacion;
        proveedor.Telefono = dto.Telefono;
        proveedor.Email = dto.Email;
        proveedor.Direccion = dto.Direccion;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> Desactivar(int id)
    {
        var proveedor = await _context.Proveedores
            .FirstOrDefaultAsync(p => p.IdProveedor == id);

        if (proveedor == null)
            return false;

        proveedor.Activo = false;

        await _context.SaveChangesAsync();

        return true;
    }
}