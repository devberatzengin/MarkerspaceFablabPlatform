using MarkerspaceFablabPlatform.Entitys;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarkerspaceFablabPlatform.Data.Configurations;

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
        
        builder.HasIndex(e => e.Email).IsUnique();

        builder.HasQueryFilter(u => !u.IsDeleted);
        
        builder.HasMany(u => u.Announcements)
            .WithOne(a => a.CreatedBy)
            .HasForeignKey(a => a.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}