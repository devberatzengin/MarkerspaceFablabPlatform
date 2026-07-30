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

        builder.HasOne(p => p.EquipmentRental)
            .WithOne(er => er.Payment)
            .HasForeignKey<Payment>(p => p.EquipmentRentalId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.User)
            .WithMany(u => u.Payments)
            .HasForeignKey(p => p.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.PaymentNumber).IsUnique();
        
        builder.HasIndex(p => new { p.UserId, p.Status });

        builder.HasIndex(p => new { p.UserId, p.CreatedAt });


        builder.HasIndex(p => new { p.Status, p.CreatedAt });

        builder.HasIndex(p => p.EquipmentRentalId);

        builder.HasIndex(p => new { p.Status, p.PaidAt });

        builder.HasIndex(p => p.Status);

        builder.HasIndex(p => p.CreatedAt);
        
    }
}
