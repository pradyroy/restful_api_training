using Users.Domain.Entities;

namespace Users.Application.Contracts;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<User>> GetPagedAsync(int skip, int take, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<User>> GetFilteredAsync(
        string? userName,
        string? role,
        string? emailId,
        string? mobileNum,
        int? skip,
        int? take,
        CancellationToken cancellationToken = default);

    Task<User> AddAsync(User user, CancellationToken cancellationToken = default);

    Task UpdateAsync(User user, CancellationToken cancellationToken = default);

    Task DeleteAsync(User user, CancellationToken cancellationToken = default);

    Task<bool> UserNameExistsAsync(string userName, long? excludeUserId = null, CancellationToken cancellationToken = default);
}
