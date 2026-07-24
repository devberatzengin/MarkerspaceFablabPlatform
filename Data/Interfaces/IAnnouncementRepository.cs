using MakerspaceFablabPlatform.Entitys;

namespace MakerspaceFablabPlatform.Data.Interfaces;
          
public interface IAnnouncementRepository : IRepository<Announcement>
{
    Task<Announcement?> GetByIdWithDetailsAsync(Guid id, bool asNoTracking = true, CancellationToken cancellationToken = default);
    Task<int> CountByTitleAsync(string title, Guid? excludeId = null, CancellationToken cancellationToken = default);
}
