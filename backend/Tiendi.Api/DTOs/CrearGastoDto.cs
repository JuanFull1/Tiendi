namespace Tiendi.Api.DTOs;

public class CrearGastoDto
{
    public string Descripcion { get; set; } = string.Empty;

    public string? Categoria { get; set; }

    public decimal Monto { get; set; }

    // Si no se envía, se usa la fecha y hora actual.
    public DateTime? Fecha { get; set; }
}
