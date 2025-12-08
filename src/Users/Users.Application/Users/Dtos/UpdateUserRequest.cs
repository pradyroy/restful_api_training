namespace Users.Application.Users.Dtos;

public sealed class UpdateUserRequest
{
    public string? FullName { get; init; }
    public string? Role { get; init; }
    public string? EmailId { get; init; }
    public string? MobileNum { get; init; }
}
