using MakerspaceFablabPlatform.Entitys;
using MakerspaceFablabPlatform.Data.Interfaces;
using MakerspaceFablabPlatform.Entitys.Enums;

namespace MakerspaceFablabPlatform.States.AnnouncementStates;

public interface IAnnouncementState
{
    Task PublishAsync(Announcement announcement, IUnitOfWork unitOfWork);
    Task UnpublishAsync(Announcement announcement, IUnitOfWork unitOfWork);
    Task ArchiveAsync(Announcement announcement, IUnitOfWork unitOfWork);
}