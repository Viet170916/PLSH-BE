using API.Common;
using API.Controllers.ResourceControllers;
using API.DTO.Book;
using Model.Entity;
using Model.Entity.book;
using Profile = AutoMapper.Profile;

namespace API.Configs;

public class MappingProfile : Profile
{
  public MappingProfile()
  {
    CreateMap<Book, BookNewDto>()
      .ForMember(dest => dest.Thumbnail,
        opt => opt.MapFrom(src => src.Thumbnail ?? Converter.ToImageUrl(src.CoverImageResource.LocalUrl)))
      .ForMember(dest => dest.IsbnNumber13, opt => opt.MapFrom(src => src.IsbNumber13))
      .ForMember(dest => dest.IsbnNumber10, opt => opt.MapFrom(src => src.IsbNumber10))
      .ForMember(dest => dest.Thumbnail, opt => opt.MapFrom(src => src.Thumbnail))
      .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category))
      .ForMember(dest => dest.Authors, opt => opt.MapFrom(src => src.Authors))
      .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.BookInstances.Count));
    CreateMap<Category, CategoryDto>().ReverseMap();
    CreateMap<Author, AuthorDto>().ReverseMap();
    CreateMap<Book, BookDto>().ReverseMap();
    CreateMap<ResourceDto, Resource>().ReverseMap();
  }
}