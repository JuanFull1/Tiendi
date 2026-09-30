namespace Tiendi.Api.DTOs;

public class MovimientoInventarioDto
{
    public long IdMovimiento { get; set; }

    public int IdProducto { get; set; }

    public string Producto { get; set; } = string.Empty;

    public string TipoMovimiento { get; set; } = string.Empty;

    public decimal Cantidad { get; set; }

    public decimal StockAnterior { get; set; }

    public decimal StockNuevo { get; set; }

    public string? Observacion { get; set; }

    public DateTime Fecha { get; set; }
}