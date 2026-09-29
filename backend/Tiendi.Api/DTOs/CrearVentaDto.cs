namespace Tiendi.Api.DTOs;

public class CrearVentaDto
{
    public int? IdCliente { get; set; }

    public decimal Descuento { get; set; }

    public string MetodoPago { get; set; } = string.Empty;

    public List<DetalleVentaDto> Detalles { get; set; } = new();
}