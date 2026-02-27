using DogWalking.Domain.Entities;
using DogWalking.Domain.Enums;

namespace DogWalking.Domain.Interfaces;

public interface IWalkEventRepository
{
    Task<WalkEvent?>              GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<WalkEvent>>  GetByDogIdAsync(int dogId, CancellationToken ct = default);
    Task<IEnumerable<WalkEvent>>  GetByStatusAsync(WalkStatus status, CancellationToken ct = default);
    Task<IEnumerable<WalkEvent>>  GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<IEnumerable<WalkEvent>>  GetByWalkerIdAsync(int walkerId, CancellationToken ct = default);
    Task<IEnumerable<WalkEvent>>  GetByClientIdAsync(int clientId, CancellationToken ct = default);

    /// <summary>
    /// Returns all walks for ALL dogs belonging to a given client in a specific month.
    /// Used by the Strategy Pattern to enforce monthly subscription limits.
    /// </summary>
    Task<IEnumerable<WalkEvent>>  GetByClientAndMonthAsync(int clientId, int year, int month,
                                                           CancellationToken ct = default);

    /// <summary>
    /// Server-side count of active walks for subscription limit display.
    /// Avoids loading full entities when only the count is needed.
    /// </summary>
    Task<int> CountActiveByClientAndMonthAsync(int clientId, int year, int month,
                                               CancellationToken ct = default);

    /// <summary>
    /// Returns a paged subset of walks filtered by status and optional search term,
    /// with total count for UI pagination.
    /// </summary>
    Task<(IEnumerable<WalkEvent> Items, int TotalCount)> GetByStatusPagedAsync(
        WalkStatus status, int page, int pageSize, string? search = null,
        CancellationToken ct = default);

    Task AddAsync(WalkEvent walkEvent, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<WalkEvent> walkEvents, CancellationToken ct = default);
    void Update(WalkEvent walkEvent);
    void Remove(WalkEvent walkEvent);
}
