using DogWalking.Application.DTOs;

namespace DogWalking.Application.Interfaces;

public interface IDogService
{
    Task<IEnumerable<DogDto>> GetByClientIdAsync(int clientId, CancellationToken ct = default);
    Task<DogDto?>             GetByIdAsync(int id, CancellationToken ct = default);
    Task<DogDto>              CreateAsync(CreateDogDto dto, CancellationToken ct = default);
    Task<DogDto>              UpdateAsync(UpdateDogDto dto, CancellationToken ct = default);
    Task                      DeleteAsync(int id, CancellationToken ct = default);
}
