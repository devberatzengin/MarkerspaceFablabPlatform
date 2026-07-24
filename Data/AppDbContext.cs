using MarkerspaceFablabPlatform.Data.Interfaces;
using MarkerspaceFablabPlatform.Entitys;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MarkerspaceFablabPlatform.Data;

public class AppDbContext : DbContext,  IApplicationDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }
    
    public DbSet<Announcement> Announcements { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<User> Users { get; set; }

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