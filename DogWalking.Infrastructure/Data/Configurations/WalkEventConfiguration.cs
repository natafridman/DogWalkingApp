using DogWalking.Domain.Entities;
using DogWalking.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DogWalking.Infrastructure.Data.Configurations;

public class WalkEventConfiguration : IEntityTypeConfiguration<WalkEvent>
{
    public void Configure(EntityTypeBuilder<WalkEvent> builder)
    {
        builder.ToTable("WalkEvents");
        builder.HasKey(w => w.Id);

        builder.Property(w => w.WalkDate).IsRequired();
        builder.Property(w => w.DurationMinutes).IsRequired();
        builder.Property(w => w.Status).HasConversion<int>().IsRequired();
        builder.Property(w => w.Location).HasMaxLength(150).IsRequired().HasDefaultValue("General");
        builder.Property(w => w.EstimatedArrivalTime).IsRequired(false);
        builder.Property(w => w.Notes).HasMaxLength(500);
        builder.Property(w => w.RecurrenceType).HasConversion<int>().HasDefaultValue(RecurrenceType.OneTime).IsRequired();
        builder.Property(w => w.CreatedAt).IsRequired();
        builder.Property(w => w.WalkerId).IsRequired(false);

        // Optimistic concurrency — prevents two walkers from claiming the same walk
        builder.Property(w => w.RowVersion).IsRowVersion();

        // Walker relationship — nullable FK to User
        builder.HasOne(w => w.Walker)
               .WithMany(u => u.AssignedWalks)
               .HasForeignKey(w => w.WalkerId)
               .OnDelete(DeleteBehavior.SetNull)
               .IsRequired(false);

        // Declines — walkers who passed on this walk
        builder.HasMany(w => w.Declines)
               .WithOne(d => d.WalkEvent)
               .HasForeignKey(d => d.WalkEventId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(w => w.Declines)
               .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Composite index for efficient subscription validation queries
        builder.HasIndex(w => new { w.DogId, w.WalkDate });
    }
}
