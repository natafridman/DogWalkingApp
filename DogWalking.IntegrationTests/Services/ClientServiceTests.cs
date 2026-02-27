using DogWalking.Application.DTOs;
using DogWalking.Domain.Enums;

namespace DogWalking.IntegrationTests.Services;

public class ClientServiceTests : IntegrationTestBase
{
    [Fact]
    public async Task CreateAsync_ValidData_PersistsAndReturns()
    {
        var svc = CreateClientService();

        var dto = new CreateClientDto("John Doe", "+1234567890", "john@test.com",
            SubscriptionType.Basic, "Palermo", "123 Main St");
        var result = await svc.CreateAsync(dto);

        Assert.Equal("John Doe", result.Name);
        Assert.Equal(SubscriptionType.Basic, result.Subscription);
        Assert.True(result.IsActive);
        Assert.True(result.Id > 0);
    }

    [Fact]
    public async Task GetByIdAsync_Existing_ReturnsWithDogCount()
    {
        var client = await SeedClientAsync();
        await SeedDogAsync(client.Id, "Rex");
        await SeedDogAsync(client.Id, "Max");
        var svc = CreateClientService();

        var result = await svc.GetByIdAsync(client.Id);

        Assert.NotNull(result);
        Assert.Equal(client.Id, result.Id);
        Assert.Equal(2, result.DogCount);
    }

    [Fact]
    public async Task GetAllActiveAsync_ReturnsOnlyActive()
    {
        var active = await SeedClientAsync("Active Client", "+1111111111", "active@test.com");
        var inactive = await SeedClientAsync("Inactive Client", "+2222222222", "inactive@test.com");
        inactive.Deactivate();
        Uow.Clients.Update(inactive);
        await Uow.CommitAsync();
        var svc = CreateClientService();

        var result = (await svc.GetAllActiveAsync()).ToList();

        Assert.Single(result);
        Assert.Equal("Active Client", result[0].Name);
    }

    [Fact]
    public async Task SearchAsync_ByName_FindsMatch()
    {
        await SeedClientAsync("Maria Lopez", "+1111111111", "maria@test.com");
        await SeedClientAsync("Carlos Garcia", "+2222222222", "carlos@test.com");
        var svc = CreateClientService();

        var result = (await svc.SearchAsync("Maria")).ToList();

        Assert.Single(result);
        Assert.Equal("Maria Lopez", result[0].Name);
    }

    [Fact]
    public async Task UpdateAsync_ChangesFields()
    {
        var client = await SeedClientAsync();
        var svc = CreateClientService();

        var dto = new UpdateClientDto(client.Id, "Updated Name", "+9999999999",
            "updated@test.com", SubscriptionType.Pro, "NewZone");
        var result = await svc.UpdateAsync(dto);

        Assert.Equal("Updated Name", result.Name);
        Assert.Equal(SubscriptionType.Pro, result.Subscription);
    }

    [Fact]
    public async Task ChangeSubscriptionAsync_UpdatesPlan()
    {
        var client = await SeedClientAsync();
        var svc = CreateClientService();

        var result = await svc.ChangeSubscriptionAsync(
            new ChangeSubscriptionDto(client.Id, SubscriptionType.Premium));

        Assert.Equal(SubscriptionType.Premium, result.Subscription);
    }

    [Fact]
    public async Task DeleteAsync_DeactivatesClient()
    {
        var client = await SeedClientAsync();
        var svc = CreateClientService();

        await svc.DeleteAsync(client.Id);

        var updated = await Uow.Clients.GetByIdAsync(client.Id);
        Assert.NotNull(updated);
        Assert.False(updated.IsActive);
    }
}
