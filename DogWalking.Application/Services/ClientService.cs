using DogWalking.Application.DTOs;
using DogWalking.Application.Interfaces;
using DogWalking.Domain.Entities;
using DogWalking.Domain.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DogWalking.Application.Services;

public class ClientService : IClientService
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<ClientService> _log;
    private readonly IValidator<CreateClientDto> _clientValidator;

    public ClientService(IUnitOfWork uow, ILogger<ClientService> log,
                         IValidator<CreateClientDto> clientValidator)
    {
        _uow = uow;
        _log = log;
        _clientValidator = clientValidator;
    }

    public async Task<IEnumerable<ClientDto>> GetAllActiveAsync(CancellationToken ct = default)
    {
        var clients = await _uow.Clients.GetAllActiveAsync(ct);
        return clients.Select(MapToDto);
    }

    public async Task<IEnumerable<ClientDto>> SearchAsync(string term, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(term))
            return await GetAllActiveAsync(ct);

        var clients = await _uow.Clients.SearchAsync(term.Trim(), ct);
        return clients.Select(MapToDto);
    }

    public async Task<ClientDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var client = await _uow.Clients.GetByIdWithDogsAsync(id, ct);
        return client is null ? null : MapToDto(client);
    }

    public async Task<ClientDto?> GetByUserIdAsync(int userId, CancellationToken ct = default)
    {
        var client = await _uow.Clients.GetByUserIdAsync(userId, ct);
        return client is null ? null : MapToDto(client);
    }

    public async Task<ClientDto> CreateAsync(CreateClientDto dto, CancellationToken ct = default)
    {
        await _clientValidator.ValidateAndThrowAsync(dto, ct);

        var client = new Client(dto.Name, dto.PhoneNumber, dto.Email, dto.Subscription,
                                zone: dto.Zone, address: dto.Address);
        await _uow.Clients.AddAsync(client, ct);
        await _uow.CommitAsync(ct);
        _log.LogInformation("Client created: {Name} ({Subscription})", dto.Name, dto.Subscription);
        return MapToDto(client);
    }

    public async Task<ClientDto> UpdateAsync(UpdateClientDto dto, CancellationToken ct = default)
    {
        var client = await _uow.Clients.GetByIdAsync(dto.Id, ct)
            ?? throw new KeyNotFoundException($"Client {dto.Id} not found.");

        client.Update(dto.Name, dto.PhoneNumber, dto.Email, dto.Zone);
        client.ChangeSubscription(dto.Subscription);
        _uow.Clients.Update(client);
        await _uow.CommitAsync(ct);
        return MapToDto(client);
    }

    public async Task<ClientDto> ChangeSubscriptionAsync(ChangeSubscriptionDto dto,
                                                          CancellationToken ct = default)
    {
        var client = await _uow.Clients.GetByIdAsync(dto.ClientId, ct)
            ?? throw new KeyNotFoundException($"Client {dto.ClientId} not found.");

        client.ChangeSubscription(dto.NewSubscription);
        _uow.Clients.Update(client);
        await _uow.CommitAsync(ct);
        _log.LogInformation("Client {ClientId} subscription changed to {Plan}",
            dto.ClientId, dto.NewSubscription);
        return MapToDto(client);
    }

    public async Task<ClientDto> UpdateContactInfoAsync(int clientId, string phone, string email,
                                                        CancellationToken ct = default)
    {
        var client = await _uow.Clients.GetByIdAsync(clientId, ct)
            ?? throw new KeyNotFoundException($"Client {clientId} not found.");
        client.Update(client.Name, phone, email, client.Zone);
        _uow.Clients.Update(client);
        await _uow.CommitAsync(ct);
        return MapToDto(client);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var client = await _uow.Clients.GetByIdWithDogsAsync(id, ct)
            ?? throw new KeyNotFoundException($"Client {id} not found.");

        // Domain entity enforces the no-active-walks rule
        client.Deactivate();

        _uow.Clients.Update(client);
        await _uow.CommitAsync(ct);
    }

    private static ClientDto MapToDto(Client c) => new(
        c.Id,
        c.Name,
        c.PhoneNumber.Value,
        c.Email,
        c.Subscription,
        c.IsActive,
        c.Dogs.Count,
        c.Zone,
        c.Address
    );
}
