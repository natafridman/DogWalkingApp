using DogWalking.Application.DTOs;
using DogWalking.Application.Interfaces;
using DogWalking.Domain.Entities;
using DogWalking.Domain.Enums;
using DogWalking.Domain.Interfaces;

namespace DogWalking.Application.Services;

public class DogService : IDogService
{
    private readonly IUnitOfWork _uow;

    public DogService(IUnitOfWork uow) => _uow = uow;

    public async Task<IEnumerable<DogDto>> GetByClientIdAsync(int clientId, CancellationToken ct = default)
    {
        var dogs = await _uow.Dogs.GetByClientIdAsync(clientId, ct);
        return dogs.Select(MapToDto);
    }

    public async Task<DogDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var dog = await _uow.Dogs.GetByIdAsync(id, ct);
        return dog is null ? null : MapToDto(dog);
    }

    public async Task<DogDto> CreateAsync(CreateDogDto dto, CancellationToken ct = default)
    {
        // Verify owner exists
        _ = await _uow.Clients.GetByIdAsync(dto.ClientId, ct)
            ?? throw new KeyNotFoundException($"Client {dto.ClientId} not found.");

        var dog = new Dog(dto.ClientId, dto.Name, dto.Breed, dto.BirthDate);
        await _uow.Dogs.AddAsync(dog, ct);
        await _uow.CommitAsync(ct);

        // Re-fetch to get navigation property populated
        var saved = await _uow.Dogs.GetByIdAsync(dog.Id, ct);
        return MapToDto(saved!);
    }

    public async Task<DogDto> UpdateAsync(UpdateDogDto dto, CancellationToken ct = default)
    {
        var dog = await _uow.Dogs.GetByIdAsync(dto.Id, ct)
            ?? throw new KeyNotFoundException($"Dog {dto.Id} not found.");

        dog.Update(dto.Name, dto.Breed, dto.BirthDate);
        _uow.Dogs.Update(dog);
        await _uow.CommitAsync(ct);
        return MapToDto(dog);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var dog = await _uow.Dogs.GetByIdWithWalksAsync(id, ct)
            ?? throw new KeyNotFoundException($"Dog {id} not found.");

        // Cascade-delete all associated walk events before removing the dog
        foreach (var walk in dog.WalkEvents.ToList())
            _uow.WalkEvents.Remove(walk);

        _uow.Dogs.Remove(dog);
        await _uow.CommitAsync(ct);
    }

    private static DogDto MapToDto(Dog d) => new(
        d.Id, d.ClientId, d.Client?.Name ?? string.Empty, d.Name, d.Breed, d.BirthDate
    );
}
