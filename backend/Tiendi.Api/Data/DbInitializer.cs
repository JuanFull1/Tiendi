using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Tiendi.Api.Models;

namespace Tiendi.Api.Data;

public static class DbInitializer
{
    public static async Task SeedAdminAsync(
        IServiceProvider services
    )
    {
        using IServiceScope scope = services.CreateScope();

        TiendiDbContext context =
            scope.ServiceProvider
                .GetRequiredService<TiendiDbContext>();

        IConfiguration configuration =
            scope.ServiceProvider
                .GetRequiredService<IConfiguration>();

        PasswordHasher<Usuario> passwordHasher =
            scope.ServiceProvider
                .GetRequiredService<PasswordHasher<Usuario>>();

        string email =
            configuration["SeedAdmin:Email"]
            ?? "admin@tiendi.local";

        string nombre =
            configuration["SeedAdmin:Nombre"]
            ?? "Administrador";

        string apellido =
            configuration["SeedAdmin:Apellido"]
            ?? "Local";

        string? password =
            configuration["SeedAdmin:Password"];

        if (string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        bool existe = await context.Usuarios
            .AnyAsync(u => u.Email == email);

        if (existe)
        {
            return;
        }

        var usuario = new Usuario
        {
            Nombre = nombre,
            Apellido = apellido,
            Email = email,
            Rol = "ADMIN",
            Activo = true,
            FechaCreacion = DateTime.Now,
            PasswordHash = string.Empty
        };

        usuario.PasswordHash =
            passwordHasher.HashPassword(
                usuario,
                password
            );

        context.Usuarios.Add(usuario);

        await context.SaveChangesAsync();
    }
}