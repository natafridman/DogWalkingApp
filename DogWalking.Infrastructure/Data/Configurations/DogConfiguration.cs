using DogWalking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DogWalking.Infrastructure.Data.Configurations;

public class DogConfiguration : IEntityTypeConfiguration<Dog>
{
    public void Configure(EntityTypeBuilder<Dog> builder)
    {
        builder.ToTable("Dogs");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Name).IsRequired().HasMaxLength(80);
        builder.Property(d => d.Breed).IsRequired().HasMaxLength(80);
        builder.Property(d => d.BirthDate).IsRequired();
        builder.Property(d => d.CreatedAt).IsRequired();
        builder.Ignore(d => d.AgeInYears);
    }
}
