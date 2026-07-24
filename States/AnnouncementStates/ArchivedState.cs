using MakerspaceFablabPlatform.Data.Interfaces;
using MakerspaceFablabPlatform.Entities;
using MakerspaceFablabPlatform.Entities.Enums;
using MakerspaceFablabPlatform.Excepitons;

namespace MakerspaceFablabPlatform.States.AnnouncementStates;

public class ArchivedState : IAnnouncementState
{
    public Task PublishAsync(Announcement announcement, IUnitOfWork unitOfWork)
    {
        throw new ConflictException("Duyuru zaten arşivlenmiş.");
    }

    public Task UnpublishAsync(Announcement announcement, IUnitOfWork unitOfWork)
    {        
        throw new ConflictException("Duyuru zaten arşivlenmiş.");
    }

    public Task ArchiveAsync(Announcement announcement, IUnitOfWork unitOfWork)
    {
        throw new ConflictException("Duyuru zaten arşivlenmiş.");
    }
}