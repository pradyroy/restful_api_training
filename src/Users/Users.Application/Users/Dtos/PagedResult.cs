namespace Users.Application.Users.Dtos;

public sealed class PagedResult<T>
{
    public required IReadOnlyList<T> Items { get; init; }
    public int TotalCount { get; init; }
    public int Skip { get; init; }
    public int Take { get; init; }

    // Computed property, will be serialized as "hasMore" (camelCase) by default
    public bool HasMore => Skip + Take < TotalCount;
}
