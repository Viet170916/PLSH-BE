using API.Common;
using API.Controllers.ResourceControllers;
using API.DTO.Account.AccountDTO;
using API.DTO.Book;
using Model.Entity;
using Model.Entity.book;
using Model.Entity.LibraryRoom;
using Model.Entity.User;
using LibraryRoomDto = Model.Entity.book.Dto.LibraryRoomDto;
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
      .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.BookInstances.Count))
      .ReverseMap()
      .ForMember(dest => dest.IsbNumber10, opt => opt.MapFrom(src => src.IsbnNumber13))
      .ForMember(dest => dest.IsbNumber10, opt => opt.MapFrom(src => src.IsbnNumber10));
    CreateMap<Category, CategoryDto>().ReverseMap();
    CreateMap<Author, AuthorDto>().ReverseMap();
    CreateMap<Book, BookDto>().ReverseMap();
    CreateMap<ResourceDto, Resource>().ReverseMap();
    CreateMap<BookInstance, LibraryRoomDto.BookInstanceDto>()
      .ForMember(dest => dest.BookId, opt => opt.MapFrom(src => src.BookId))
      .ForMember(dest => dest.RowShelfId, opt => opt.MapFrom(src => src.RowShelfId))
      .ForMember(dest => dest.BookName, opt => opt.MapFrom(src => src.Book.Title))
      .ForMember(dest => dest.BookVersion, opt => opt.MapFrom(src => src.Book.Version))
      .ForMember(dest => dest.BookThumbnail,
        opt => opt.MapFrom(src => src.Book.Thumbnail ?? Converter.ToImageUrl(src.Book.CoverImageResource.LocalUrl)))
      .ForMember(dest => dest.BookAuthor, opt => opt.MapFrom(src => src.Book.Authors.FirstOrDefault().FullName))
      .ForMember(dest => dest.BookCategory, opt => opt.MapFrom(src => src.Book.Category.Name))
      .ReverseMap();
    CreateMap<RowShelf, LibraryRoomDto.RowShelfDto>()
      .ForMember(dest => dest.BookInstances, opt => opt.MapFrom(src => src.BookInstances))
      .ForMember(dest => dest.Count, opt => opt.MapFrom(src => (src.BookInstances.Count)))
      .ReverseMap();
    CreateMap<Shelf, LibraryRoomDto.ShelfDto>()
      .ForMember(dest => dest.RowShelves, opt => opt.MapFrom(src => src.RowShelves))
      .ReverseMap();
    CreateMap<LibraryRoom, LibraryRoomDto>()
      .ReverseMap();
    CreateMap<Account, AccountGDto>()
      .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.Name))
      .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
      .ReverseMap()
      .ForMember(des => des.Role, opt => opt.Ignore());
  }
}