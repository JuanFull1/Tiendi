namespace Tiendi.Api.DTOs;

public class ProductoDto
{
    public int IdProducto { get; set; }

    public int? IdCategoria { get; set; }

    public string? Categoria { get; set; }

    public string? Codigo { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public string? ImagenUrl { get; set; }

    public decimal Costo { get; set; }

    public decimal PrecioVenta { get; set; }

    public decimal Stock { get; set; }

    public decimal StockMinimo { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }
}