using Microsoft.EntityFrameworkCore;
using Users.Application.Contracts;
using Users.Domain.Entities;
using Users.Infrastructure.Persistence;

namespace Users.Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly UsersDbContext _dbContext;

    public UserRepository(UsersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<User?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .OrderBy(u => u.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetPagedAsync(
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .OrderBy(u => u.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetFilteredAsync(
        string? userName,
        string? role,
        string? emailId,
        string? mobileNum,
        int? skip,
        int? take,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Users.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(userName))
            query = query.Where(u => u.UserName.Contains(userName));

        if (!string.IsNullOrWhiteSpace(role))
            query = query.Where(u => u.Role.ToString() == role);

        if (!string.IsNullOrWhiteSpace(emailId))
            query = query.Where(u => u.EmailId.Contains(emailId));

        if (!string.IsNullOrWhiteSpace(mobileNum))
            query = query.Where(u => u.MobileNum.Contains(mobileNum));

        query = query.OrderBy(u => u.Id);

        if (skip.HasValue)
            query = query.Skip(skip.Value);
        if (take.HasValue)
            query = query.Take(take.Value);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(User user, CancellationToken cancellationToken = default)
    {
        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> UserNameExistsAsync(
        string userName,
        long? excludeUserId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Users.AsQueryable()
            .Where(u => u.UserName == userName);

        if (excludeUserId.HasValue)
            query = query.Where(u => u.Id != excludeUserId.Value);

        return query.AnyAsync(cancellationToken);
    }

    public Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.Users.CountAsync(cancellationToken);
    }

    public Task<int> GetFilteredCountAsync(
        string? userName,
        string? role,
        string? emailId,
        string? mobileNum,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(userName))
            query = query.Where(u => u.UserName.Contains(userName));

        if (!string.IsNullOrWhiteSpace(role))
            query = query.Where(u => u.Role.ToString() == role);

        if (!string.IsNullOrWhiteSpace(emailId))
            query = query.Where(u => u.EmailId.Contains(emailId));

        if (!string.IsNullOrWhiteSpace(mobileNum))
            query = query.Where(u => u.MobileNum.Contains(mobileNum));

        return query.CountAsync(cancellationToken);
    }

    public Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        return _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserName == userName, cancellationToken);
    }
}
