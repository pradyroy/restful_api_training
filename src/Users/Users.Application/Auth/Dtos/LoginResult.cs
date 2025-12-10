namespace Users.Application.Auth.Dtos;

public sealed class LoginResult
{
    public bool Success { get; init; }
    public string? Error { get; init; }
    public long? UserId { get; init; }
    public string? UserName { get; init; }
    public string? Role { get; init; }
}
