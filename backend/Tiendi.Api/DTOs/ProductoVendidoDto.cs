namespace Tiendi.Api.DTOs;

public class ProductoVendidoDto
{
    public int IdProducto { get; set; }

    public string NombreProducto { get; set; } = string.Empty;

    public decimal CantidadVendida { get; set; }

    public decimal TotalVendido { get; set; }
}