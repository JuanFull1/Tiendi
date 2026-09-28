using System;
using System.Collections.Generic;

namespace Tiendi.Api.Models;

public partial class DetalleCompra
{
    public long IdDetalleCompra { get; set; }

    public long IdCompra { get; set; }

    public int IdProducto { get; set; }

    public decimal Cantidad { get; set; }

    public decimal CostoUnitario { get; set; }

    public decimal Descuento { get; set; }

    public decimal Subtotal { get; set; }

    public virtual Compra IdCompraNavigation { get; set; } = null!;

    public virtual Producto IdProductoNavigation { get; set; } = null!;
}
