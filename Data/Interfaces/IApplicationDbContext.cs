using MakerspaceFablabPlatform.Entities;
using Microsoft.EntityFrameworkCore;

namespace MakerspaceFablabPlatform.Data.Interfaces;
   
public interface IApplicationDbContext
{
    //Buraya başka yeni Db Table Lar gelicek
    DbSet<Announcement> Announcements { get; }
    DbSet<Category> Categories { get; }
    DbSet<User> Users { get; }
    DbSet<Equipment> Equipments { get; }
    DbSet<EquipmentRental> EquipmentRentals { get; }
    DbSet<Payment> Payments { get; }
    
    DbSet<T> Set<T>() where T : class;  
    
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}