using System.Data;
using Microsoft.EntityFrameworkCore;
using Tiendi.Api.Data;
using Tiendi.Api.DTOs;
using Tiendi.Api.Models;

namespace Tiendi.Api.Services;

public class VentasService
{
    private static readonly HashSet<string> MetodosPagoPermitidos =
    [
        "EFECTIVO",
        "TRANSFERENCIA",
        "TARJETA"
    ];

    private readonly TiendiDbContext _context;

    public VentasService(TiendiDbContext context)
    {
        _context = context;
    }

    public async Task<List<VentaDto>> ObtenerTodasAsync()
    {
        List<Venta> ventas = await ConsultaVentas()
            .OrderByDescending(v => v.Fecha)
            .ThenByDescending(v => v.IdVenta)
            .ToListAsync();

        return ventas.Select(MapearADto).ToList();
    }

    public async Task<VentaDto?> ObtenerPorIdAsync(long id)
    {
        Venta? venta = await ConsultaVentas()
            .FirstOrDefaultAsync(v => v.IdVenta == id);

        return venta is null ? null : MapearADto(venta);
    }

    public async Task<VentaDto> CrearAsync(
        CrearVentaDto dto,
        int idUsuario
    )
    {
        if (dto.Detalles is null || dto.Detalles.Count == 0)
        {
            throw new ArgumentException(
                "La venta debe incluir al menos un producto."
            );
        }

        string metodoPago =
            dto.MetodoPago?.Trim().ToUpperInvariant() ?? string.Empty;

        if (!MetodosPagoPermitidos.Contains(metodoPago))
        {
            throw new ArgumentException(
                "El método de pago debe ser EFECTIVO, TRANSFERENCIA o TARJETA."
            );
        }

        decimal descuentoVenta = Redondear(dto.Descuento);

        if (descuentoVenta < 0)
        {
            throw new ArgumentException(
                "El descuento de la venta no puede ser negativo."
            );
        }

        List<(DetalleVentaDto Dto, decimal Cantidad, decimal Descuento)> detallesValidados =
            new();

        foreach (DetalleVentaDto detalle in dto.Detalles)
        {
            if (detalle is null)
            {
                throw new ArgumentException(
                    "La venta contiene un detalle inválido."
                );
            }

            if (detalle.IdProducto <= 0)
            {
                throw new ArgumentException(
                    "La venta contiene un producto inválido."
                );
            }

            decimal cantidad = Redondear(detalle.Cantidad);
            decimal descuento = Redondear(detalle.Descuento);

            if (cantidad <= 0)
            {
                throw new ArgumentException(
                    "La cantidad de cada producto debe ser mayor que cero."
                );
            }

            if (descuento < 0)
            {
                throw new ArgumentException(
                    "El descuento del producto no puede ser negativo."
                );
            }

            detallesValidados.Add((detalle, cantidad, descuento));
        }

        if (dto.IdCliente.HasValue && dto.IdCliente.Value <= 0)
        {
            throw new ArgumentException("El cliente seleccionado no es válido.");
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable
            );

        try
        {
            Cliente? cliente = null;

            if (dto.IdCliente.HasValue)
            {
                cliente = await _context.Clientes
                    .FirstOrDefaultAsync(
                        c => c.IdCliente == dto.IdCliente.Value && c.Activo
                    );

                if (cliente is null)
                {
                    throw new ArgumentException(
                        "El cliente no existe o está inactivo."
                    );
                }
            }

            Usuario? usuario = await _context.Usuarios
                .FirstOrDefaultAsync(
                    u => u.IdUsuario == idUsuario && u.Activo
                );

            if (usuario is null)
            {
                throw new ArgumentException(
                    "El usuario autenticado no existe o está inactivo."
                );
            }

            int[] idsProducto = detallesValidados
                .Select(d => d.Dto.IdProducto)
                .Distinct()
                .ToArray();

            Dictionary<int, Producto> productos = await _context.Productos
                .Where(p => idsProducto.Contains(p.IdProducto))
                .ToDictionaryAsync(p => p.IdProducto);

            foreach (int idProducto in idsProducto)
            {
                if (!productos.TryGetValue(idProducto, out Producto? producto))
                {
                    throw new ArgumentException(
                        $"El producto {idProducto} no existe."
                    );
                }

                if (!producto.Activo)
                {
                    throw new ArgumentException(
                        $"El producto {idProducto} está inactivo."
                    );
                }
            }

            Dictionary<int, decimal> cantidadesPorProducto = detallesValidados
                .GroupBy(d => d.Dto.IdProducto)
                .ToDictionary(
                    grupo => grupo.Key,
                    grupo => grupo.Sum(d => d.Cantidad)
                );

            decimal subtotalVenta = 0;

            foreach ((DetalleVentaDto detalleDto, decimal cantidad, decimal descuento)
                in detallesValidados)
            {
                Producto producto = productos[detalleDto.IdProducto];
                decimal importeBruto = Redondear(cantidad * producto.PrecioVenta);

                if (descuento > importeBruto)
                {
                    throw new ArgumentException(
                        $"El descuento del producto {producto.Nombre} no puede superar su importe."
                    );
                }

                subtotalVenta += Redondear(importeBruto - descuento);
            }

            subtotalVenta = Redondear(subtotalVenta);

            if (descuentoVenta > subtotalVenta)
            {
                throw new ArgumentException(
                    "El descuento de la venta no puede superar el subtotal."
                );
            }

            decimal totalVenta = Redondear(subtotalVenta - descuentoVenta);

            if (totalVenta <= 0)
            {
                throw new ArgumentException(
                    "El total debe ser mayor que cero para registrar el pago."
                );
            }

            foreach ((int idProducto, decimal cantidad) in cantidadesPorProducto)
            {
                Producto producto = productos[idProducto];

                if (producto.Stock < cantidad)
                {
                    throw new ArgumentException(
                        $"Stock insuficiente para {producto.Nombre}. Disponible: {producto.Stock}; solicitado: {cantidad}."
                    );
                }
            }

            DateTime fecha = DateTime.Now;

            Venta venta = new()
            {
                IdCliente = cliente?.IdCliente,
                IdUsuario = usuario.IdUsuario,
                NumeroVenta = GenerarNumeroVenta(),
                Fecha = fecha,
                Subtotal = subtotalVenta,
                Descuento = descuentoVenta,
                Total = totalVenta,
                Estado = "COMPLETADA",
                IdClienteNavigation = cliente,
                IdUsuarioNavigation = usuario
            };

            _context.Ventas.Add(venta);
            await _context.SaveChangesAsync();

            foreach ((DetalleVentaDto detalleDto, decimal cantidad, decimal descuento)
                in detallesValidados)
            {
                Producto producto = productos[detalleDto.IdProducto];
                decimal importeBruto = Redondear(cantidad * producto.PrecioVenta);

                DetalleVenta detalle = new()
                {
                    IdVenta = venta.IdVenta,
                    IdProducto = producto.IdProducto,
                    Cantidad = cantidad,
                    PrecioUnitario = producto.PrecioVenta,
                    CostoUnitario = producto.Costo,
                    Descuento = descuento,
                    Subtotal = Redondear(importeBruto - descuento),
                    IdVentaNavigation = venta,
                    IdProductoNavigation = producto
                };

                _context.DetalleVentas.Add(detalle);
                venta.DetalleVenta.Add(detalle);
            }

            foreach ((int idProducto, decimal cantidad) in cantidadesPorProducto)
            {
                Producto producto = productos[idProducto];
                decimal stockAnterior = producto.Stock;
                decimal stockNuevo = Redondear(stockAnterior - cantidad);

                producto.Stock = stockNuevo;

                MovimientosInventario movimiento = new()
                {
                    IdProducto = producto.IdProducto,
                    IdUsuario = usuario.IdUsuario,
                    TipoMovimiento = "VENTA",
                    Cantidad = cantidad,
                    StockAnterior = stockAnterior,
                    StockNuevo = stockNuevo,
                    IdVenta = venta.IdVenta,
                    Observacion = $"Venta {venta.NumeroVenta}",
                    Fecha = fecha,
                    IdProductoNavigation = producto,
                    IdUsuarioNavigation = usuario,
                    IdVentaNavigation = venta
                };

                _context.MovimientosInventarios.Add(movimiento);
            }

            Pago pago = new()
            {
                IdVenta = venta.IdVenta,
                MetodoPago = metodoPago,
                Monto = totalVenta,
                Fecha = fecha,
                IdVentaNavigation = venta
            };

            _context.Pagos.Add(pago);
            venta.Pagos.Add(pago);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return MapearADto(venta);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private IQueryable<Venta> ConsultaVentas()
    {
        return _context.Ventas
            .AsNoTrackingWithIdentityResolution()
            .Include(v => v.IdClienteNavigation)
            .Include(v => v.IdUsuarioNavigation)
            .Include(v => v.DetalleVenta)
                .ThenInclude(d => d.IdProductoNavigation)
            .Include(v => v.Pagos);
    }

    private static VentaDto MapearADto(Venta venta)
    {
        Pago? pago = venta.Pagos
            .OrderBy(p => p.IdPago)
            .FirstOrDefault();

        return new VentaDto
        {
            IdVenta = venta.IdVenta,
            IdCliente = venta.IdCliente,
            NombreCliente = venta.IdClienteNavigation?.Nombre,
            IdUsuario = venta.IdUsuario,
            NombreUsuario =
                $"{venta.IdUsuarioNavigation.Nombre} {venta.IdUsuarioNavigation.Apellido}".Trim(),
            NumeroVenta = venta.NumeroVenta,
            Fecha = venta.Fecha,
            Subtotal = venta.Subtotal,
            Descuento = venta.Descuento,
            Total = venta.Total,
            Estado = venta.Estado,
            MetodoPago = pago?.MetodoPago,
            MontoPago = pago?.Monto,
            Detalles = venta.DetalleVenta
                .OrderBy(d => d.IdDetalleVenta)
                .Select(d => new VentaDetalleDto
                {
                    IdDetalleVenta = d.IdDetalleVenta,
                    IdProducto = d.IdProducto,
                    NombreProducto = d.IdProductoNavigation.Nombre,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario,
                    Descuento = d.Descuento,
                    Subtotal = d.Subtotal
                })
                .ToList()
        };
    }

    private static string GenerarNumeroVenta()
    {
        return $"V-{Guid.NewGuid():N}"[..30];
    }

    private static decimal Redondear(decimal valor)
    {
        return Math.Round(valor, 2, MidpointRounding.AwayFromZero);
    }
}