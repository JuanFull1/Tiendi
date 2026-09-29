namespace Tiendi.Api.DTOs;

public class VentasPorFechaDto
{
    public DateTime Fecha { get; set; }

    public int CantidadVentas { get; set; }

    public decimal TotalVentas { get; set; }
}