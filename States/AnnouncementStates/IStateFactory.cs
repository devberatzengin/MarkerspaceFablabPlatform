using MakerspaceFablabPlatform.Entities.Enums;

namespace MakerspaceFablabPlatform.States.AnnouncementStates;

public interface IStateFactory
{
    IAnnouncementState Create(ContentStatus status);
}