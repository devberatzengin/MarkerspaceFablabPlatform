using MakerspaceFablabPlatform.Entities.Enums;

namespace MakerspaceFablabPlatform.States.AnnouncementStates;

public class StateFactory : IStateFactory
{
    public IAnnouncementState Create(ContentStatus status)
    {
        return status switch
        {
            _ when status == ContentStatus.Archived => new ArchivedState(),
            _ when status == ContentStatus.Draft => new DraftState(),
            _ when status == ContentStatus.Published => new PublishedState(),
            _ when status == ContentStatus.Unpublished => new UnpublishedState(),
            _ => throw new NotSupportedException($"Content status {status} is not supported.")
        };
    }
}