using DogWalking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DogWalking.Infrastructure.Data.Configurations;

public class WalkDeclineConfiguration : IEntityTypeConfiguration<WalkDecline>
{
    public void Configure(EntityTypeBuilder<WalkDecline> builder)
    {
        builder.ToTable("WalkDeclines");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.DeclinedAt).IsRequired();

        builder.HasOne(d => d.Walker)
               .WithMany()
               .HasForeignKey(d => d.WalkerId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(d => new { d.WalkEventId, d.WalkerId }).IsUnique();
    }
}
