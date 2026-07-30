using MakerspaceFablabPlatform.Entities;
using MakerspaceFablabPlatform.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MakerspaceFablabPlatform.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(p => p.Id);

        // ========== PROPERTIES ==========
        builder.Property(p => p.PaymentNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.RentalFee)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(p => p.LateFee)
            .HasPrecision(10, 2)
            .HasDefaultValue(0m);

        builder.Property(p => p.TotalAmount)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(p => p.DiscountAmount)
            .HasPrecision(10, 2)
            .HasDefaultValue(0m);

        builder.Property(p => p.PaidAmount)
            .HasPrecision(10, 2)
            .HasDefaultValue(0m);

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue(PaymentStatus.Pending);

        builder.Property(p => p.PaymentMethod)
            .HasConversion<string>()
            .HasMaxLength(50);



        builder.Property(p => p.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // ========== RELATIONSHIPS ==========
        // Equipment Rental -> Payment (one-to-one)
        builder.HasOne(p => p.EquipmentRental)
            .WithOne(er => er.Payment)
            .HasForeignKey<Payment>(p => p.EquipmentRentalId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        // User -> Payments (one-to-many)
        builder.HasOne(p => p.User)
            .WithMany(u => u.Payments)
            .HasForeignKey(p => p.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        // ========== INDEXES - CRITICAL FOR PERFORMANCE ==========

        // Unique payment number for reference
        builder.HasIndex(p => p.PaymentNumber)
            .IsUnique()
            .HasName("IX_Payments_PaymentNumber_Unique");

        // User payment lookup
        builder.HasIndex(p => new { p.UserId, p.Status })
            .HasName("IX_Payments_UserStatus");

        builder.HasIndex(p => new { p.UserId, p.CreatedAt })
            .HasName("IX_Payments_UserDate");

        // Revenue reporting indexes
        builder.HasIndex(p => new { p.Status, p.CreatedAt })
            .HasName("IX_Payments_StatusDate");

        // Rental payment lookup
        builder.HasIndex(p => p.EquipmentRentalId)
            .HasName("IX_Payments_EquipmentRentalId");

        // Tracking sent payments
        builder.HasIndex(p => new { p.Status, p.PaidAt })
            .HasName("IX_Payments_StatusPaidAt");

        // Single column indexes for filtering
        builder.HasIndex(p => p.Status)
            .HasName("IX_Payments_Status");

        builder.HasIndex(p => p.CreatedAt)
            .HasName("IX_Payments_CreatedAt");

        // ========== CONSTRAINTS ==========
        builder.HasCheckConstraint("CK_TotalAmount_NonNegative",
            "\"TotalAmount\" >= 0");

        builder.HasCheckConstraint("CK_RentalFee_NonNegative",
            "\"RentalFee\" >= 0");

        builder.HasCheckConstraint("CK_LateFee_NonNegative",
            "\"LateFee\" >= 0");

        builder.HasCheckConstraint("CK_PaidAmount_NonNegative",
            "\"PaidAmount\" >= 0");

        builder.HasCheckConstraint("CK_PaidAtAfterCreated",
            "\"PaidAt\" IS NULL OR \"PaidAt\" >= \"CreatedAt\"");

        builder.HasCheckConstraint("CK_RefundedAtAfterCreated",
            "\"RefundedAt\" IS NULL OR \"RefundedAt\" >= \"CreatedAt\"");
    }
}
