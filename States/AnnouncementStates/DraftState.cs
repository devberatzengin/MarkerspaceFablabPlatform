using MakerspaceFablabPlatform.Data.Interfaces;
using MakerspaceFablabPlatform.Entities;
using MakerspaceFablabPlatform.Entities.Enums;
using MakerspaceFablabPlatform.Excepitons;

namespace MakerspaceFablabPlatform.States.AnnouncementStates;

public class DraftState : IAnnouncementState
{
    public async Task PublishAsync(Announcement announcement)
    {
        announcement.Status = ContentStatus.Published;
        announcement.UpdatedAt = DateTime.UtcNow;
    }

    public Task UnpublishAsync(Announcement announcement)
    {
        throw new ConflictException("Draft halindeki bir duyuru kaldırılamaz.");
    }

    public async Task ArchiveAsync(Announcement announcement)
    {
        announcement.Status = ContentStatus.Archived;
        announcement.UpdatedAt = DateTime.UtcNow;
    }
}