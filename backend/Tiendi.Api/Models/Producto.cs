using System;
using System.Collections.Generic;

namespace Tiendi.Api.Models;

public partial class Producto
{
    public int IdProducto { get; set; }

    public int? IdCategoria { get; set; }

    public string? Codigo { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public string? ImagenUrl { get; set; }

    public decimal Costo { get; set; }

    public decimal PrecioVenta { get; set; }

    public decimal Stock { get; set; }

    public decimal StockMinimo { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public virtual ICollection<DetalleCompra> DetalleCompras { get; set; } = new List<DetalleCompra>();

    public virtual ICollection<DetalleVenta> DetalleVenta { get; set; } = new List<DetalleVenta>();

    public virtual Categoria? IdCategoriaNavigation { get; set; }

    public virtual ICollection<MovimientosInventario> MovimientosInventarios { get; set; } = new List<MovimientosInventario>();
}
