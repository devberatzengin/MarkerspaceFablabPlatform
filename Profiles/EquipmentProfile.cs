using AutoMapper;
using MakerspaceFablabPlatform.Dtos.Equipment;
using MakerspaceFablabPlatform.Entities;

namespace MakerspaceFablabPlatform.Profiles;

public class EquipmentProfile : Profile
{
    public EquipmentProfile()
    {
        CreateMap<UpdateRequest, Equipment>()
            .ForAllMembers(opts =>
                opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<Equipment, Response>()
            .ForMember(dest => dest.CurrentUserId,
                opt => opt.MapFrom(src =>
                    src.EquipmentRentals
                        .Where(r => r.ReleasedAt == null)
                        .Select(r => (Guid?)r.UserId)
                        .FirstOrDefault()));
        
        CreateMap<EquipmentRental, Response>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.EquipmentId))
            .ForMember(dest => dest.CurrentUserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.AvailableAt, opt => opt.MapFrom(src => src.ExpectedReturnAt))
            .ForMember(dest => dest.Name, opt => opt.Ignore()) // Equipment'tan gelecek
            .ForMember(dest => dest.Description, opt => opt.Ignore())
            .ForMember(dest => dest.Type, opt => opt.Ignore())
            .ForMember(dest => dest.PlacementType, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore());

    }
}