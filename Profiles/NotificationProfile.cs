using AutoMapper;
using MakerspaceFablabPlatform.Dtos.Notification;
using MakerspaceFablabPlatform.Entities;

namespace MakerspaceFablabPlatform.Profiles;

public class NotificationProfile : Profile
{
    public NotificationProfile()
    {
        CreateMap<Notification, Response>();
    }
}
