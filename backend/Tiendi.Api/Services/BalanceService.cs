using Microsoft.EntityFrameworkCore;
using Tiendi.Api.Data;
using Tiendi.Api.DTOs;
using Tiendi.Api.Models;

namespace Tiendi.Api.Services;

public class BalanceService
{
    private const string VentaCompletada = "COMPLETADA";

    private readonly TiendiDbContext _context;

    public BalanceService(TiendiDbContext context)
    {
        _context = context;
    }

    public async Task<BalanceResumenDto> ObtenerResumenAsync(
        DateTime? desde,
        DateTime? hasta
    )
    {
        (DateTime inicio, DateTime finExclusivo) =
            NormalizarPeriodo(desde, hasta);

        IQueryable<Venta> ventas = _context.Ventas
            .Where(v =>
                v.Estado == VentaCompletada &&
                v.Fecha >= inicio &&
                v.Fecha < finExclusivo
            );

        IQueryable<Gasto> gastos = _context.Gastos
            .Where(g =>
                g.Activo &&
                g.Fecha >= inicio &&
                g.Fecha < finExclusivo
            );

        decimal totalIngresos =
            await ventas.SumAsync(v => v.Total);

        int cantidadVentas =
            await ventas.CountAsync();

        decimal totalGastos =
            await gastos.SumAsync(g => g.Monto);

        int cantidadGastos =
            await gastos.CountAsync();

        List<IngresoPorMetodoDto> porMetodo = await _context.Pagos
            .Where(p =>
                p.IdVentaNavigation.Estado == VentaCompletada &&
                p.IdVentaNavigation.Fecha >= inicio &&
                p.IdVentaNavigation.Fecha < finExclusivo
            )
            .GroupBy(p => p.MetodoPago)
            .Select(g => new IngresoPorMetodoDto
            {
                MetodoPago = g.Key,
                Total = g.Sum(p => p.Monto)
            })
            .OrderByDescending(x => x.Total)
            .ToListAsync();

        return new BalanceResumenDto
        {
            Desde = inicio,
            Hasta = finExclusivo.AddDays(-1),
            TotalIngresos = totalIngresos,
            CantidadVentas = cantidadVentas,
            TotalGastos = totalGastos,
            CantidadGastos = cantidadGastos,
            Balance = totalIngresos - totalGastos,
            IngresosPorMetodoPago = porMetodo
        };
    }

    public async Task<List<GastoDto>> ListarGastosAsync(
        DateTime? desde,
        DateTime? hasta
    )
    {
        (DateTime inicio, DateTime finExclusivo) =
            NormalizarPeriodo(desde, hasta);

        return await _context.Gastos
            .Where(g =>
                g.Activo &&
                g.Fecha >= inicio &&
                g.Fecha < finExclusivo
            )
            .OrderByDescending(g => g.Fecha)
            .Select(g => new GastoDto
            {
                IdGasto = g.IdGasto,
                Descripcion = g.Descripcion,
                Categoria = g.Categoria,
                Monto = g.Monto,
                Fecha = g.Fecha,
                RegistradoPor =
                    (g.IdUsuarioNavigation.Nombre + " " +
                     (g.IdUsuarioNavigation.Apellido ?? "")).Trim()
            })
            .ToListAsync();
    }

    public async Task<GastoDto> RegistrarGastoAsync(
        CrearGastoDto dto,
        int idUsuario
    )
    {
        string descripcion = dto.Descripcion?.Trim() ?? string.Empty;

        string? categoria =
            string.IsNullOrWhiteSpace(dto.Categoria)
                ? null
                : dto.Categoria.Trim();

        if (descripcion.Length == 0)
        {
            throw new ArgumentException("La descripción es obligatoria.");
        }

        if (descripcion.Length > 250)
        {
            throw new ArgumentException("La descripción no puede superar 250 caracteres.");
        }

        if (categoria is not null && categoria.Length > 100)
        {
            throw new ArgumentException("La categoría no puede superar 100 caracteres.");
        }

        if (dto.Monto <= 0)
        {
            throw new ArgumentException("El monto debe ser mayor a 0.");
        }

        DateTime ahora = DateTime.Now;

        DateTime fecha = dto.Fecha ?? ahora;

        // Si solo se eligió el día de hoy (sin hora), se guarda la hora actual.
        if (fecha.Date == ahora.Date && fecha.TimeOfDay == TimeSpan.Zero)
        {
            fecha = ahora;
        }

        if (fecha.Date > ahora.Date)
        {
            throw new ArgumentException("La fecha del gasto no puede ser futura.");
        }

        var gasto = new Gasto
        {
            IdUsuario = idUsuario,
            Descripcion = descripcion,
            Categoria = categoria,
            Monto = Math.Round(dto.Monto, 2),
            Fecha = fecha,
            Activo = true
        };

        _context.Gastos.Add(gasto);

        await _context.SaveChangesAsync();

        Usuario? usuario = await _context.Usuarios.FindAsync(idUsuario);

        return new GastoDto
        {
            IdGasto = gasto.IdGasto,
            Descripcion = gasto.Descripcion,
            Categoria = gasto.Categoria,
            Monto = gasto.Monto,
            Fecha = gasto.Fecha,
            RegistradoPor =
                $"{usuario?.Nombre} {usuario?.Apellido}".Trim()
        };
    }

    // Anula un gasto (no lo borra, solo cambia Activo a false).
    public async Task<bool> AnularGastoAsync(long idGasto)
    {
        Gasto? gasto = await _context.Gastos
            .FirstOrDefaultAsync(g => g.IdGasto == idGasto && g.Activo);

        if (gasto is null)
        {
            return false;
        }

        gasto.Activo = false;

        await _context.SaveChangesAsync();

        return true;
    }

    // Por defecto: desde el día 1 del mes actual hasta hoy.
    // "hasta" incluye todo ese día, por eso se devuelve el día siguiente como límite.
    private static (DateTime inicio, DateTime finExclusivo) NormalizarPeriodo(
        DateTime? desde,
        DateTime? hasta
    )
    {
        DateTime hoy = DateTime.Today;

        DateTime inicio =
            (desde ?? new DateTime(hoy.Year, hoy.Month, 1)).Date;

        DateTime fin =
            (hasta ?? hoy).Date;

        if (inicio > fin)
        {
            throw new ArgumentException(
                "La fecha 'desde' no puede ser mayor que la fecha 'hasta'."
            );
        }

        return (inicio, fin.AddDays(1));
    }
}
