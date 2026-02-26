using DogWalking.Application.DTOs;
using DogWalking.Domain.Enums;

namespace DogWalking.Application.Interfaces;

public interface IClientService
{
    Task<ClientDto> CreateAsync(CreateClientDto dto, CancellationToken ct = default);
    Task<ClientDto> UpdateAsync(UpdateClientDto dto, CancellationToken ct = default);
    Task<ClientDto> ChangeSubscriptionAsync(ChangeSubscriptionDto dto, CancellationToken ct = default);
    Task<ClientDto> UpdateContactInfoAsync(int clientId, string phone, string email, CancellationToken ct = default);
    Task<IEnumerable<ClientDto>> GetAllActiveAsync(CancellationToken ct = default);
    Task<IEnumerable<ClientDto>> SearchAsync(string term, CancellationToken ct = default);
    Task<ClientDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<ClientDto?> GetByUserIdAsync(int userId, CancellationToken ct = default);
}
