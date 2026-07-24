using MarkerspaceFablabPlatform.Entitys;
using Microsoft.EntityFrameworkCore;

namespace MarkerspaceFablabPlatform.Data.Interfaces;
   
public interface IApplicationDbContext
{
    //Buraya başka yeni Db Table Lar gelicek
    DbSet<Announcement> Announcements { get; }
    DbSet<Category> Categories { get; }
    DbSet<User> Users { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}