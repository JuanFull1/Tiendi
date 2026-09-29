using Microsoft.EntityFrameworkCore;
using Tiendi.Api.Data;
using Tiendi.Api.DTOs;
using Tiendi.Api.Models;

namespace Tiendi.Api.Services;

public class ComprasService
{
    private readonly TiendiDbContext _context;

    public ComprasService(TiendiDbContext context)
    {
        _context = context;
    }

    // =========================================================
    // LISTAR COMPRAS
    // =========================================================

    public async Task<List<object>> ObtenerTodas()
    {
        return await _context.Compras
            .Include(c => c.IdProveedorNavigation)
            .Include(c => c.IdUsuarioNavigation)
            .Include(c => c.DetalleCompras)
                .ThenInclude(d => d.IdProductoNavigation)
            .OrderByDescending(c => c.Fecha)
            .Select(c => new
            {
                c.IdCompra,
                c.NumeroCompra,
                c.Fecha,
                c.Subtotal,
                c.Descuento,
                c.Total,
                c.Estado,
                c.Observacion,

                proveedor = new
                {
                    c.IdProveedorNavigation.IdProveedor,
                    c.IdProveedorNavigation.Nombre
                },

                usuario = new
                {
                    c.IdUsuarioNavigation.IdUsuario,
                    nombre = c.IdUsuarioNavigation.Nombre,
                    apellido = c.IdUsuarioNavigation.Apellido
                },

                detalles = c.DetalleCompras.Select(d => new
                {
                    d.IdDetalleCompra,
                    d.IdProducto,
                    producto = d.IdProductoNavigation.Nombre,
                    d.Cantidad,
                    d.CostoUnitario,
                    d.Descuento,
                    d.Subtotal
                })
            })
            .Cast<object>()
            .ToListAsync();
    }

    // =========================================================
    // REGISTRAR COMPRA
    // =========================================================

