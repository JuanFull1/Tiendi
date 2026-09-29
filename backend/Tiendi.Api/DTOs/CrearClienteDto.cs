using System.ComponentModel.DataAnnotations;

namespace Tiendi.Api.DTOs;

public class CrearClienteDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Apellido { get; set; }

    [MaxLength(20)]
    public string? Identificacion { get; set; }

    [MaxLength(20)]
    public string? Telefono { get; set; }

    [MaxLength(150)]
    [EmailAddress(ErrorMessage = "El email no tiene un formato valido.")]
    public string? Email { get; set; }

    [MaxLength(250)]
    public string? Direccion { get; set; }
}