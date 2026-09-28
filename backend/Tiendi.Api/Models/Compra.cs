using System;
using System.Collections.Generic;

namespace Tiendi.Api.Models;

public partial class Compra
{
    public long IdCompra { get; set; }

    public int IdProveedor { get; set; }

    public int IdUsuario { get; set; }

    public string NumeroCompra { get; set; } = null!;

    public DateTime Fecha { get; set; }

    public decimal Subtotal { get; set; }

    public decimal Descuento { get; set; }

    public decimal Total { get; set; }

    public string Estado { get; set; } = null!;

    public string? Observacion { get; set; }

    public virtual ICollection<DetalleCompra> DetalleCompras { get; set; } = new List<DetalleCompra>();

    public virtual Proveedor IdProveedorNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;

    public virtual ICollection<MovimientosInventario> MovimientosInventarios { get; set; } = new List<MovimientosInventario>();
}
