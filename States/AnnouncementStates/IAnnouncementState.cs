using MakerspaceFablabPlatform.Entities;
using MakerspaceFablabPlatform.Data.Interfaces;
using MakerspaceFablabPlatform.Entities.Enums;

namespace MakerspaceFablabPlatform.States.AnnouncementStates;

public interface IAnnouncementState
{
    Task PublishAsync(Announcement announcement);
    Task UnpublishAsync(Announcement announcement);
    Task ArchiveAsync(Announcement announcement);
}