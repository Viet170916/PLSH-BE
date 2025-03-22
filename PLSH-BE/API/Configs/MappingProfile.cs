using API.Common;
using API.Controllers.ResourceControllers;
using API.DTO.Book;
using API.DTO.LibRoomDto;
using Model.Entity;
using Model.Entity.book;
using Model.Entity.book.Dto;
using Model.Entity.LibraryRoom;
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
    CreateMap<BookInstance, LibraryRoomDto.BookInstanceDto>()
      .ForMember(dest => dest.BookId, opt => opt.MapFrom(src => src.BookId))
      .ForMember(dest => dest.RowShelfId, opt => opt.MapFrom(src => src.RowShelfId))
      .ReverseMap();
    CreateMap<RowShelf, LibraryRoomDto.RowShelfDto>()
      .ForMember(dest => dest.BookInstances, opt => opt.MapFrom(src => src.BookInstances))
      .ForMember(dest => dest.Count, opt => opt.MapFrom(src => (src.BookInstances.Count)))
      .ReverseMap();
    CreateMap<Shelf, LibraryRoomDto.ShelfDto>()
      .ForMember(dest => dest.RowShelves, opt => opt.MapFrom(src => src.RowShelves))
      .ReverseMap();
    CreateMap<LibraryRoom, LibRoomDto.LibraryRoomDto>()
      .ReverseMap();
  }
}