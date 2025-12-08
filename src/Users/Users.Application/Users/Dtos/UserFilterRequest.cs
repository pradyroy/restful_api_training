namespace Users.Application.Users.Dtos;

public sealed class UserFilterRequest
{
    public string? UserName { get; init; }
    public string? Role { get; init; }
    public string? EmailId { get; init; }
    public string? MobileNum { get; init; }

    public int? Skip { get; init; }
    public int? Take { get; init; }
}
