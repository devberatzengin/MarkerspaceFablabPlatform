using MakerspaceFablabPlatform.Data.Interfaces;
using MakerspaceFablabPlatform.Entities;
using MakerspaceFablabPlatform.Entities.Enums;
using MakerspaceFablabPlatform.Excepitons;

namespace MakerspaceFablabPlatform.States.AnnouncementStates;

public class PublishedState : IAnnouncementState
{
    public Task PublishAsync(Announcement announcement, IUnitOfWork unitOfWork)
    {
        throw new ConflictException("Duyuru zaten yayında.");
    }

    public async Task UnpublishAsync(Announcement announcement, IUnitOfWork unitOfWork)
    {
        announcement.Status = ContentStatus.Unpublished;
        announcement.UpdatedAt  = DateTime.Now;
        
        unitOfWork.Announcements.Update(announcement);
        await unitOfWork.Announcements.SaveChangesAsync();
    }

    public async Task ArchiveAsync(Announcement announcement, IUnitOfWork unitOfWork)
    {
        announcement.Status = ContentStatus.Archived;
        announcement.UpdatedAt  = DateTime.Now;
        
        unitOfWork.Announcements.Update(announcement);
        await unitOfWork.Announcements.SaveChangesAsync();
    }
}