using System;
using System.Collections.Generic;

namespace Tiendi.Api.Models;

public partial class DetalleVenta
{
    public long IdDetalleVenta { get; set; }

    public long IdVenta { get; set; }

    public int IdProducto { get; set; }

    public decimal Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }

    public decimal CostoUnitario { get; set; }

    public decimal Descuento { get; set; }

    public decimal Subtotal { get; set; }

    public virtual Producto IdProductoNavigation { get; set; } = null!;

    public virtual Venta IdVentaNavigation { get; set; } = null!;
}
