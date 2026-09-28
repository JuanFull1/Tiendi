using System;
using System.Collections.Generic;

namespace Tiendi.Api.Models;

public partial class Venta
{
    public long IdVenta { get; set; }

    public int? IdCliente { get; set; }

    public int IdUsuario { get; set; }

    public string NumeroVenta { get; set; } = null!;

    public DateTime Fecha { get; set; }

    public decimal Subtotal { get; set; }

    public decimal Descuento { get; set; }

    public decimal Total { get; set; }

    public string Estado { get; set; } = null!;

    public virtual ICollection<DetalleVenta> DetalleVenta { get; set; } = new List<DetalleVenta>();

    public virtual Cliente? IdClienteNavigation { get; set; }

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;

    public virtual ICollection<MovimientosInventario> MovimientosInventarios { get; set; } = new List<MovimientosInventario>();

    public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();
}
