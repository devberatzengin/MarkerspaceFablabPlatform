using MakerspaceFablabPlatform.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MakerspaceFablabPlatform.Data.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {

        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id)
            .ValueGeneratedNever();

        builder.Property(n => n.Type)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(n => n.Channel)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(n => n.Title)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(n => n.Message)
            .HasMaxLength(500);

        builder.Property(n => n.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(n => n.ReadAt)
            .HasColumnType("timestamp with time zone");

        builder.HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(n => new { n.UserId, n.IsRead, n.CreatedAt });
        builder.HasIndex(n => new { n.RelatedEntityId, n.Type, n.ThresholdMinutes });

        builder.HasQueryFilter(n => !n.IsDeleted);
    }
}
