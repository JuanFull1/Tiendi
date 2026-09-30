using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Tiendi.Api.Data;
using Tiendi.Api.DTOs;
using Tiendi.Api.Models;

namespace Tiendi.Api.Services;

public class AuthService
{
    private readonly TiendiDbContext _context;
    private readonly PasswordHasher<Usuario> _passwordHasher;
    private readonly JwtService _jwtService;

    public AuthService(
        TiendiDbContext context,
        PasswordHasher<Usuario> passwordHasher,
        JwtService jwtService
    )
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginDto dto)
    {
        string email = dto.Email.Trim();

        Usuario? usuario = await _context.Usuarios
            .FirstOrDefaultAsync(
                u => u.Email == email && u.Activo
            );

        if (usuario is null)
        {
            return null;
        }

        PasswordVerificationResult resultado =
            _passwordHasher.VerifyHashedPassword(
                usuario,
                usuario.PasswordHash,
                dto.Password
            );

        if (resultado == PasswordVerificationResult.Failed)
        {
            return null;
        }

        string token = _jwtService.CrearToken(usuario);

        return new LoginResponseDto
        {
            Token = token,

            Usuario = new UsuarioActualDto
            {
                IdUsuario = usuario.IdUsuario,

                Nombre = $"{usuario.Nombre} {usuario.Apellido}".Trim(),

                Email = usuario.Email,

                Rol = usuario.Rol
            }
        };
    }
}