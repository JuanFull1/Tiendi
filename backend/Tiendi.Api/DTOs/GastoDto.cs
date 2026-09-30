namespace Tiendi.Api.DTOs;

public class GastoDto
{
    public long IdGasto { get; set; }

    public string Descripcion { get; set; } = string.Empty;

    public string? Categoria { get; set; }

    public decimal Monto { get; set; }

    public DateTime Fecha { get; set; }

    public string RegistradoPor { get; set; } = string.Empty;
}
