using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Tiendi.Api.Data;
using Tiendi.Api.Models;
using Tiendi.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddDbContext<TiendiDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString(
            "DefaultConnection"
        )
    );
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddScoped<PasswordHasher<Usuario>>();

builder.Services.AddScoped<JwtService>();

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ClientesService>();

builder.Services.AddScoped<ProveedoresService>();

builder.Services.AddScoped<ComprasService>();
builder.Services.AddScoped<EstadisticasService>();

builder.Services.AddScoped<BalanceService>();
builder.Services.AddScoped<ProductosService>();
builder.Services.AddScoped<InventarioService>();

string jwtKey =
    builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "No se configuró Jwt:Key."
    );

string jwtIssuer =
    builder.Configuration["Jwt:Issuer"]
    ?? "Tiendi.Api";

string jwtAudience =
    builder.Configuration["Jwt:Audience"]
    ?? "Tiendi.Frontend";

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme
    )
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,

                ValidateAudience = true,

                ValidateLifetime = true,

                ValidateIssuerSigningKey = true,

                ValidIssuer = jwtIssuer,

                ValidAudience = jwtAudience,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey)
                    ),

                ClockSkew = TimeSpan.Zero
            };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

await DbInitializer.SeedAdminAsync(app.Services);

app.UseCors("Frontend");

app.UseStaticFiles();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();