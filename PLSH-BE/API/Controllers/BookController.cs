using Common.Helper;
using Data.DatabaseContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Model.ResponseModel;
using System.IO;
using API.DTO.Book;
using System.Net;
using Common.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Model.Entity;

namespace API.Controllers
{
  [Route("api/v1/book")]
  [ApiController]
  public class BookController : ControllerBase
  {
    private readonly AppDbContext _context;
    private readonly ILogger<ManageAccountController> _logger;

    public BookController(AppDbContext context, ILogger<ManageAccountController> logger)
    {
      _context = context;
      _logger = logger;
    }

    //Lấy sách theo id
    [HttpGet("{id}")] public async Task<IActionResult> GetBookById(int id)
    {
      var bookDto = await (
        from book in _context.Books
        join author in _context.Authors on book.AuthorId equals author.Id
        join category in _context.Categories on book.CategoryId equals category.Id
        where book.Id == id
        select new BookNewDto
        {
          Id = book.Id,
          Title = book.Title,
          Description = book.Description,
          AuthorId = book.AuthorId,
          CategoryId = book.CategoryId,
          Kind = book.Kind,
          Version = book.Version,
          Publisher = book.Publisher,
          PublishDate = book.PublishDate ?? DateTime.MinValue,
          Language = book.Language,
          PageCount = book.PageCount,
          IsbnNumber = book.ISBNumber12,
          Price = book.Price ?? 0.0,
          TotalCopies = book.TotalCopies,
          AvailableCopies = book.AvailableCopies,

          // Thông tin tác giả
          Author = new AuthorDto
          {
            Id = author.Id,
            FullName = author.FullName,
            AvatarUrl = author.AvatarUrl,
            Description = author.Description,
            SummaryDescription = author.SummaryDescription,
            BirthYear = author.BirthYear,
            DeathYear = author.DeathYear
          },

          // Thông tin thể loại
          Category = new API.DTO.Book.CategoryDto
          {
            Id = category.Id, Name = category.Name, Description = category.Description
          }
        }).FirstOrDefaultAsync();
      if (bookDto == null)
      {
        return NotFound(new ErrorResponse
        {
          Status = HttpStatus.NOT_FOUND.GetDescription(),
          StatusCode = HttpStatus.NOT_FOUND,
          Message = "Không tìm thấy sách.",
          Data = new { BookId = id }
        });
      }

      return Ok(new OkResponse
      {
        Status = HttpStatus.OK.GetDescription(),
        StatusCode = HttpStatus.OK,
        Message = "Lấy thông tin sách thành công.",
        Data = bookDto
      });
    }

    //Add book chung
    [HttpPost("add")] public async Task<IActionResult> AddBook([FromForm] BookNewDto bookDto)
    {
      try
      {
        if (bookDto == null)
        {
          return BadRequest(new ErrorResponse
          {
            Status = HttpStatus.BAD_REQUEST.GetDescription(),
            StatusCode = HttpStatus.BAD_REQUEST,
            Message = "Dữ liệu không hợp lệ.",
            Data = null
          });
        }

        // Xác định Kind dựa trên file upload
        if (bookDto.File != null)
        {
          var extension = Path.GetExtension(bookDto.File.FileName).ToLower();
          if (extension == ".pdf")
            bookDto.Kind = (AvailabilityKind)2; // Ebook
          else
            if (extension == ".mp3")
              bookDto.Kind = (AvailabilityKind)3; // Audiobook
            else
              return BadRequest(new ErrorResponse
              {
                Status = HttpStatus.BAD_REQUEST.GetDescription(),
                StatusCode = HttpStatus.BAD_REQUEST,
                Message = "Định dạng file không hợp lệ, chỉ hỗ trợ PDF và MP3.",
                Data = null
              });
        }
        else
        {
          bookDto.Kind = (AvailabilityKind)1; // Physical Book
        }

        // Tạo đối tượng Book từ DTO
        var book = new Book
        {
          Title = bookDto.Title,
          Description = bookDto.Description,
          AuthorId = bookDto.AuthorId,
          CategoryId = bookDto.CategoryId,
          Kind = bookDto.Kind,
          Version = bookDto.Version,
          Publisher = bookDto.Publisher,
          PublishDate = bookDto.PublishDate,
          Language = bookDto.Language,
          PageCount = bookDto.PageCount,
          ISBNumber12 = bookDto.IsbnNumber,
          Price = bookDto.Price,
          TotalCopies = bookDto.TotalCopies,
          AvailableCopies = bookDto.AvailableCopies,
          CreateDate = DateTime.UtcNow,
          UpdateDate = DateTime.UtcNow
        };
        _context.Books.Add(book);
        await _context.SaveChangesAsync();
        return Ok(new OkResponse
        {
          Status = HttpStatus.OK.GetDescription(),
          StatusCode = HttpStatus.OK,
          Message = "Thêm sách thành công.",
          Data = new { BookId = book.Id }
        });
      }
      catch (Exception ex)
      {
        return StatusCode(StatusCodes.Status500InternalServerError,
          new ErrorResponse
          {
            Status = HttpStatusCode.InternalServerError.ToString(),
            StatusCode = HttpStatus.INTERNAL_ERROR,
            Message = "Lỗi máy chủ nội bộ.",
            Data = new { Error = ex.Message }
          });
      }
    }
  }
}