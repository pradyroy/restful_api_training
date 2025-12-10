namespace Users.Application.Auth.Dtos;

public sealed class LoginRequest
{
    public string UserName { get; init; } = default!;
    public string Password { get; init; } = default!;
}
