using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Tiendi.Api.Models;

namespace Tiendi.Api.Services;

public class JwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string CrearToken(Usuario usuario)
    {
        string key = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("No se configuró Jwt:Key.");

        string issuer = _configuration["Jwt:Issuer"] ?? "Tiendi.Api";

        string audience = _configuration["Jwt:Audience"] ?? "Tiendi.Frontend";

        int expirationMinutes =
            int.TryParse(
                _configuration["Jwt:ExpirationMinutes"],
                out int minutes
            )
                ? minutes
                : 120;

        var claims = new List<Claim>
        {
            new(
                ClaimTypes.NameIdentifier,
                usuario.IdUsuario.ToString()
            ),

            new(
                ClaimTypes.Name,
                $"{usuario.Nombre} {usuario.Apellido}".Trim()
            ),

            new(
                ClaimTypes.Email,
                usuario.Email
            ),

            new(
                ClaimTypes.Role,
                usuario.Rol
            ),

            new(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString()
            )
        };

        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(key)
        );

        var credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256
        );

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}