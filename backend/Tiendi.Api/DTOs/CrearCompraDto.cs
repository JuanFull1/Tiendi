namespace Tiendi.Api.DTOs;

public class CrearCompraDto
{
    public int IdProveedor { get; set; }

    public string NumeroCompra { get; set; } = string.Empty;

    public decimal Descuento { get; set; }

    public string? Observacion { get; set; }

    public List<DetalleCompraDto> Detalles { get; set; } = new();
}