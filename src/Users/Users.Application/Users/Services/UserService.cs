using System.Security.Cryptography;
using System.Text;
using Users.Application.Contracts;
using Users.Application.Users.Dtos;
using Users.Domain.Entities;
using Users.Domain.Enums;

namespace Users.Application.Users.Services;

public sealed class UserService
{
    private readonly IUserRepository _repository;

    public UserService(IUserRepository repository)
    {
        _repository = repository;
    }

    // Simple SHA256 for demo; in real life use BCrypt/PBKDF2/Argon2.
    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static UserDto ToDto(User user) => new()
    {
        Id = user.Id,
        UserName = user.UserName,
        FullName = user.FullName,
        Role = user.Role.ToString(),
        EmailId = user.EmailId,
        MobileNum = user.MobileNum,
        ProfilePicUrl = user.ProfilePicUrl,
        CreatedAt = user.CreatedAt
    };

    public async Task<UserDto?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var user = await _repository.GetByIdAsync(id, ct);
        return user is null ? null : ToDto(user);
    }

    public async Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken ct = default)
    {
        var users = await _repository.GetAllAsync(ct);
        return users.Select(ToDto).ToList();
    }

    // 👇 CHANGED: now returns PagedResult<UserDto>
    public async Task<PagedResult<UserDto>> GetPagedAsync(int skip, int take, CancellationToken ct = default)
    {
        var users = await _repository.GetPagedAsync(skip, take, ct);
        var totalCount = await _repository.GetTotalCountAsync(ct);

        var items = users.Select(ToDto).ToList();

        return new PagedResult<UserDto>
        {
            Items = items,
            TotalCount = totalCount,
            Skip = skip,
            Take = take
        };
    }

    // Existing filtered method (no paging OR paging via flag)
    public async Task<IReadOnlyList<UserDto>> GetFilteredAsync(
        UserFilterRequest filter,
        bool paged,
        CancellationToken ct = default)
    {
        var users = await _repository.GetFilteredAsync(
            filter.UserName,
            filter.Role,
            filter.EmailId,
            filter.MobileNum,
            paged ? filter.Skip : null,
            paged ? filter.Take : null,
            ct);

        return users.Select(ToDto).ToList();
    }

    // 👇 NEW: filtered + paged, with metadata
    public async Task<PagedResult<UserDto>> GetFilteredPagedAsync(
        UserFilterRequest filter,
        CancellationToken ct = default)
    {
        var users = await _repository.GetFilteredAsync(
            filter.UserName,
            filter.Role,
            filter.EmailId,
            filter.MobileNum,
            filter.Skip,
            filter.Take,
            ct);

        var totalCount = await _repository.GetFilteredCountAsync(
            filter.UserName,
            filter.Role,
            filter.EmailId,
            filter.MobileNum,
            ct);

        var items = users.Select(ToDto).ToList();

        var skip = filter.Skip ?? 0;
        var take = filter.Take ?? items.Count;

        return new PagedResult<UserDto>
        {
            Items = items,
            TotalCount = totalCount,
            Skip = skip,
            Take = take
        };
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken ct = default)
    {
        // Basic validation for demo
        if (string.IsNullOrWhiteSpace(request.UserName))
            throw new ArgumentException("UserName is required.", nameof(request.UserName));

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new ArgumentException("Password is required.", nameof(request.Password));

        var exists = await _repository.UserNameExistsAsync(request.UserName, null, ct);
        if (exists)
            throw new InvalidOperationException($"UserName '{request.UserName}' is already taken.");

        if (!Enum.TryParse<UserRole>(request.Role, ignoreCase: true, out var role))
        {
            role = UserRole.ReadOnly;
        }

        var passwordHash = HashPassword(request.Password);

        var user = new User(
            userName: request.UserName,
            passwordHash: passwordHash,
            fullName: request.FullName,
            role: role,
            emailId: request.EmailId,
            mobileNum: request.MobileNum
        );

        var created = await _repository.AddAsync(user, ct);
        return ToDto(created);
    }

    public async Task<UserDto?> UpdateAsync(long id, UpdateUserRequest request, CancellationToken ct = default)
    {
        var user = await _repository.GetByIdAsync(id, ct);
        if (user is null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(request.FullName))
            user.UpdateFullName(request.FullName);

        if (!string.IsNullOrWhiteSpace(request.Role)
            && Enum.TryParse<UserRole>(request.Role, ignoreCase: true, out var newRole))
        {
            user.UpdateRole(newRole);
        }

        if (!string.IsNullOrWhiteSpace(request.EmailId))
            user.UpdateEmail(request.EmailId);

        if (!string.IsNullOrWhiteSpace(request.MobileNum))
            user.UpdateMobile(request.MobileNum);

        await _repository.UpdateAsync(user, ct);

        return ToDto(user);
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken ct = default)
    {
        var user = await _repository.GetByIdAsync(id, ct);
        if (user is null)
        {
            return false;
        }

        await _repository.DeleteAsync(user, ct);
        return true;
    }

    public async Task<UserDto?> UpdateProfileFieldAsync(
        long id,
        string fieldName,
        string value,
        CancellationToken ct = default)
    {
        var user = await _repository.GetByIdAsync(id, ct);
        if (user is null)
        {
            return null;
        }

        // For now, only profile_pic_url is supported, but the signature is generic for training.
        if (string.Equals(fieldName, "profile_pic_url", StringComparison.OrdinalIgnoreCase))
        {
            user.UpdateProfilePicUrl(value);
        }

        await _repository.UpdateAsync(user, ct);

        return ToDto(user);
    }
}
