using DogWalking.Domain.Exceptions;
using DogWalking.Domain.ValueObjects;

namespace DogWalking.Tests.Domain;

public class PhoneNumberTests
{
    [Theory]
    [InlineData("+1234567890")]
    [InlineData("123 456 7890")]
    [InlineData("+54 11 1234-5678")]
    public void Constructor_ValidPhone_Succeeds(string phone)
    {
        var p = new PhoneNumber(phone);
        Assert.Equal(phone.Trim(), p.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("abc")]
    [InlineData("123")]
    public void Constructor_InvalidPhone_ThrowsDomainException(string phone)
        => Assert.Throws<DomainException>(() => new PhoneNumber(phone));

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var p1 = new PhoneNumber("+1234567890");
        var p2 = new PhoneNumber("+1234567890");
        Assert.Equal(p1, p2);
        Assert.Equal(p1.GetHashCode(), p2.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValue_AreNotEqual()
    {
        var p1 = new PhoneNumber("+1234567890");
        var p2 = new PhoneNumber("+9876543210");
        Assert.NotEqual(p1, p2);
    }

    [Fact]
    public void ToString_ReturnsValue()
        => Assert.Equal("+1234567890", new PhoneNumber("+1234567890").ToString());
}
