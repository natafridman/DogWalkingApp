using DogWalking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DogWalking.Infrastructure.Data.Configurations;

public class WalkerAvailabilityConfiguration : IEntityTypeConfiguration<WalkerAvailability>
{
    public void Configure(EntityTypeBuilder<WalkerAvailability> builder)
    {
        builder.ToTable("WalkerAvailabilities");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.DayOfWeek)
               .HasConversion<int>()
               .IsRequired();

        builder.Property(a => a.StartTime).IsRequired();
        builder.Property(a => a.EndTime).IsRequired();

        builder.Property(a => a.Zone)
               .HasMaxLength(100)
               .IsRequired()
               .HasDefaultValue(string.Empty);

        builder.HasOne(a => a.Walker)
               .WithMany()
               .HasForeignKey(a => a.WalkerId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => a.WalkerId);
    }
}
