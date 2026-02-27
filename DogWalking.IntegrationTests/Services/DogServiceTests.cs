using DogWalking.Application.DTOs;

namespace DogWalking.IntegrationTests.Services;

public class DogServiceTests : IntegrationTestBase
{
    [Fact]
    public async Task CreateAsync_ValidData_PersistsWithClientRef()
    {
        var client = await SeedClientAsync();
        var svc = CreateDogService();

        var result = await svc.CreateAsync(
            new CreateDogDto(client.Id, "Rex", "German Shepherd", DateOnly.FromDateTime(DateTime.Today.AddYears(-2))));

        Assert.True(result.Id > 0);
        Assert.Equal("Rex", result.Name);
        Assert.Equal("German Shepherd", result.Breed);
        Assert.Equal(client.Id, result.ClientId);
    }

    [Fact]
    public async Task UpdateAsync_ChangesFields()
    {
        var client = await SeedClientAsync();
        var dog = await SeedDogAsync(client.Id, "Rex", "Labrador");
        var svc = CreateDogService();

        var result = await svc.UpdateAsync(
            new UpdateDogDto(dog.Id, "Max", "Poodle", DateOnly.FromDateTime(DateTime.Today.AddYears(-1))));

        Assert.Equal("Max", result.Name);
        Assert.Equal("Poodle", result.Breed);
    }

    [Fact]
    public async Task DeleteAsync_CascadeDeletesWalkEvents()
    {
        var client = await SeedClientAsync();
        var dog = await SeedDogAsync(client.Id);
        await SeedWalkEventAsync(dog.Id);
        await SeedWalkEventAsync(dog.Id, DateTime.UtcNow.AddDays(2));
        var svc = CreateDogService();

        await svc.DeleteAsync(dog.Id);

        var deletedDog = await Uow.Dogs.GetByIdAsync(dog.Id);
        Assert.Null(deletedDog);

        // Walk events should also be removed
        var walks = await Uow.WalkEvents.GetByDogIdAsync(dog.Id);
        Assert.Empty(walks);
    }

    [Fact]
    public async Task GetByClientIdAsync_ReturnsOnlyClientDogs()
    {
        var client1 = await SeedClientAsync("Client1", "+1111111111", "c1@test.com");
        var client2 = await SeedClientAsync("Client2", "+2222222222", "c2@test.com");
        await SeedDogAsync(client1.Id, "Dog1");
        await SeedDogAsync(client1.Id, "Dog2");
        await SeedDogAsync(client2.Id, "Dog3");
        var svc = CreateDogService();

        var result = (await svc.GetByClientIdAsync(client1.Id)).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, d => Assert.Equal(client1.Id, d.ClientId));
    }

    [Fact]
    public async Task DeleteAsync_NonExistent_ThrowsKeyNotFound()
    {
        var svc = CreateDogService();

        await Assert.ThrowsAsync<KeyNotFoundException>(() => svc.DeleteAsync(999));
    }
}
