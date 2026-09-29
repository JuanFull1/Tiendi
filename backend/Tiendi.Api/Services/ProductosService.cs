using Microsoft.EntityFrameworkCore;
using Tiendi.Api.Data;

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

    public async Task<string> GuardarImagenAsync(
        int idProducto,
        IFormFile imagen)
    {
        var producto = await _context.Productos
            .FirstOrDefaultAsync(p => p.IdProducto == idProducto);

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
            Path.GetExtension(imagen.FileName)
                .ToLowerInvariant();

        string[] extensionesPermitidas =
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };

        if (!extensionesPermitidas.Contains(extension))
            throw new ArgumentException(
                "Formato de imagen no permitido."
            );

        string carpeta = Path.Combine(
            _environment.WebRootPath
                ?? Path.Combine(
                    _environment.ContentRootPath,
                    "wwwroot"
                ),
            "uploads",
            "productos"
        );

        Directory.CreateDirectory(carpeta);

        if (!string.IsNullOrWhiteSpace(producto.ImagenUrl))
        {
            string imagenAnterior =
                producto.ImagenUrl
                    .TrimStart('/')
                    .Replace('/', Path.DirectorySeparatorChar);

            string rutaAnterior = Path.Combine(
                _environment.WebRootPath
                    ?? Path.Combine(
                        _environment.ContentRootPath,
                        "wwwroot"
                    ),
                imagenAnterior
            );

            if (File.Exists(rutaAnterior))
                File.Delete(rutaAnterior);
        }

        string nombreArchivo =
            $"{Guid.NewGuid()}{extension}";

        string rutaArchivo = Path.Combine(
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
        var producto = await _context.Productos
            .FirstOrDefaultAsync(
                p => p.IdProducto == idProducto
            );

        if (producto == null)
            throw new KeyNotFoundException(
                "El producto no existe."
            );

        if (string.IsNullOrWhiteSpace(
            producto.ImagenUrl))
            return false;

        string rutaRelativa =
            producto.ImagenUrl
                .TrimStart('/')
                .Replace(
                    '/',
                    Path.DirectorySeparatorChar
                );

        string rutaCompleta = Path.Combine(
            _environment.WebRootPath
                ?? Path.Combine(
                    _environment.ContentRootPath,
                    "wwwroot"
                ),
            rutaRelativa
        );

        if (File.Exists(rutaCompleta))
            File.Delete(rutaCompleta);

        producto.ImagenUrl = null;

        await _context.SaveChangesAsync();

        return true;
    }
}