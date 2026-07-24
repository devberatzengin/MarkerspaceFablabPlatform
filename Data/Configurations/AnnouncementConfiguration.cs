using MakerspaceFablabPlatform.Entitys;
using MakerspaceFablabPlatform.Entitys.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MakerspaceFablabPlatform.Data.Configurations;

public class AnnouncementConfiguration : IEntityTypeConfiguration<Announcement>
{
    public void Configure(EntityTypeBuilder<Announcement> builder)
    {
        builder.HasKey(x => x.Id); 
        builder.Property(x => x.Id)
            .ValueGeneratedNever();
        
        
        builder.HasOne(a => a.Category)
            .WithMany()
            .HasForeignKey(a => a.CategoryId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict); 
        
        builder.HasOne(a => a.CreatedBy)
            .WithMany()
            .HasForeignKey(a => a.CreatedByUserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Property(a => a.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasIndex(a => new { a.CategoryId, a.Status });

        builder.HasQueryFilter(x => x.Status != ContentStatus.Archived);
        
        
    }
}