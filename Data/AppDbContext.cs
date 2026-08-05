using MakerspaceFablabPlatform.Data.Configurations;
using MakerspaceFablabPlatform.Data.Interfaces;
using MakerspaceFablabPlatform.Entities;
using MakerspaceFablabPlatform.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MakerspaceFablabPlatform.Data;

public class AppDbContext : DbContext,  IApplicationDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }
    
    public DbSet<Announcement> Announcements { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Equipment> Equipments { get; set; }
    public DbSet<EquipmentRental> EquipmentRentals { get; set; }
    public DbSet<Payment> Payments { get; set; }
    
    
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<Notification> Notifications { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
        // Tek kural: her tarih UTC. Kind belirtilmemişse zaten UTC kabul edilir,
        // sunucunun yerel saatine göre kaydırılmaz (ToUniversalTime bunu yapardı).
        var utcConverter = new ValueConverter<DateTime, DateTime>(
            v => v.ToUtc(),                                  // DB'ye yazarken
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc)); // DB'den okurken

        var nullableUtcConverter = new ValueConverter<DateTime?, DateTime?>(
            v => v.HasValue ? v.Value.ToUtc() : v,
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        foreach (var property in entityType.GetProperties())
        {
            if (property.ClrType == typeof(DateTime))
                property.SetValueConverter(utcConverter);
            else if (property.ClrType == typeof(DateTime?))
                property.SetValueConverter(nullableUtcConverter);
        }

        
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        modelBuilder.ApplyConfiguration(new PaymentConfiguration());
    }
}