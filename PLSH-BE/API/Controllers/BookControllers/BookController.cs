using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using API.Common;
using API.DTO.Book;
using BU.Services.Interface;
using Common.Enums;
using Common.Helper;
using Data.DatabaseContext;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Model.Entity;
using Model.Entity.book;
using Model.ResponseModel;

namespace API.Controllers.BookControllers
{
  [Route("api/v1/book")]
  [ApiController]
  public partial class BookController(
    AppDbContext context,
    IAuthorService authorService,
    ILogger<BookController> logger,
    IHttpClientFactory httpClientFactory,
    GoogleCloudStorageHelper googleCloudStorageHelper
  )
    : ControllerBase
  {
    // private readonly ILogger<ManageAccountController> _logger = logger;

    // [HttpGet("{id}")] public async Task<IActionResult> GetBookById(int id)
    // {
    //   // httpClientFactory.CreateClient();
    //   var bookDto = await (
    //     from book in context.Books
    //     join author in context.Authors on book.AuthorId equals author.Id
    //     join category in context.Categories on book.CategoryId equals category.Id
    //     where book.Id == id
    //     select new BookNewDto
    //     {
    //       Id = book.Id,
    //       Title = book.Title,
    //       Description = book.Description,
    //       AuthorId = book.AuthorId,
    //       CategoryId = book.CategoryId,
    //       Kind = book.Kind,
    //       Version = book.Version,
    //       Publisher = book.Publisher,
    //       PublishDate = book.PublishDate ?? DateTime.MinValue,
    //       Language = book.Language,
    //       PageCount = book.PageCount,
    //       IsbnNumber = book.ISBNumber13,
    //       Price = book.Price ?? 0.0,
    //       TotalCopies = book.TotalCopies,
    //       AvailableCopies = book.AvailableCopies,
    //
    //       // Thông tin tác giả
    //       Author = new AuthorDto
    //       {
    //         Id = author.Id,
    //         FullName = author.FullName,
    //         AvatarUrl = author.AvatarUrl,
    //         Description = author.Description,
    //         SummaryDescription = author.SummaryDescription,
    //         BirthYear = author.BirthYear,
    //         DeathYear = author.DeathYear
    //       },
    //
    //       // Thông tin thể loại
    //       Category = new API.DTO.Book.CategoryDto
    //       {
    //         Id = category.Id, Name = category.Name, Description = category.Description
    //       }
    //     }).FirstOrDefaultAsync();
    //   if (bookDto == null)
    //   {
    //     return NotFound(new ErrorResponse
    //     {
    //       Status = HttpStatus.NOT_FOUND.GetDescription(),
    //       StatusCode = HttpStatus.NOT_FOUND,
    //       Message = "Không tìm thấy sách.",
    //       Data = new { BookId = id }
    //     });
    //   }
    //
    //   return Ok(new OkResponse
    //   {
    //     Status = HttpStatus.OK.GetDescription(),
    //     StatusCode = HttpStatus.OK,
    //     Message = "Lấy thông tin sách thành công.",
    //     Data = bookDto
    //   });
    // }

    [HttpPost("add")] public async Task<IActionResult> AddBook([FromForm] BookNewDto? bookDto)
    {
      logger.LogInformation(
        $"{nameof(AddBook)}>>===>public async Task<IActionResult> AddBook([FromForm] BookNewDto? bookDto)");
      try
      {
        if (bookDto is null)
        {
          return BadRequest(new ErrorResponse
          {
            Status = HttpStatus.BAD_REQUEST.GetDescription(),
            StatusCode = HttpStatus.BAD_REQUEST,
            Message = "Dữ liệu không hợp lệ.",
            Data = null
          });
        }

        // Khai báo biến để lưu URL file sau khi upload và đối tượng Resource tương ứng
        string? coverImageUrl;
        string? contentPdfUrl;
        string? audioFileUrl;
        Resource? coverResource = null;
        Resource? pdfResource = null;
        Resource? audioResource = null;
        if (bookDto.CoverImage is not null)
        {
          coverImageUrl = await googleCloudStorageHelper.UploadFileAsync(bookDto.CoverImage.OpenReadStream(),
            bookDto.CoverImage.FileName,
            StaticFolder.DIRPath_BOOK_COVER,
            bookDto.CoverImage.ContentType);
          coverResource = new Resource
          {
            Type = "image",
            Name = bookDto.CoverImage.FileName,
            SizeByte = bookDto.CoverImage.Length,
            FileType = bookDto.CoverImage.ContentType,
            LocalUrl = coverImageUrl
          };
          context.Resources.Add(coverResource);
          await context.SaveChangesAsync();
        }

        if (bookDto.ContentPdf is not null)
        {
          contentPdfUrl = await googleCloudStorageHelper.UploadFileAsync(bookDto.ContentPdf.OpenReadStream(),
            bookDto.ContentPdf.FileName,
            StaticFolder.DIRPath_BOOK_PDF,
            bookDto.ContentPdf.ContentType);
          pdfResource = new Resource
          {
            Type = "pdf",
            Name = bookDto.ContentPdf.FileName,
            SizeByte = bookDto.ContentPdf.Length,
            FileType = bookDto.ContentPdf.ContentType,
            LocalUrl = contentPdfUrl
          };
          context.Resources.Add(pdfResource);
          await context.SaveChangesAsync();
        }

        if (bookDto.AudioFile is not null)
        {
          audioFileUrl = await googleCloudStorageHelper.UploadFileAsync(bookDto.AudioFile.OpenReadStream(),
            bookDto.AudioFile.FileName,
            StaticFolder.DIRPath_BOOK_AUDIO,
            bookDto.AudioFile.ContentType);
          audioResource = new Resource
          {
            Type = "audio",
            Name = bookDto.AudioFile.FileName,
            SizeByte = bookDto.AudioFile.Length,
            FileType = bookDto.AudioFile.ContentType,
            LocalUrl = audioFileUrl
          };
          context.Resources.Add(audioResource);
          await context.SaveChangesAsync();
        }

        IList<string> authorNames = new List<string>();
        if (bookDto.Authors is not null && bookDto.Authors.Any())
        {
          authorNames = bookDto.Authors.Select(a => a.FullName.Trim()).ToList();
          await authorService.AddIfAuthorsNameDoesntExistAsync(authorNames);
        }

        IList<Author> processedAuthors = new List<Author>();
        if (authorNames.Any())
        {
          processedAuthors = await context.Authors
                                          .Where(a => authorNames.Contains(a.FullName))
                                          .ToListAsync();
        }

        var book = new Book
        {
          Title = bookDto.Title,
          Description = bookDto.Description,
          Authors = processedAuthors,
          CategoryId = bookDto.CategoryId ?? 0,
          Kind = bookDto.Kind ?? 0,
          Version = bookDto.Version,
          Publisher = bookDto.Publisher,
          PublishDate = bookDto.PublishDate,
          Language = bookDto.Language,
          PageCount = bookDto.PageCount ?? 0,
          IsbNumber13 = bookDto.IsbnNumber13,
          Price = bookDto.Price,
          TotalCopies = bookDto.TotalCopies ?? 0,
          AvailableCopies = bookDto.AvailableCopies ?? 0,
          CreateDate = DateTime.UtcNow,
          UpdateDate = DateTime.UtcNow
        };
        if (coverResource is not null)
        {
          book.CoverImageResourceId = coverResource.Id;
          book.Thumbnail = coverResource.LocalUrl;
        }

        if (pdfResource is not null) { book.PreviewPdfResourceId = pdfResource.Id; }

        if (audioResource is not null) { book.AudioResourceId = audioResource.Id; }

        context.Books.Add(book);
        await context.SaveChangesAsync();
        return Ok(new OkResponse
        {
          Status = HttpStatus.OK.GetDescription(),
          StatusCode = HttpStatus.OK,
          Message = "Thêm sách thành công.",
          Data = new { BookId = book.Id, },
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
            Data = new { Error = ex.Message, },
          });
      }
    }
  }
}