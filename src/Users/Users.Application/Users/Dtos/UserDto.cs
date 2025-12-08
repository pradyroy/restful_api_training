namespace Users.Application.Users.Dtos;

public sealed class UserDto
{
    public long Id { get; init; }
    public string UserName { get; init; } = default!;
    public string FullName { get; init; } = default!;
    public string Role { get; init; } = default!; // "Admin" / "ReadOnly"
    public string EmailId { get; init; } = default!;
    public string MobileNum { get; init; } = default!;
    public string? ProfilePicUrl { get; init; }
    public DateTime CreatedAt { get; init; }
}
