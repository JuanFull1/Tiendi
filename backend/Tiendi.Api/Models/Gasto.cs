using System;
using System.Collections.Generic;

namespace Tiendi.Api.Models;

public partial class Gasto
{
    public long IdGasto { get; set; }

    public int IdUsuario { get; set; }

    public string Descripcion { get; set; } = null!;

    public string? Categoria { get; set; }

    public decimal Monto { get; set; }

    public DateTime Fecha { get; set; }

    public bool Activo { get; set; }

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
