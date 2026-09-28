using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tiendi.Api.DTOs;
using Tiendi.Api.Services;

namespace Tiendi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginDto dto
    )
    {
        if (
            string.IsNullOrWhiteSpace(dto.Email) ||
            string.IsNullOrWhiteSpace(dto.Password)
        )
        {
            return BadRequest(new
            {
                mensaje = "Email y contraseña son obligatorios."
            });
        }

        LoginResponseDto? resultado =
            await _authService.LoginAsync(dto);

        if (resultado is null)
        {
            return Unauthorized(new
            {
                mensaje = "Correo o contraseña incorrectos."
            });
        }

        return Ok(resultado);
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        string? id =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        string? nombre =
            User.FindFirstValue(ClaimTypes.Name);

        string? email =
            User.FindFirstValue(ClaimTypes.Email);

        string? rol =
            User.FindFirstValue(ClaimTypes.Role);

        return Ok(new
        {
            idUsuario = id,
            nombre,
            email,
            rol
        });
    }
}