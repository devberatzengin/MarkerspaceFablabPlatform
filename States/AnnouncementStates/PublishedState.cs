using MakerspaceFablabPlatform.Data.Interfaces;
using MakerspaceFablabPlatform.Entities;
using MakerspaceFablabPlatform.Entities.Enums;
using MakerspaceFablabPlatform.Excepitons;

namespace MakerspaceFablabPlatform.States.AnnouncementStates;

public class PublishedState : IAnnouncementState
{
    public Task PublishAsync(Announcement announcement)
    {
        throw new ConflictException("Duyuru zaten yayında.");
    }

    public async Task UnpublishAsync(Announcement announcement)
    {
        announcement.Status = ContentStatus.Unpublished;
        announcement.UpdatedAt  = DateTime.UtcNow;
    }

    public async Task ArchiveAsync(Announcement announcement)
    {
        announcement.Status = ContentStatus.Archived;
        announcement.UpdatedAt  = DateTime.UtcNow;
    }
}