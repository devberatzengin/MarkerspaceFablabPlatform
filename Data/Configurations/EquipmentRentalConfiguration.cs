using MakerspaceFablabPlatform.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MakerspaceFablabPlatform.Data.Configurations;

public class EquipmentRentalConfiguration : IEntityTypeConfiguration<EquipmentRental>
{
    public void Configure(EntityTypeBuilder<EquipmentRental> builder)
    {
        // Table name
        builder.ToTable("EquipmentRentals");

        // Primary key
        builder.HasKey(er => er.Id);

        // Properties configuration
        builder
            .Property(er => er.Id)
            .HasColumnType("uuid")
            .ValueGeneratedOnAdd();

        builder
            .Property(er => er.UserId)
            .HasColumnType("uuid")
            .IsRequired();

        builder
            .Property(er => er.EquipmentId)
            .HasColumnType("uuid")
            .IsRequired();

        builder
            .Property(er => er.RentedAt)
            .HasColumnType("timestamp")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder
            .Property(er => er.ExpectedReturnAt)
            .HasColumnType("timestamp")
            .IsRequired();

        builder
            .Property(er => er.ReleasedAt)
            .HasColumnType("timestamp");

        // NEW: Payment relationship
        builder
            .Property(er => er.PaymentId)
            .HasColumnType("uuid");

        builder
            .Property(er => er.IsPaid)
            .HasColumnType("boolean")
            .HasDefaultValue(false);

        builder
            .Property(er => er.PaidAt)
            .HasColumnType("timestamp");

        builder
            .Property(er => er.IsOverdue)
            .HasColumnType("boolean")
            .HasDefaultValue(false);

        builder
            .Property(er => er.OverdueBy)
            .HasColumnType("interval");

        // Foreign keys
        builder
            .HasOne(er => er.User)
            .WithMany(u => u.EquipmentRentals)
            .HasForeignKey(er => er.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(er => er.Equipment)
            .WithMany(e => e.EquipmentRentals)
            .HasForeignKey(er => er.EquipmentId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        // NEW: Payment relationship
        builder
            .HasOne(er => er.Payment)
            .WithOne(p => p.EquipmentRental)
            .HasForeignKey<EquipmentRental>(er => er.PaymentId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(er => er.UserId);
        builder.HasIndex(er => er.EquipmentId);
        builder.HasIndex(er => er.RentedAt);
        builder.HasIndex(er => er.ExpectedReturnAt);
        builder.HasIndex(er => er.ReleasedAt);
        builder.HasIndex(er => er.PaymentId);
        builder.HasIndex(er => er.IsPaid);
        builder.HasIndex(er => er.IsOverdue);
        builder.HasIndex(er => new { er.UserId, er.ReleasedAt }); // For active rentals query
        builder.HasIndex(er => new { er.UserId, er.RentedAt }); // For rental history query

        // Constraints
    }
}
