namespace Tiendi.Api.DTOs;

public class ClienteDto
{
    public int IdCliente { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Apellido { get; set; }

    public string? Identificacion { get; set; }

    public string? Telefono { get; set; }

    public string? Email { get; set; }

    public string? Direccion { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }
}