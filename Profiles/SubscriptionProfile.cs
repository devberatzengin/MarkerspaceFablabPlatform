using AutoMapper;
using MakerspaceFablabPlatform.Dtos.Subscription;
using MakerspaceFablabPlatform.Entities;

namespace MakerspaceFablabPlatform.Profiles;

public class SubscriptionProfile : Profile
{
    public SubscriptionProfile()
    {
        CreateMap<Subscription, Response>()
            .ForMember(d => d.CategoryName, o => 
                    o.MapFrom(s => s.Category != null ? s.Category.Name : null))
            
            .ForMember(d => d.EquipmentName, o => 
                o.MapFrom(s => s.Equipment != null ? s.Equipment.Name : null));
    }
}
