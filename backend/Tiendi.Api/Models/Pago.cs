using System;
using System.Collections.Generic;

namespace Tiendi.Api.Models;

public partial class Pago
{
    public long IdPago { get; set; }

    public long IdVenta { get; set; }

    public string MetodoPago { get; set; } = null!;

    public decimal Monto { get; set; }

    public DateTime Fecha { get; set; }

    public virtual Venta IdVentaNavigation { get; set; } = null!;
}
