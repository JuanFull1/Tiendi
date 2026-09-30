namespace Tiendi.Api.DTOs;

public class DetalleCompraDto
{
    public int IdProducto { get; set; }

    public decimal Cantidad { get; set; }

    public decimal CostoUnitario { get; set; }

    public decimal Descuento { get; set; }
}