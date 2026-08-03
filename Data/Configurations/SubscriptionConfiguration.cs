using MakerspaceFablabPlatform.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MakerspaceFablabPlatform.Data.Configurations;

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .ValueGeneratedNever();

        builder.Property(s => s.TargetType)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(s => s.Channel)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Category)
            .WithMany()
            .HasForeignKey(s => s.CategoryId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Equipment)
            .WithMany()
            .HasForeignKey(s => s.EquipmentId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => s.CategoryId);
        builder.HasIndex(s => s.EquipmentId);
        builder.HasIndex(s => new { s.UserId, s.IsActive });

        builder.HasIndex(s => new { s.UserId, s.CategoryId })
            .IsUnique()
            .HasFilter("\"CategoryId\" IS NOT NULL");

        builder.HasIndex(s => new { s.UserId, s.EquipmentId })
            .IsUnique()
            .HasFilter("\"EquipmentId\" IS NOT NULL");

        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}
