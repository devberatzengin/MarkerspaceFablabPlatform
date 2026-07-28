using AutoMapper;
using MakerspaceFablabPlatform.Dtos.Announcement;
using MakerspaceFablabPlatform.Entities;

namespace MakerspaceFablabPlatform.Profiles;

public class AnnouncementProfile : Profile
{
    public AnnouncementProfile()
    {
        CreateMap<Announcement, Response>()
            .ForMember(dest => dest.CreatedByName,
                opt => opt.MapFrom(src => src.CreatedBy.FirstName + " " + src.CreatedBy.LastName))
            .ForMember(dest => dest.CategoryName,
                opt => opt.MapFrom(src => src.Category.Name));
    }
}
