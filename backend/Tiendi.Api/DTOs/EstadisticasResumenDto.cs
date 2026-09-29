namespace Tiendi.Api.DTOs;

public class EstadisticasResumenDto
{
    public decimal TotalVentas { get; set; }

    public int CantidadVentas { get; set; }

    public decimal PromedioVenta { get; set; }

    public decimal VentaMayor { get; set; }
}