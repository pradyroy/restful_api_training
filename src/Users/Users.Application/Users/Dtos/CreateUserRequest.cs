namespace Users.Application.Users.Dtos;

public sealed class CreateUserRequest
{
    public string UserName { get; init; } = default!;
    public string Password { get; init; } = default!;
    public string FullName { get; init; } = default!;
    public string Role { get; init; } = "ReadOnly"; // "Admin" / "ReadOnly"
    public string EmailId { get; init; } = default!;
    public string MobileNum { get; init; } = default!;
}
