using AutoMapper;
using MakerspaceFablabPlatform.Dtos.Equipment;
using MakerspaceFablabPlatform.Dtos.EquipmentRental;
using MakerspaceFablabPlatform.Entities;
using MakerspaceFablabPlatform.Entities.Enums;
using Response = MakerspaceFablabPlatform.Dtos.Equipment.Response;
using EquipmentRentalResponse = MakerspaceFablabPlatform.Dtos.EquipmentRental.Response;
using EquipmentRentalEntity = MakerspaceFablabPlatform.Entities.EquipmentRental;

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
                        .FirstOrDefault()))
            .ForMember(dest => dest.AvailableAt,
                opt => opt.MapFrom(src =>
                    src.EquipmentRentals
                        .Where(r => r.ReleasedAt == null)
                        .Select(r => (DateTime?)r.ExpectedReturnAt)
                        .FirstOrDefault() ?? DateTime.UtcNow));
        
        CreateMap<EquipmentRentalEntity, EquipmentRentalResponse>()
            .ForMember(dest => dest.EquipmentName,
                opt => opt.MapFrom(src => src.Equipment.Name))
            .ForMember(dest => dest.EquipmentDescription,
                opt => opt.MapFrom(src => src.Equipment.Description));

    }
}