namespace Tiendi.Api.DTOs;

public class BalanceResumenDto
{
    public DateTime Desde { get; set; }

    public DateTime Hasta { get; set; }

    public decimal TotalIngresos { get; set; }

    public int CantidadVentas { get; set; }

    public decimal TotalGastos { get; set; }

    public int CantidadGastos { get; set; }

    public decimal TotalCompras { get; set; }

    public int CantidadCompras { get; set; }

    public decimal Balance { get; set; }

    public List<IngresoPorMetodoDto> IngresosPorMetodoPago { get; set; } = new();
}

public class IngresoPorMetodoDto
{
    public string MetodoPago { get; set; } = string.Empty;

    public decimal Total { get; set; }
}