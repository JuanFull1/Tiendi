using Microsoft.EntityFrameworkCore;
using Tiendi.Api.Data;
using Tiendi.Api.DTOs;

namespace Tiendi.Api.Services;

public class EstadisticasService
{
    private readonly TiendiDbContext _context;

    public EstadisticasService(TiendiDbContext context)
    {
        _context = context;
    }

    // RESUMEN GENERAL
    public async Task<EstadisticasResumenDto> ObtenerResumenAsync()
    {
        var ventas = _context.Ventas
            .Where(v => v.Estado == "COMPLETADA");

        var cantidadVentas = await ventas.CountAsync();

        var totalVentas = await ventas
            .SumAsync(v => (decimal?)v.Total) ?? 0;

        var promedioVenta = cantidadVentas > 0
            ? totalVentas / cantidadVentas
            : 0;

        var ventaMayor = await ventas
            .MaxAsync(v => (decimal?)v.Total) ?? 0;

        return new EstadisticasResumenDto
        {
            TotalVentas = totalVentas,
            CantidadVentas = cantidadVentas,
            PromedioVenta = promedioVenta,
            VentaMayor = ventaMayor
        };
    }

    // PRODUCTOS MÁS VENDIDOS
    public async Task<List<ProductoVendidoDto>> ObtenerProductosMasVendidosAsync()
    {
        var productos = await _context.DetalleVentas
            .Where(d => d.IdVentaNavigation.Estado == "COMPLETADA")
            .GroupBy(d => new
            {
                d.IdProducto,
                d.IdProductoNavigation.Nombre
            })
            .Select(g => new ProductoVendidoDto
            {
                IdProducto = g.Key.IdProducto,
                NombreProducto = g.Key.Nombre,
                CantidadVendida = g.Sum(x => x.Cantidad),
                TotalVendido = g.Sum(x => x.Subtotal)
            })
            .OrderByDescending(x => x.CantidadVendida)
            .Take(10)
            .ToListAsync();

        return productos;
    }

    // VENTAS AGRUPADAS POR FECHA
    public async Task<List<VentasPorFechaDto>> ObtenerVentasPorFechaAsync()
    {
        var ventas = await _context.Ventas
            .Where(v => v.Estado == "COMPLETADA")
            .GroupBy(v => v.Fecha.Date)
            .Select(g => new VentasPorFechaDto
            {
                Fecha = g.Key,
                CantidadVentas = g.Count(),
                TotalVentas = g.Sum(v => v.Total)
            })
            .OrderBy(x => x.Fecha)
            .ToListAsync();

        return ventas;
    }
}