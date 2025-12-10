using System.Security.Cryptography;
using System.Text;
using Users.Application.Auth.Dtos;
using Users.Application.Contracts;

namespace Users.Application.Auth;

public interface IAuthService
{
    Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;

    public AuthService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByUserNameAsync(request.UserName, cancellationToken);
        if (user is null)
        {
            return new LoginResult
            {
                Success = false,
                Error = "Invalid username or password."
            };
        }

        // Demo password verification: SHA256 of plain-text password
        var incomingHash = ComputeSha256(request.Password);

        if (!string.Equals(user.PasswordHash, incomingHash, StringComparison.Ordinal))
        {
            return new LoginResult
            {
                Success = false,
                Error = "Invalid username or password."
            };
        }

        return new LoginResult
        {
            Success = true,
            UserId = user.Id,
            UserName = user.UserName,
            Role = user.Role.ToString()
        };
    }

    private static string ComputeSha256(string input)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = sha.ComputeHash(bytes);
        // Match UserService.HashPassword exactly:
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
