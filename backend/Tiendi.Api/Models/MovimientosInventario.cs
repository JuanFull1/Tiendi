using System;
using System.Collections.Generic;

namespace Tiendi.Api.Models;

public partial class MovimientosInventario
{
    public long IdMovimiento { get; set; }

    public int IdProducto { get; set; }

    public int IdUsuario { get; set; }

    public string TipoMovimiento { get; set; } = null!;

    public decimal Cantidad { get; set; }

    public decimal StockAnterior { get; set; }

    public decimal StockNuevo { get; set; }

    public long? IdVenta { get; set; }

    public long? IdCompra { get; set; }

    public string? Observacion { get; set; }

    public DateTime Fecha { get; set; }

    public virtual Compra? IdCompraNavigation { get; set; }

    public virtual Producto IdProductoNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;

    public virtual Venta? IdVentaNavigation { get; set; }
}
