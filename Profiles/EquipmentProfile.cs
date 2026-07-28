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

        CreateMap<Equipment, Response>();
    }
}