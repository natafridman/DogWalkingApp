using System.Text.RegularExpressions;
using DogWalking.Domain.Exceptions;

namespace DogWalking.Domain.ValueObjects;

/// <summary>
/// Immutable value object representing a validated phone number.
/// Equality is structural (by value), not referential.
/// </summary>
public sealed class PhoneNumber : IEquatable<PhoneNumber>
{
    public string Value { get; }

    private static readonly Regex PhoneRegex =
        new(@"^\+?[\d\s\-\(\)]{7,20}$", RegexOptions.Compiled);

    public PhoneNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Phone number cannot be empty.");

        var normalized = value.Trim();

        if (!PhoneRegex.IsMatch(normalized))
            throw new DomainException($"'{value}' is not a valid phone number.");

        Value = normalized;
    }

    public bool Equals(PhoneNumber? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is PhoneNumber p && Equals(p);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value;
}
