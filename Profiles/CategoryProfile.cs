using AutoMapper;
using MakerspaceFablabPlatform.Dtos.Category;
using MakerspaceFablabPlatform.Entities;

namespace MakerspaceFablabPlatform.Profiles;

public class CategoryProfile : Profile
{
    public CategoryProfile()
    {
        CreateMap<Category, Response>();
    }
}
