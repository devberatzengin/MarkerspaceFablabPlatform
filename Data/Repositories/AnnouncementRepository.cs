using MakerspaceFablabPlatform.Data.Interfaces;
using MakerspaceFablabPlatform.Entitys;
using Microsoft.EntityFrameworkCore;

namespace MakerspaceFablabPlatform.Data.Repositories;

public class AnnouncementRepository : Repository<Announcement>, IAnnouncementRepository
{
    public AnnouncementRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Announcement?> GetByIdWithDetailsAsync(Guid id, bool asNoTracking = true, CancellationToken cancellationToken = default) =>
        await Query(asNoTracking)
            .Include(a => a.Category)
            .Include(a => a.CreatedBy)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public Task<int> CountByTitleAsync(string title, Guid? excludeId = null, CancellationToken cancellationToken = default) =>
        Query()
            .Where(a => excludeId == null || a.Id != excludeId)
            .CountAsync(a => a.Title == title || a.Title.StartsWith(title + " "), cancellationToken);
}
