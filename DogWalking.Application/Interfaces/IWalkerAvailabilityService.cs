using DogWalking.Application.DTOs;

namespace DogWalking.Application.Interfaces;

public interface IWalkerAvailabilityService
{
    Task<IEnumerable<WalkerAvailabilityDto>> GetByWalkerIdAsync(int walkerId, CancellationToken ct = default);

    Task<WalkerAvailabilityDto> AddAvailabilityAsync(CreateAvailabilityDto dto, CancellationToken ct = default);
    Task                        DeleteAvailabilityAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Finds walkers eligible to handle the given walk request
    /// (availability slot covers the time AND zone matches, no conflicts).
    /// </summary>
    Task<IEnumerable<WalkerMatchDto>> FindEligibleWalkersAsync(int walkEventId, CancellationToken ct = default);
}
