using API.Common;
using API.Controllers.ResourceControllers;
using BU.Models.DTO.Account.AccountDTO;
using BU.Models.DTO.Book;
using BU.Models.DTO.Loan;
using BU.Models.DTO.Notification;
using Model.Entity;
using Model.Entity.book;
using Model.Entity.Book.e_book;
using Model.Entity.Borrow;
using Model.Entity.LibraryRoom;
using Model.Entity.Notification;
using Model.Entity.User;
using LibraryRoomDto = Model.Entity.book.Dto.LibraryRoomDto;
using Profile = AutoMapper.Profile;

namespace API.Configs;

public class MappingProfile : Profile
{
  public MappingProfile()
  {
    CreateMap<EBookChapter, EBookChapterDto.EBookChapterFulltDto>();
    CreateMap<EBookChapter, EBookChapterDto.EBookChapterHtmlContentDto>();
    CreateMap<EBookChapter, EBookChapterDto.EBookChapterPlainTextDto>();
    CreateMap<Book, BookMinimalDto>()
      .ForMember(dest => dest.Thumbnail,
        opt => opt.MapFrom(src =>
          src.Thumbnail ??
          Converter.ToImageUrl(src.CoverImageResource != null ? src.CoverImageResource.LocalUrl : null)))
      .ForMember(dest => dest.Rating,
        opt => opt.MapFrom(
          src => src.Reviews != null && src.Reviews.Count != 0 ? src.Reviews.Average(r => r.Rating) : 0))
      // .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category))
      .ForMember(dest => dest.Authors, opt => opt.MapFrom(src => src.Authors));
    // .ForMember(dest => dest.Quantity,
    // opt => opt.MapFrom(src => src.BookInstances != null ? src.BookInstances.Count : 0))
    // .ForMember(dest => dest.PublishDate, opt => opt.MapFrom(src => src.PublishDate));
    CreateMap<Book, BookNewDto>()
      .ForMember(dest => dest.Thumbnail,
        opt => opt.MapFrom(src =>
          src.Thumbnail ??
          Converter.ToImageUrl(src.CoverImageResource != null ? src.CoverImageResource.LocalUrl : null)))
      .ForMember(dest => dest.IsbnNumber13, opt => opt.MapFrom(src => src.IsbNumber13))
      .ForMember(dest => dest.HasEbook,
        opt => opt.MapFrom(src => src.EBookChapters != null && src.EBookChapters.Count > 0))
      .ForMember(dest => dest.Rating,
        opt => opt.MapFrom(
          src => src.Reviews != null && src.Reviews.Count != 0 ? src.Reviews.Average(r => r.Rating) : 0))
      .ForMember(dest => dest.IsbnNumber10, opt => opt.MapFrom(src => src.IsbNumber10))
      .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category))
      .ForMember(dest => dest.Authors, opt => opt.MapFrom(src => src.Authors))
      // .ForMember(dest => dest.Quantity,
      //   opt => opt.MapFrom(src => src.BookInstances != null ? src.BookInstances.Count : 0))
      .ForMember(dest => dest.PublishDate, opt => opt.MapFrom(src => src.PublishDate))
      // .ForMember(dest => dest.AvailableBookCount,
      //   opt => opt.MapFrom(src => src.BookInstances != null ? src.BookInstances.Count(b => !b.IsInBorrowing) : 0))
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
      .ForMember(dest => dest.ShelfId, opt => opt.MapFrom(src => src.RowShelf.ShelfId))
      .ForMember(dest => dest.BookName, opt => opt.MapFrom(src => src.Book.Title))
      .ForMember(dest => dest.BookVersion, opt => opt.MapFrom(src => src.Book.Version))
      .ForMember(dest => dest.BookThumbnail,
        opt => opt.MapFrom(src =>
          src.Book.Thumbnail ??
          Converter.ToImageUrl(src.Book.CoverImageResource != null ? src.Book.CoverImageResource.LocalUrl : null)))
      .ForMember(dest => dest.BookAuthor,
        opt => opt.MapFrom(src => src.Book.Authors != null ? src.Book.Authors.FirstOrDefault()!.FullName : null))
      .ForMember(dest => dest.BookCategory, opt => opt.MapFrom(src => src.Book.Category!.Name))
      .ForMember(dest => dest.ShelfPosition,
        opt => opt.MapFrom(src => src.RowShelfId != null ?
          $"{src.RowShelf.Shelf.Name ?? $"Kệ [x-{src.RowShelf.Shelf.X},y-{src.RowShelf.Shelf.Y}]"},{src.RowShelf.Name ?? "chưa đặt tên hàng"}, Ngăn {src.Position}" :
          "Chưa có trên kệ"))
      .ReverseMap();
    CreateMap<RowShelf, LibraryRoomDto.RowShelfDto>()
      .ForMember(dest => dest.BookInstances, opt => opt.MapFrom(src => src.BookInstances))
      .ForMember(dest => dest.Count, opt => opt.MapFrom(src => src.BookInstances!.Count))
      .ReverseMap();
    CreateMap<Shelf, LibraryRoomDto.ShelfDto>()
      .ForMember(dest => dest.RowShelves, opt => opt.MapFrom(src => src.RowShelves))
      .ForMember(dest => dest.Name,
        opt => opt.MapFrom(src => src.Type.Contains("SHELF") ? src.Name ?? $"Kệ [{src.X},{src.Y}]" : src.Name ?? null))
      .ReverseMap();
    CreateMap<LibraryRoom, LibraryRoomDto>()
      .ReverseMap();
    CreateMap<Account, AccountGDto>()
      .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.Name))
      .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
      .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => src.ClassRoom))
      .ReverseMap()
      .ForMember(des => des.Role, opt => opt.Ignore())
      .ForMember(des => des.ClassRoom, opt => opt.MapFrom(src => src.ClassRoom ?? src.ClassName));
    CreateMap<Loan, LoanDto>()
      .ForMember(dest => dest.BookCount, opt => opt.MapFrom(src => src.BookBorrowings.Count))
      .ReverseMap();
    CreateMap<BookBorrowing, BookBorrowingDto>()
      .ForMember(dest => dest.BookImageUrlsBeforeBorrow,
        opt => opt.MapFrom(src => src.BookImagesBeforeBorrow.Select(img => Converter.ToImageUrl(img.LocalUrl))))
      .ForMember(dest => dest.BookImageUrlsAfterBorrow,
        opt => opt.MapFrom(src => src.BookImagesAfterBorrow.Select(img => Converter.ToImageUrl(img.LocalUrl))))
      .ForMember(dest => dest.BorrowingStatus,
        opt => opt.MapFrom(src =>
          (src.BorrowingStatus != "returned" && src.ReturnDates.Count > 0 && src.ReturnDates.Max() < DateTime.UtcNow) ?
            "overdue" :
            src.BorrowingStatus))
      .ForMember(dest => dest.overdueDays,
        opt => opt.MapFrom(src => DateTimeConverter.CalculateOverdueDays(src.BorrowDate, src.ReturnDates,
          src.ExtendDates, src.ActualReturnDate ?? DateTime.UtcNow)))
      .ReverseMap();
    CreateMap<BookBorrowing, BookBorrowingDetailDto>()
      .ForMember(dest => dest.BorrowerAvatar,
        opt => opt.MapFrom(src => src.Loan.Borrower.AvatarUrl))
      .ForMember(dest => dest.BorrowerEmail,
        opt => opt.MapFrom(src => src.Loan.Borrower.Email))
      .ForMember(dest => dest.BorrowerPhone,
        opt => opt.MapFrom(src => src.Loan.Borrower.PhoneNumber))
      .ForMember(dest => dest.BorrowerClass,
        opt => opt.MapFrom(src => src.Loan.Borrower.ClassRoom))
      .ForMember(dest => dest.BorrowerFullName,
        opt => opt.MapFrom(src => src.Loan.Borrower.FullName))
      .ForMember(dest => dest.BorrowerRole,
        opt => opt.MapFrom(src => src.Loan.Borrower.Role.Name))
      .ForMember(dest => dest.BorrowerId,
        opt => opt.MapFrom(src => src.Loan.BorrowerId))
      .ForPath(dest => dest.BookInstance.RowShelfId,
        opt => opt.Ignore())
      //
      .ForMember(dest => dest.BookImageUrlsBeforeBorrow,
        opt => opt.MapFrom(src => src.BookImagesBeforeBorrow.Select(img => Converter.ToImageUrl(img.LocalUrl))))
      .ForMember(dest => dest.BookImageUrlsAfterBorrow,
        opt => opt.MapFrom(src => src.BookImagesAfterBorrow.Select(img => Converter.ToImageUrl(img.LocalUrl))))
      .ForMember(dest => dest.BorrowingStatus,
        opt => opt.MapFrom(src =>
          (src.BorrowingStatus != "returned" && src.ReturnDates.Count > 0 && src.ReturnDates.Max() < DateTime.UtcNow) ?
            "overdue" :
            src.BorrowingStatus))
      .ForMember(dest => dest.OverdueDays,
        opt => opt.MapFrom(src => DateTimeConverter.CalculateOverdueDays(src.BorrowDate, src.ReturnDates,
          src.ExtendDates, src.ActualReturnDate ?? DateTime.UtcNow)))
      .ReverseMap();
    CreateMap<Review, ReviewDto>()
      .ForMember(dest => dest.ResourceUrl,
        opt => opt.MapFrom(src => Converter.ToImageUrl(src.Resource!.LocalUrl)))
      .ForMember(dest => dest.AccountSenderAvatar,
        opt => opt.MapFrom(src => src.AccountSender.AvatarUrl))
      .ForMember(dest => dest.AccountSenderName,
        opt => opt.MapFrom(src => src.AccountSender.FullName))
      .ForMember(dest => dest.IsYouAreSender, opt => opt.MapFrom((src, _, _, context) =>
      {
        var currentUserId = context.Items.TryGetValue("CurrentUserId", out var value) ? (int)value : 0;
        return src.AccountSenderId == currentUserId;
      }))
      .ReverseMap();
    CreateMap<Notification, NotificationDto>()
      .ForMember(dest => dest.ReferenceData, opt => opt.MapFrom((_, _, _, context) =>
      {
        var referenceData = context.Items.TryGetValue("ReferenceData", out var value) ? value : null;
        return referenceData;
      }))
      .ReverseMap();
    CreateMap<Message, MessageDto>()
      .ForMember(dest => dest.ResourceUrl,
        opt => opt.MapFrom(src => Converter.ToImageUrl(src.Resource!.LocalUrl)))
      .ForMember(dest => dest.SenderAvatar,
        opt => opt.MapFrom(mapExpression: src => src.Sender.AvatarUrl))
      .ForMember(dest => dest.RepliedAvatar,
        opt => opt.MapFrom(mapExpression: src => src.RepliedPerson!.AvatarUrl))
      .ForMember(dest => dest.SenderName,
        opt => opt.MapFrom(mapExpression: src => src.Sender.FullName))
      .ForMember(dest => dest.RepliedPersonName,
        opt => opt.MapFrom(src => src.RepliedPerson!.FullName))
      .ForMember(dest => dest.IsYouAreSender, opt => opt.MapFrom((src, _, _, context) =>
      {
        var currentUserId = context.Items.TryGetValue("CurrentUserId", out var value) ? (int)value : 0;
        return src.SenderId == currentUserId;
      }))
      .ReverseMap();
  }
}
