using MakerspaceFablabPlatform.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MakerspaceFablabPlatform.Data.Configurations;

public class EquipmentConfiguration : IEntityTypeConfiguration<Equipment>
{
    public void Configure(EntityTypeBuilder<Equipment> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.PlacementType) // Equipment Placement Type Enum
            .HasConversion<string>()
            .IsRequired();

        builder.Property(e => e.Type) // Equipment Type Enum
            .HasConversion<string>()
            .IsRequired();
        
        builder.Property(e => e.Status) // Equipment Status Enum
            .HasConversion<string>()
            .IsRequired();
        
        builder.HasQueryFilter(u => !u.IsDeleted);
        
        builder.HasMany(e => e.EquipmentRentals)
            .WithOne(r => r.Equipment)
            .HasForeignKey(r => r.EquipmentId)
            .OnDelete(DeleteBehavior.Restrict);
        
    }
}