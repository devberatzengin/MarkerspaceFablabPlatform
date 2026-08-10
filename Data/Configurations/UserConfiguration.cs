using MakerspaceFablabPlatform.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MakerspaceFablabPlatform.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
        builder.HasIndex(u => u.Email).IsUnique();          // KRİTİK: aynı email 2 kez kayıt olamasın
        builder.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(u => u.LastName).IsRequired().HasMaxLength(100);
        
        builder.Property(u => u.Type)
            .HasConversion<string>()
            .IsRequired();
        
        builder.Property(u => u.Status)
            .HasConversion<string>()
            .IsRequired();
        
        builder.HasIndex(e => e.Email).IsUnique();

        builder.HasQueryFilter(u => !u.IsDeleted);
        
        builder.HasMany(u => u.Announcements)
            .WithOne(a => a.CreatedBy)
            .HasForeignKey(a => a.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(u => u.EquipmentRentals)
            .WithOne(r => r.User)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Db'de artık aynı anda 2 erişim olamyacak ve aynı anda değişim gerçekleştirilemeyecek
        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();
    }
}