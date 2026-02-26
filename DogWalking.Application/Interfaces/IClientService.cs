using DogWalking.Application.DTOs;
using DogWalking.Domain.Enums;

namespace DogWalking.Application.Interfaces;

public interface IClientService
{
    Task<ClientDto> CreateAsync(CreateClientDto dto, CancellationToken ct = default);
    Task<ClientDto> UpdateAsync(UpdateClientDto dto, CancellationToken ct = default);
    Task<ClientDto> ChangeSubscriptionAsync(ChangeSubscriptionDto dto, CancellationToken ct = default);
    Task<ClientDto> UpdateContactInfoAsync(int clientId, string phone, string email, CancellationToken ct = default);
}
