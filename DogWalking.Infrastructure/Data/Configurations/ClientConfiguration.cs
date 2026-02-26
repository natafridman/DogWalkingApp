using DogWalking.Domain.Entities;
using DogWalking.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DogWalking.Infrastructure.Data.Configurations;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("Clients");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
               .IsRequired()
               .HasMaxLength(100);

        // PhoneNumber value object — stored as a plain string column
        builder.Property(c => c.PhoneNumber)
               .HasConversion(p => p.Value, v => new PhoneNumber(v))
               .HasColumnName("PhoneNumber")
               .HasMaxLength(30)
               .IsRequired();

        builder.Property(c => c.Email)
               .IsRequired()
               .HasMaxLength(150);

        builder.Property(c => c.Subscription)
               .HasConversion<int>()
               .IsRequired();

        builder.Property(c => c.IsActive)
               .IsRequired()
               .HasDefaultValue(true);

        builder.Property(c => c.Zone)
               .HasMaxLength(100)
               .IsRequired()
               .HasDefaultValue(string.Empty);

        builder.Property(c => c.Address)
               .HasMaxLength(200)
               .IsRequired()
               .HasDefaultValue(string.Empty);

        builder.Property(c => c.CreatedAt).IsRequired();

        builder.Property(c => c.UserId).IsRequired(false);

        builder.HasIndex(c => c.Email).IsUnique();
    }
}