    public async Task<object> Crear(
        CrearCompraDto dto,
        int idUsuario)
    {
        // -----------------------------------------------------
        // VALIDACIONES GENERALES
        // -----------------------------------------------------

        if (dto.IdProveedor <= 0)
            throw new ArgumentException(
                "Debe seleccionar un proveedor."
            );

        if (string.IsNullOrWhiteSpace(dto.NumeroCompra))
            throw new ArgumentException(
                "El número de compra es obligatorio."
            );

        if (dto.Detalles == null || dto.Detalles.Count == 0)
            throw new ArgumentException(
                "La compra debe tener al menos un producto."
            );

        if (dto.Descuento < 0)
            throw new ArgumentException(
                "El descuento no puede ser negativo."
            );

        // -----------------------------------------------------
        // VERIFICAR PROVEEDOR
        // -----------------------------------------------------

        var proveedor = await _context.Proveedores
            .FirstOrDefaultAsync(
                p => p.IdProveedor == dto.IdProveedor
                     && p.Activo
            );

        if (proveedor == null)
            throw new ArgumentException(
                "El proveedor no existe o está inactivo."
            );

        // -----------------------------------------------------
        // VERIFICAR USUARIO
        // -----------------------------------------------------

        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(
                u => u.IdUsuario == idUsuario
                     && u.Activo
            );

        if (usuario == null)
            throw new ArgumentException(
                "El usuario autenticado no existe o está inactivo."
            );

        // -----------------------------------------------------
        // EVITAR NÚMERO DE COMPRA DUPLICADO
        // -----------------------------------------------------

        bool numeroExiste = await _context.Compras
            .AnyAsync(
                c => c.NumeroCompra == dto.NumeroCompra
            );

        if (numeroExiste)
            throw new ArgumentException(
                "El número de compra ya existe."
            );

        // -----------------------------------------------------
        // TRANSACCIÓN
        // -----------------------------------------------------

        using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            decimal subtotalCompra = 0;

            var detallesParaGuardar =
                new List<DetalleCompra>();

            // -------------------------------------------------
            // PROCESAR DETALLES
            // -------------------------------------------------

            foreach (var detalleDto in dto.Detalles)
            {
                if (detalleDto.IdProducto <= 0)
                {
                    throw new ArgumentException(
                        "Existe un producto inválido."
                    );
                }

                if (detalleDto.Cantidad <= 0)
                {
                    throw new ArgumentException(
                        "La cantidad debe ser mayor que cero."
                    );
                }

                if (detalleDto.CostoUnitario < 0)
                {
                    throw new ArgumentException(
                        "El costo unitario no puede ser negativo."
                    );
                }

                if (detalleDto.Descuento < 0)
                {
                    throw new ArgumentException(
                        "El descuento del detalle no puede ser negativo."
                    );
                }

                // ---------------------------------------------
                // BUSCAR PRODUCTO
                // ---------------------------------------------

                var producto = await _context.Productos
                    .FirstOrDefaultAsync(
                        p => p.IdProducto == detalleDto.IdProducto
                             && p.Activo
                    );

                if (producto == null)
                {
                    throw new ArgumentException(
                        $"El producto {detalleDto.IdProducto} no existe o está inactivo."
                    );
                }

                // ---------------------------------------------
                // CALCULAR SUBTOTAL DEL DETALLE
                // ---------------------------------------------

                decimal subtotalDetalle =
                    (
                        detalleDto.Cantidad
                        * detalleDto.CostoUnitario
                    )
                    - detalleDto.Descuento;

                if (subtotalDetalle < 0)
                {
                    throw new ArgumentException(
                        "El descuento del detalle no puede ser mayor al subtotal."
                    );
                }

                // ---------------------------------------------
                // CREAR DETALLE
                // ---------------------------------------------

                var detalle = new DetalleCompra
                {
                    IdProducto = producto.IdProducto,

                    Cantidad = detalleDto.Cantidad,

                    CostoUnitario =
                        detalleDto.CostoUnitario,

                    Descuento =
                        detalleDto.Descuento,

                    Subtotal =
                        subtotalDetalle
                };

                detallesParaGuardar.Add(detalle);

                subtotalCompra += subtotalDetalle;
            }

            // -------------------------------------------------
            // VALIDAR DESCUENTO GENERAL
            // -------------------------------------------------

            if (dto.Descuento > subtotalCompra)
            {
                throw new ArgumentException(
                    "El descuento de la compra no puede ser mayor al subtotal."
                );
            }

            decimal totalCompra =
                subtotalCompra - dto.Descuento;

            // -------------------------------------------------
            // CREAR CABECERA DE COMPRA
            // -------------------------------------------------

            var compra = new Compra
            {
                IdProveedor = dto.IdProveedor,

                IdUsuario = idUsuario,

                NumeroCompra = dto.NumeroCompra.Trim(),

                Fecha = DateTime.Now,

                Subtotal = subtotalCompra,

                Descuento = dto.Descuento,

                Total = totalCompra,

                Estado = "COMPLETADA",

                Observacion = dto.Observacion
            };

            _context.Compras.Add(compra);

            await _context.SaveChangesAsync();

            // -------------------------------------------------
            // GUARDAR DETALLES Y ACTUALIZAR STOCK
            // -------------------------------------------------

            foreach (var detalle in detallesParaGuardar)
            {
                detalle.IdCompra = compra.IdCompra;

                _context.DetalleCompras.Add(detalle);

                // ---------------------------------------------
                // BUSCAR PRODUCTO
                // ---------------------------------------------

                var producto = await _context.Productos
                    .FirstAsync(
                        p => p.IdProducto == detalle.IdProducto
                    );

                // ---------------------------------------------
                // GUARDAR STOCK ANTERIOR
                // ---------------------------------------------

                decimal stockAnterior =
                    producto.Stock;

                // ---------------------------------------------
                // AUMENTAR STOCK
                // ---------------------------------------------

                decimal stockNuevo =
                    stockAnterior + detalle.Cantidad;

                producto.Stock = stockNuevo;

                // ---------------------------------------------
                // ACTUALIZAR COSTO DEL PRODUCTO
                // ---------------------------------------------

                producto.Costo =
                    detalle.CostoUnitario;

                // ---------------------------------------------
                // REGISTRAR MOVIMIENTO
                // ---------------------------------------------

                var movimiento =
                    new MovimientosInventario
                    {
                        IdProducto =
                            producto.IdProducto,

                        IdUsuario =
                            idUsuario,

                        TipoMovimiento =
                            "COMPRA",

                        Cantidad =
                            detalle.Cantidad,

                        StockAnterior =
                            stockAnterior,

                        StockNuevo =
                            stockNuevo,

                        IdCompra =
                            compra.IdCompra,

                        Observacion =
                            $"Compra {compra.NumeroCompra}",

                        Fecha =
                            DateTime.Now
                    };

                _context.MovimientosInventarios
                    .Add(movimiento);
            }

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            // -------------------------------------------------
            // RESPUESTA
            // -------------------------------------------------

            return new
            {
                mensaje = "Compra registrada correctamente.",

                compra = new
                {
                    compra.IdCompra,
                    compra.NumeroCompra,
                    compra.Fecha,
                    compra.Subtotal,
                    compra.Descuento,
                    compra.Total,
                    compra.Estado,
                    compra.Observacion,
                    proveedor = proveedor.Nombre
                }
            };
        }
        catch
        {
            await transaction.RollbackAsync();

            throw;
        }
    }
}