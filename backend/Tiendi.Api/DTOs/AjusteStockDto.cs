using System.ComponentModel.DataAnnotations;

namespace Tiendi.Api.DTOs;

public class AjusteStockDto
{
    [Required]
    public int IdProducto { get; set; }

    [Required]
    public string TipoMovimiento { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue)]
    public decimal Cantidad { get; set; }

    [MaxLength(250)]
    public string? Observacion { get; set; }
}