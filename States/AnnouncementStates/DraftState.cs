using MakerspaceFablabPlatform.Data.Interfaces;
using MakerspaceFablabPlatform.Entities;
using MakerspaceFablabPlatform.Entities.Enums;
using MakerspaceFablabPlatform.Excepitons;

namespace MakerspaceFablabPlatform.States.AnnouncementStates;

public class DraftState : IAnnouncementState
{
    public async Task PublishAsync(Announcement announcement, IUnitOfWork unitOfWork)
    {
        announcement.Status = ContentStatus.Published;
        announcement.UpdatedAt = DateTime.UtcNow;
        unitOfWork.Announcements.Update(announcement);
        await unitOfWork.SaveChangesAsync();
    }

    public Task UnpublishAsync(Announcement announcement, IUnitOfWork unitOfWork)
    {
        throw new ConflictException("Draft halindeki bir duyuru kaldırılamaz.");
    }

    public async Task ArchiveAsync(Announcement announcement, IUnitOfWork unitOfWork)
    {
        announcement.Status = ContentStatus.Archived;
        announcement.UpdatedAt = DateTime.UtcNow;
        
        unitOfWork.Announcements.Update(announcement);
        await unitOfWork.SaveChangesAsync();
    }
}