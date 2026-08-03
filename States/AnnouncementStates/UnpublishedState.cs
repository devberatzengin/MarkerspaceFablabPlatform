using MakerspaceFablabPlatform.Data.Interfaces;
using MakerspaceFablabPlatform.Entities;
using MakerspaceFablabPlatform.Entities.Enums;
using MakerspaceFablabPlatform.Excepitons;

namespace MakerspaceFablabPlatform.States.AnnouncementStates;

public class UnpublishedState : IAnnouncementState
{
    public async Task PublishAsync(Announcement announcement)
    {
        announcement.Status = ContentStatus.Published;
        announcement.UpdatedAt  = DateTime.UtcNow;
    }

    public async Task UnpublishAsync(Announcement announcement)
    {
        throw new ConflictException("Duyuru zaten yayından kaldırılmış.");
    }

    public async Task ArchiveAsync(Announcement announcement)
    {
        announcement.Status = ContentStatus.Archived;
        announcement.UpdatedAt  = DateTime.UtcNow;
    }
}