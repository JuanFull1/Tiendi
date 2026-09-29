using Microsoft.EntityFrameworkCore;
using Tiendi.Api.Data;
using Tiendi.Api.DTOs;
using Tiendi.Api.Models;

namespace Tiendi.Api.Services;

public class InventarioService
{
    private readonly TiendiDbContext _context;

    public InventarioService(
        TiendiDbContext context)
    {
        _context = context;
    }

    public async Task<List<CategoriaDto>>
        ObtenerCategoriasAsync()
    {
        return await _context.Categorias
            .AsNoTracking()
            .Where(c => c.Activo)
            .OrderBy(c => c.Nombre)
            .Select(c => new CategoriaDto
            {
                IdCategoria =
                    c.IdCategoria,
                Nombre =
                    c.Nombre,
                Descripcion =
                    c.Descripcion
            })
            .ToListAsync();
    }

    public async Task<List<ProductoDto>>
        ObtenerStockBajoAsync()
    {
        return await _context.Productos
            .AsNoTracking()
            .Include(
                p => p.IdCategoriaNavigation
            )
            .Where(
                p =>
                    p.Activo &&
                    p.Stock <= p.StockMinimo
            )
            .OrderBy(p => p.Stock)
            .Select(p => new ProductoDto
            {
                IdProducto =
                    p.IdProducto,
                IdCategoria =
                    p.IdCategoria,
                Categoria =
                    p.IdCategoriaNavigation != null
                        ? p.IdCategoriaNavigation.Nombre
                        : null,
                Codigo =
                    p.Codigo,
                Nombre =
                    p.Nombre,
                Descripcion =
                    p.Descripcion,
                ImagenUrl =
                    p.ImagenUrl,
                Costo =
                    p.Costo,
                PrecioVenta =
                    p.PrecioVenta,
                Stock =
                    p.Stock,
                StockMinimo =
                    p.StockMinimo,
                Activo =
                    p.Activo,
                FechaCreacion =
                    p.FechaCreacion
            })
            .ToListAsync();
    }

    public async Task<List<MovimientoInventarioDto>>
        ObtenerMovimientosAsync(
            int? idProducto = null)
    {
        var consulta =
            _context.MovimientosInventarios
                .AsNoTracking()
                .Include(
                    m => m.IdProductoNavigation
                )
                .AsQueryable();

        if (idProducto.HasValue)
        {
            consulta =
                consulta.Where(
                    m =>
                        m.IdProducto ==
                        idProducto.Value
                );
        }

        return await consulta
            .OrderByDescending(m => m.Fecha)
            .Take(100)
            .Select(
                m =>
                    new MovimientoInventarioDto
                    {
                        IdMovimiento =
                            m.IdMovimiento,
                        IdProducto =
                            m.IdProducto,
                        Producto =
                            m.IdProductoNavigation.Nombre,
                        TipoMovimiento =
                            m.TipoMovimiento,
                        Cantidad =
                            m.Cantidad,
                        StockAnterior =
                            m.StockAnterior,
                        StockNuevo =
                            m.StockNuevo,
                        Observacion =
                            m.Observacion,
                        Fecha =
                            m.Fecha
                    }
            )
            .ToListAsync();
    }

    public async Task<MovimientoInventarioDto>
        AjustarStockAsync(
            AjusteStockDto dto,
            int idUsuario)
    {
        string tipo =
            dto.TipoMovimiento
                .Trim()
                .ToUpperInvariant();

        if (
            tipo != "AJUSTE_ENTRADA" &&
            tipo != "AJUSTE_SALIDA"
        )
        {
            throw new ArgumentException(
                "El tipo debe ser AJUSTE_ENTRADA o AJUSTE_SALIDA."
            );
        }

        if (dto.Cantidad <= 0)
        {
            throw new ArgumentException(
                "La cantidad debe ser mayor a cero."
            );
        }

        var producto =
            await _context.Productos
                .FirstOrDefaultAsync(
                    p =>
                        p.IdProducto ==
                            dto.IdProducto &&
                        p.Activo
                );

        if (producto == null)
        {
            throw new KeyNotFoundException(
                "El producto no existe."
            );
        }

        decimal stockAnterior =
            producto.Stock;

        decimal stockNuevo;

        if (tipo == "AJUSTE_ENTRADA")
        {
            stockNuevo =
                stockAnterior +
                dto.Cantidad;
        }
        else
        {
            if (dto.Cantidad > stockAnterior)
            {
                throw new ArgumentException(
                    "No existe stock suficiente para realizar la salida."
                );
            }

            stockNuevo =
                stockAnterior -
                dto.Cantidad;
        }

        producto.Stock =
            stockNuevo;

        var movimiento =
            new MovimientosInventario
            {
                IdProducto =
                    producto.IdProducto,
                IdUsuario =
                    idUsuario,
                TipoMovimiento =
                    tipo,
                Cantidad =
                    dto.Cantidad,
                StockAnterior =
                    stockAnterior,
                StockNuevo =
                    stockNuevo,
                Observacion =
                    string.IsNullOrWhiteSpace(
                        dto.Observacion)
                        ? null
                        : dto.Observacion.Trim(),
                Fecha =
                    DateTime.Now
            };

        _context.MovimientosInventarios.Add(
            movimiento
        );

        await _context.SaveChangesAsync();

        return new MovimientoInventarioDto
        {
            IdMovimiento =
                movimiento.IdMovimiento,
            IdProducto =
                producto.IdProducto,
            Producto =
                producto.Nombre,
            TipoMovimiento =
                movimiento.TipoMovimiento,
            Cantidad =
                movimiento.Cantidad,
            StockAnterior =
                movimiento.StockAnterior,
            StockNuevo =
                movimiento.StockNuevo,
            Observacion =
                movimiento.Observacion,
            Fecha =
                movimiento.Fecha
        };
    }
}