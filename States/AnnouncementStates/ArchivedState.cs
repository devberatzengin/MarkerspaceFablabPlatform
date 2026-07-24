using MakerspaceFablabPlatform.Data.Interfaces;
using MakerspaceFablabPlatform.Entitys;
using MakerspaceFablabPlatform.Entitys.Enums;
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