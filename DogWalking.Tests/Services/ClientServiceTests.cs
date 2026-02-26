using DogWalking.Application.DTOs;
using DogWalking.Application.Services;
using DogWalking.Application.Validators;
using DogWalking.Domain.Entities;
using DogWalking.Domain.Enums;
using DogWalking.Domain.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace DogWalking.Tests.Services;

public class ClientServiceTests
{
    private readonly Mock<IUnitOfWork> _uow;
    private readonly Mock<IClientRepository> _repo;
    private readonly ClientService _sut;

    public ClientServiceTests()
    {
        _uow = new Mock<IUnitOfWork>();
        _repo = new Mock<IClientRepository>();
        _uow.Setup(u => u.Clients).Returns(_repo.Object);
        _uow.Setup(u => u.CommitAsync(default)).ReturnsAsync(1);
        _sut = new ClientService(_uow.Object, NullLogger<ClientService>.Instance,
                                  new CreateClientDtoValidator());
    }

    [Fact]
    public async Task CreateAsync_ValidData_SavesAndReturnsDto()
    {
        _repo.Setup(r => r.AddAsync(It.IsAny<Client>(), default)).Returns(Task.CompletedTask);
        var dto = new CreateClientDto("John Doe", "+1234567890", "j@j.com",
                                      SubscriptionType.Basic, Address: "123 Main St");
        var result = await _sut.CreateAsync(dto);
        Assert.Equal("John Doe", result.Name);
        Assert.Equal(SubscriptionType.Basic, result.Subscription);
        _uow.Verify(u => u.CommitAsync(default), Times.Once);
    }

    [Theory]
    [InlineData("", "+1234567890", "j@j.com")]
    [InlineData("John", "", "j@j.com")]
    [InlineData("John", "+1234567890", "notanemail")]
    public async Task CreateAsync_InvalidData_ThrowsBeforeCommit(string name, string phone, string email)
    {
        await Assert.ThrowsAnyAsync<Exception>(() =>
            _sut.CreateAsync(new CreateClientDto(name, phone, email)));
        _uow.Verify(u => u.CommitAsync(default), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingClient_ReturnsDto()
    {
        var client = new Client("Jane", "+9876543210", "jane@test.com", SubscriptionType.Pro);
        _repo.Setup(r => r.GetByIdWithDogsAsync(1, default)).ReturnsAsync(client);
        var result = await _sut.GetByIdAsync(1);
        Assert.NotNull(result);
        Assert.Equal("Jane", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsNull()
    {
        _repo.Setup(r => r.GetByIdWithDogsAsync(99, default)).ReturnsAsync((Client?)null);
        Assert.Null(await _sut.GetByIdAsync(99));
    }

    [Fact]
    public async Task DeleteAsync_NotFound_ThrowsKeyNotFoundException()
    {
        _repo.Setup(r => r.GetByIdWithDogsAsync(99, default)).ReturnsAsync((Client?)null);
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.DeleteAsync(99));
    }
}