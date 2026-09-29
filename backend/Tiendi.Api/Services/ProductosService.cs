using Microsoft.EntityFrameworkCore;
using Tiendi.Api.Data;
using Tiendi.Api.DTOs;
using Tiendi.Api.Models;

namespace Tiendi.Api.Services;

public class ProductosService
{
    private readonly TiendiDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public ProductosService(
        TiendiDbContext context,
        IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    public async Task<List<ProductoDto>> ObtenerTodosAsync()
    {
        return await _context.Productos
            .AsNoTracking()
            .Include(p => p.IdCategoriaNavigation)
            .Where(p => p.Activo)
            .OrderBy(p => p.Nombre)
            .Select(p => new ProductoDto
            {
                IdProducto = p.IdProducto,
                IdCategoria = p.IdCategoria,
                Categoria = p.IdCategoriaNavigation != null
                    ? p.IdCategoriaNavigation.Nombre
                    : null,
                Codigo = p.Codigo,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                ImagenUrl = p.ImagenUrl,
                Costo = p.Costo,
                PrecioVenta = p.PrecioVenta,
                Stock = p.Stock,
                StockMinimo = p.StockMinimo,
                Activo = p.Activo,
                FechaCreacion = p.FechaCreacion
            })
            .ToListAsync();
    }

    public async Task<ProductoDto?> ObtenerPorIdAsync(
        int idProducto)
    {
        return await _context.Productos
            .AsNoTracking()
            .Include(p => p.IdCategoriaNavigation)
            .Where(p =>
                p.IdProducto == idProducto &&
                p.Activo)
            .Select(p => new ProductoDto
            {
                IdProducto = p.IdProducto,
                IdCategoria = p.IdCategoria,
                Categoria = p.IdCategoriaNavigation != null
                    ? p.IdCategoriaNavigation.Nombre
                    : null,
                Codigo = p.Codigo,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                ImagenUrl = p.ImagenUrl,
                Costo = p.Costo,
                PrecioVenta = p.PrecioVenta,
                Stock = p.Stock,
                StockMinimo = p.StockMinimo,
                Activo = p.Activo,
                FechaCreacion = p.FechaCreacion
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ProductoDto> CrearAsync(
        CrearProductoDto dto,
        int idUsuario)
    {
        string nombre = dto.Nombre.Trim();

        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException(
                "El nombre del producto es obligatorio."
            );

        string? codigo = string.IsNullOrWhiteSpace(dto.Codigo)
            ? null
            : dto.Codigo.Trim();

        if (codigo != null)
        {
            bool codigoExiste =
                await _context.Productos.AnyAsync(
                    p => p.Codigo == codigo
                );

            if (codigoExiste)
                throw new ArgumentException(
                    "Ya existe un producto con ese código."
                );
        }

        if (dto.IdCategoria.HasValue)
        {
            bool categoriaExiste =
                await _context.Categorias.AnyAsync(
                    c =>
                        c.IdCategoria == dto.IdCategoria.Value &&
                        c.Activo
                );

            if (!categoriaExiste)
                throw new ArgumentException(
                    "La categoría seleccionada no existe."
                );
        }

        var producto = new Producto
        {
            IdCategoria = dto.IdCategoria,
            Codigo = codigo,
            Nombre = nombre,
            Descripcion = string.IsNullOrWhiteSpace(
                dto.Descripcion)
                    ? null
                    : dto.Descripcion.Trim(),
            Costo = dto.Costo,
            PrecioVenta = dto.PrecioVenta,
            Stock = dto.StockInicial,
            StockMinimo = dto.StockMinimo,
            Activo = true,
            FechaCreacion = DateTime.Now
        };

        _context.Productos.Add(producto);

        await _context.SaveChangesAsync();

        if (dto.StockInicial > 0)
        {
            var movimiento =
                new MovimientosInventario
                {
                    IdProducto =
                        producto.IdProducto,
                    IdUsuario =
                        idUsuario,
                    TipoMovimiento =
                        "AJUSTE_ENTRADA",
                    Cantidad =
                        dto.StockInicial,
                    StockAnterior =
                        0,
                    StockNuevo =
                        dto.StockInicial,
                    Observacion =
                        "Stock inicial del producto.",
                    Fecha =
                        DateTime.Now
                };

            _context.MovimientosInventarios.Add(
                movimiento
            );

            await _context.SaveChangesAsync();
        }

        return await ObtenerPorIdAsync(
            producto.IdProducto
        ) ?? throw new InvalidOperationException(
            "No se pudo obtener el producto creado."
        );
    }

    public async Task<ProductoDto> ActualizarAsync(
        int idProducto,
        ActualizarProductoDto dto)
    {
        var producto =
            await _context.Productos
                .FirstOrDefaultAsync(
                    p =>
                        p.IdProducto == idProducto &&
                        p.Activo
                );

        if (producto == null)
            throw new KeyNotFoundException(
                "El producto no existe."
            );

        string nombre = dto.Nombre.Trim();

        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException(
                "El nombre del producto es obligatorio."
            );

        string? codigo = string.IsNullOrWhiteSpace(
            dto.Codigo)
                ? null
                : dto.Codigo.Trim();

        if (codigo != null)
        {
            bool codigoExiste =
                await _context.Productos.AnyAsync(
                    p =>
                        p.Codigo == codigo &&
                        p.IdProducto != idProducto
                );

            if (codigoExiste)
                throw new ArgumentException(
                    "Ya existe otro producto con ese código."
                );
        }

        if (dto.IdCategoria.HasValue)
        {
            bool categoriaExiste =
                await _context.Categorias.AnyAsync(
                    c =>
                        c.IdCategoria ==
                            dto.IdCategoria.Value &&
                        c.Activo
                );

            if (!categoriaExiste)
                throw new ArgumentException(
                    "La categoría seleccionada no existe."
                );
        }

        producto.IdCategoria =
            dto.IdCategoria;

        producto.Codigo =
            codigo;

        producto.Nombre =
            nombre;

        producto.Descripcion =
            string.IsNullOrWhiteSpace(
                dto.Descripcion)
                ? null
                : dto.Descripcion.Trim();

        producto.Costo =
            dto.Costo;

        producto.PrecioVenta =
            dto.PrecioVenta;

        producto.StockMinimo =
            dto.StockMinimo;

        await _context.SaveChangesAsync();

        return await ObtenerPorIdAsync(
            idProducto
        ) ?? throw new InvalidOperationException(
            "No se pudo obtener el producto actualizado."
        );
    }

    public async Task<bool> DesactivarAsync(
        int idProducto)
    {
        var producto =
            await _context.Productos
                .FirstOrDefaultAsync(
                    p =>
                        p.IdProducto == idProducto &&
                        p.Activo
                );

        if (producto == null)
            return false;

        producto.Activo = false;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<string> GuardarImagenAsync(
        int idProducto,
        IFormFile imagen)
    {
        var producto =
            await _context.Productos
                .FirstOrDefaultAsync(
                    p =>
                        p.IdProducto == idProducto &&
                        p.Activo
                );

        if (producto == null)
            throw new KeyNotFoundException(
                "El producto no existe."
            );

        if (imagen == null || imagen.Length == 0)
            throw new ArgumentException(
                "Debe seleccionar una imagen."
            );

        if (imagen.Length > 5 * 1024 * 1024)
            throw new ArgumentException(
                "La imagen no puede superar los 5 MB."
            );

        string extension =
            Path.GetExtension(
                    imagen.FileName
                )
                .ToLowerInvariant();

        string[] extensionesPermitidas =
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };

        if (!extensionesPermitidas.Contains(
            extension))
        {
            throw new ArgumentException(
                "Formato de imagen no permitido."
            );
        }

        string webRoot =
            _environment.WebRootPath
            ?? Path.Combine(
                _environment.ContentRootPath,
                "wwwroot"
            );

        string carpeta =
            Path.Combine(
                webRoot,
                "uploads",
                "productos"
            );

        Directory.CreateDirectory(carpeta);

        if (!string.IsNullOrWhiteSpace(
            producto.ImagenUrl))
        {
            string imagenAnterior =
                producto.ImagenUrl
                    .TrimStart('/')
                    .Replace(
                        '/',
                        Path.DirectorySeparatorChar
                    );

            string rutaAnterior =
                Path.Combine(
                    webRoot,
                    imagenAnterior
                );

            if (File.Exists(rutaAnterior))
                File.Delete(rutaAnterior);
        }

        string nombreArchivo =
            $"{Guid.NewGuid()}{extension}";

        string rutaArchivo =
            Path.Combine(
                carpeta,
                nombreArchivo
            );

        await using var stream =
            new FileStream(
                rutaArchivo,
                FileMode.Create
            );

        await imagen.CopyToAsync(stream);

        producto.ImagenUrl =
            $"/uploads/productos/{nombreArchivo}";

        await _context.SaveChangesAsync();

        return producto.ImagenUrl;
    }

    public async Task<bool> EliminarImagenAsync(
        int idProducto)
    {
        var producto =
            await _context.Productos
                .FirstOrDefaultAsync(
                    p =>
                        p.IdProducto == idProducto &&
                        p.Activo
                );

        if (producto == null)
            throw new KeyNotFoundException(
                "El producto no existe."
            );

        if (string.IsNullOrWhiteSpace(
            producto.ImagenUrl))
        {
            return false;
        }

        string webRoot =
            _environment.WebRootPath
            ?? Path.Combine(
                _environment.ContentRootPath,
                "wwwroot"
            );

        string rutaRelativa =
            producto.ImagenUrl
                .TrimStart('/')
                .Replace(
                    '/',
                    Path.DirectorySeparatorChar
                );

        string rutaCompleta =
            Path.Combine(
                webRoot,
                rutaRelativa
            );

        if (File.Exists(rutaCompleta))
            File.Delete(rutaCompleta);

        producto.ImagenUrl = null;

        await _context.SaveChangesAsync();

        return true;
    }
}