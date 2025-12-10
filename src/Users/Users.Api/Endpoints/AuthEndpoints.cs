using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Users.Api;
using Users.Application.Auth;
using Users.Application.Auth.Dtos;

namespace Users.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth");

        group.MapPost("/login", async (
            [FromBody] LoginRequest request,
            IAuthService authService,
            IOptions<JwtSettings> jwtOptions) =>
        {
            var loginResult = await authService.LoginAsync(request);

            if (!loginResult.Success || loginResult.UserId is null)
            {
                return Results.Unauthorized();
            }

            var jwtSettings = jwtOptions.Value;
            var token = GenerateJwtToken(
                loginResult.UserId.Value,
                loginResult.UserName!,
                loginResult.Role!,
                jwtSettings);

            return Results.Ok(new
            {
                access_token = token,
                token_type = "Bearer",
                expires_in_minutes = jwtSettings.ExpiresInMinutes,
                user = new
                {
                    id = loginResult.UserId,
                    userName = loginResult.UserName,
                    role = loginResult.Role
                }
            });
        })
        .WithName("Login")
        .WithSummary("Authenticate user and return JWT token");

        return app;
    }

    private static string GenerateJwtToken(
        long userId,
        string userName,
        string role,
        JwtSettings settings)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, userName),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, userName),
            new(ClaimTypes.Role, role)
        };

        var now = DateTime.UtcNow;

        var token = new JwtSecurityToken(
            issuer: settings.Issuer,
            audience: settings.Audience,
            claims: claims,
            notBefore: now,
            expires: now.AddMinutes(settings.ExpiresInMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
