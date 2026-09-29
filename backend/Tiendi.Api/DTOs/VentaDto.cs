namespace Tiendi.Api.DTOs;

public class VentaDto
{
    public long IdVenta { get; set; }

    public int? IdCliente { get; set; }

    public string? NombreCliente { get; set; }

    public int IdUsuario { get; set; }

    public string NombreUsuario { get; set; } = string.Empty;

    public string NumeroVenta { get; set; } = string.Empty;

    public DateTime Fecha { get; set; }

    public decimal Subtotal { get; set; }

    public decimal Descuento { get; set; }

    public decimal Total { get; set; }

    public string Estado { get; set; } = string.Empty;

    public string? MetodoPago { get; set; }

    public decimal? MontoPago { get; set; }

    public List<VentaDetalleDto> Detalles { get; set; } = new();
}

public class VentaDetalleDto
{
    public long IdDetalleVenta { get; set; }

    public int IdProducto { get; set; }

    public string NombreProducto { get; set; } = string.Empty;

    public decimal Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }

    public decimal Descuento { get; set; }

    public decimal Subtotal { get; set; }
}