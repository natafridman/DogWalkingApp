using DogWalking.Domain.Enums;

namespace DogWalking.IntegrationTests.Services;

public class UserServiceTests : IntegrationTestBase
{
    [Fact]
    public async Task GetWalkersAsync_ReturnsOnlyWalkers()
    {
        await SeedUserAsync("admin1", UserRole.Admin, "Admin User");
        await SeedUserAsync("walker1", UserRole.Walker, "Walker One");
        await SeedUserAsync("walker2", UserRole.Walker, "Walker Two");
        await SeedUserAsync("client1", UserRole.Client, "Client User");
        var svc = CreateUserService();

        var result = (await svc.GetWalkersAsync()).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, u => Assert.Equal(UserRole.Walker, u.Role));
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAll()
    {
        await SeedUserAsync("admin1", UserRole.Admin, "Admin User");
        await SeedUserAsync("walker1", UserRole.Walker, "Walker User");
        await SeedUserAsync("client1", UserRole.Client, "Client User");
        var svc = CreateUserService();

        var result = (await svc.GetAllAsync()).ToList();

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetByIdAsync_Existing_ReturnsDto()
    {
        var user = await SeedUserAsync("testuser", UserRole.Admin, "Test Person");
        var svc = CreateUserService();

        var result = await svc.GetByIdAsync(user.Id);

        Assert.NotNull(result);
        Assert.Equal("testuser", result.Username);
        Assert.Equal("Test Person", result.FullName);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistent_ReturnsNull()
    {
        var svc = CreateUserService();

        var result = await svc.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateContactInfoAsync_PersistsChanges()
    {
        var user = await SeedUserAsync("walker1", UserRole.Walker, "Walker One");
        var svc = CreateUserService();

        await svc.UpdateContactInfoAsync(user.Id, "+9999999999", "walker@updated.com");

        var updated = await Uow.Users.GetByIdAsync(user.Id);
        Assert.NotNull(updated);
        Assert.Equal("+9999999999", updated.Phone);
        Assert.Equal("walker@updated.com", updated.Email);
    }
}
