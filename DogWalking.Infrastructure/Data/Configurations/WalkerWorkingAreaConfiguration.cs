using DogWalking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DogWalking.Infrastructure.Data.Configurations;

public class WalkerWorkingAreaConfiguration : IEntityTypeConfiguration<WalkerWorkingArea>
{
    public void Configure(EntityTypeBuilder<WalkerWorkingArea> builder)
    {
        builder.ToTable("WalkerWorkingAreas");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.ZoneName)
               .HasMaxLength(100)
               .IsRequired();

        builder.HasOne(a => a.Walker)
               .WithMany()
               .HasForeignKey(a => a.WalkerId)
               .OnDelete(DeleteBehavior.Cascade);

        // Each walker can only have one entry per zone
        builder.HasIndex(a => new { a.WalkerId, a.ZoneName }).IsUnique();
    }
}
