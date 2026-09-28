namespace Tiendi.Api.DTOs;

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;

    public UsuarioActualDto Usuario { get; set; } = new();
}