using MakerspaceFablabPlatform.Data.Interfaces;
using MakerspaceFablabPlatform.Entities;
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
    public DbSet<Payment> Payments { get; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
        var utcConverter = new ValueConverter<DateTime, DateTime>(
            v => v.ToUniversalTime(),                        // DB'ye yazarken
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc)); // DB'den okurken

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        foreach (var property in entityType.GetProperties())
            if (property.ClrType == typeof(DateTime))
                property.SetValueConverter(utcConverter);

        
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}