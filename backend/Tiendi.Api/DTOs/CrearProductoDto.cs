using System.ComponentModel.DataAnnotations;

namespace Tiendi.Api.DTOs;

public class CrearProductoDto
{
    public int? IdCategoria { get; set; }

    [MaxLength(50)]
    public string? Codigo { get; set; }

    [Required]
    [MaxLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? Descripcion { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Costo { get; set; }

    [Range(0, double.MaxValue)]
    public decimal PrecioVenta { get; set; }

    [Range(0, double.MaxValue)]
    public decimal StockInicial { get; set; }

    [Range(0, double.MaxValue)]
    public decimal StockMinimo { get; set; }
}