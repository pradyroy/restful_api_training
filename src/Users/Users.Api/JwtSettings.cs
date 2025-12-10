namespace Users.Api;

public sealed class JwtSettings
{
    public string Key { get; init; } = default!;
    public string Issuer { get; init; } = default!;
    public string Audience { get; init; } = default!;
    public int ExpiresInMinutes { get; init; } = 60;
}
