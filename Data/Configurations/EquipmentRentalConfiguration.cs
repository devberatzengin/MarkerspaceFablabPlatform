using MakerspaceFablabPlatform.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MakerspaceFablabPlatform.Data.Configurations;

public class EquipmentRentalConfiguration : IEntityTypeConfiguration<EquipmentRental>
{
    public void Configure(EntityTypeBuilder<EquipmentRental> builder)
    {
        builder.HasKey(r => r.Id);

        builder.HasIndex(r => new { r.EquipmentId, r.ReleasedAt });

        builder.HasQueryFilter(r => !r.Equipment.IsDeleted && !r.User.IsDeleted);
    }
}
