using DogWalking.Application.DTOs;
using DogWalking.Domain.Enums;

namespace DogWalking.IntegrationTests.Services;

public class AuthServiceTests : IntegrationTestBase
{
    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsSuccess()
    {
        var svc = CreateAuthService();
        await svc.CreateUserAsync(new CreateUserDto("john", "Pass123!", "John Doe", UserRole.Admin));

        var result = await svc.LoginAsync(new LoginDto("john", "Pass123!"));

        Assert.True(result.Success);
        Assert.Equal("john", result.Username);
        Assert.Equal("John Doe", result.FullName);
        Assert.Equal("Admin", result.Role);
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ReturnsFail()
    {
        var svc = CreateAuthService();
        await svc.CreateUserAsync(new CreateUserDto("john", "Pass123!", "John Doe", UserRole.Admin));

        var result = await svc.LoginAsync(new LoginDto("john", "WrongPass"));

        Assert.False(result.Success);
        Assert.Equal("Invalid credentials.", result.ErrorMessage);
    }

    [Fact]
    public async Task LoginAsync_NonexistentUser_ReturnsFail()
    {
        var svc = CreateAuthService();

        var result = await svc.LoginAsync(new LoginDto("nobody", "Pass123!"));

        Assert.False(result.Success);
        Assert.Equal("Invalid credentials.", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateUserAsync_NewUser_ReturnsTrue()
    {
        var svc = CreateAuthService();

        var result = await svc.CreateUserAsync(
            new CreateUserDto("alice", "Pass123!", "Alice Smith", UserRole.Walker));

        Assert.True(result);

        var user = await Uow.Users.GetByUsernameAsync("alice");
        Assert.NotNull(user);
        Assert.Equal(UserRole.Walker, user.Role);
    }

    [Fact]
    public async Task CreateUserAsync_DuplicateUsername_ReturnsFalse()
    {
        var svc = CreateAuthService();
        await svc.CreateUserAsync(new CreateUserDto("alice", "Pass123!", "Alice", UserRole.Walker));

        var result = await svc.CreateUserAsync(
            new CreateUserDto("alice", "Other123!", "Alice Dup", UserRole.Admin));

        Assert.False(result);
    }

    [Fact]
    public async Task RegisterClientUserAsync_Valid_CreatesUserAndClient()
    {
        var svc = CreateAuthService();

        var result = await svc.RegisterClientUserAsync(new RegisterClientUserDto(
            "clientuser", "Pass123!", "Client Person",
            "+1234567890", "client@test.com", SubscriptionType.Basic,
            "123 Test St", "Pass123!"));

        Assert.True(result.Success);
        Assert.Equal("Client", result.Role);

        // Verify a Client entity was also created
        var clients = await Uow.Clients.GetAllActiveAsync();
        Assert.Single(clients);
        Assert.Equal("Client Person", clients.First().Name);
    }

    [Fact]
    public async Task RegisterClientUserAsync_DuplicateUsername_ReturnsFail()
    {
        var svc = CreateAuthService();
        await svc.CreateUserAsync(new CreateUserDto("taken", "Pass123!", "Taken User", UserRole.Admin));

        var result = await svc.RegisterClientUserAsync(new RegisterClientUserDto(
            "taken", "Pass123!", "Another Person",
            "+9999999999", "another@test.com", SubscriptionType.Free,
            "456 Other St", "Pass123!"));

        Assert.False(result.Success);
        Assert.Equal("Username is already taken.", result.ErrorMessage);
    }
}
