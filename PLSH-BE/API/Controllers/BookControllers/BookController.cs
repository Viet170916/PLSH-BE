using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using API.Common;
using API.DTO.Book;
using AutoMapper;
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
    IMapper mapper,
    GoogleCloudStorageHelper googleCloudStorageHelper
  )
    : ControllerBase
  {
    [HttpGet("{id}")] public async Task<IActionResult> GetBookById(int id)
    {
      var book = await context.Books
                              .Include(b => b.Authors)
                              .Include(b => b.Category)
                              .FirstOrDefaultAsync(b => b.Id == id);
      if (book == null) return NotFound();
      return Ok(book);
    }

    [HttpGet] public async Task<IActionResult> SearchBooks(
      [FromQuery] string? keyword,
      [FromQuery] int? categoryId,
      [FromQuery] int? authorId,
      [FromQuery] int page = 1,
      [FromQuery] int limit = 10,
      [FromQuery] string orderBy = "title",
      [FromQuery] bool descending = false
    )
    {
      limit = Math.Clamp(limit, 1, 40);
      var query = context.Books
                         .Include(b => b.Authors)
                         .Include(b => b.Category)
                         .Include(b => b.AudioResource)
                         .Include(b => b.CoverImageResource)
                         .Include(b => b.PreviewPdfResource)
                         .AsQueryable();
      if (!string.IsNullOrEmpty(keyword)) query = query.Where(b => b.Title.Contains(keyword));
      if (categoryId.HasValue) query = query.Where(b => b.CategoryId == categoryId);
      if (authorId.HasValue) query = query.Where(b => b.Authors.Any(a => a.Id == authorId));
      query = orderBy.ToLower() switch
      {
        "title" => descending ? query.OrderByDescending(b => b.Title) : query.OrderBy(b => b.Title),
        "createdate" => descending ? query.OrderByDescending(b => b.CreateDate) : query.OrderBy(b => b.CreateDate),
        _ => query.OrderBy(b => b.Title)
      };
      var totalRecords = await query.CountAsync();
      var booksQueryPagging = query.Skip((page - 1) * limit).Take(limit);
      var books = await booksQueryPagging.ToListAsync();
      var bookDtos = mapper.Map<List<BookNewDto>>(books);
      var response = new
      {
        PageCount = (int)Math.Ceiling((double)totalRecords / limit),
        CurrentPage = page,
        TotalBook = totalRecords,
        Data = bookDtos,
      };
      return Ok(response);
    }

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

        string? coverImageUrl;
        string? contentPdfUrl;
        string? audioFileUrl;
        Resource? coverResource = null;
        Resource? pdfResource = null;
        Resource? audioResource = null;

        // if (bookDto.CoverImage is not null)
        // {
        //   coverImageUrl = await googleCloudStorageHelper.UploadFileAsync(bookDto.CoverImage.OpenReadStream(),
        //     bookDto.CoverImage.FileName,
        //     StaticFolder.DIRPath_BOOK_COVER,
        //     bookDto.CoverImage.ContentType);
        //   coverResource = new Resource
        //   {
        //     Type = "image",
        //     Name = bookDto.CoverImage.FileName,
        //     SizeByte = bookDto.CoverImage.Length,
        //     FileType = bookDto.CoverImage.ContentType,
        //     LocalUrl = coverImageUrl
        //   };
        //   context.Resources.Add(coverResource);
        //   await context.SaveChangesAsync();
        // }
        //
        // if (bookDto.ContentPdf is not null)
        // {
        //   contentPdfUrl = await googleCloudStorageHelper.UploadFileAsync(bookDto.ContentPdf.OpenReadStream(),
        //     bookDto.ContentPdf.FileName,
        //     StaticFolder.DIRPath_BOOK_PDF,
        //     bookDto.ContentPdf.ContentType);
        //   pdfResource = new Resource
        //   {
        //     Type = "pdf",
        //     Name = bookDto.ContentPdf.FileName,
        //     SizeByte = bookDto.ContentPdf.Length,
        //     FileType = bookDto.ContentPdf.ContentType,
        //     LocalUrl = contentPdfUrl
        //   };
        //   context.Resources.Add(pdfResource);
        //   await context.SaveChangesAsync();
        // }
        //
        // if (bookDto.AudioFile is not null)
        // {
        //   audioFileUrl = await googleCloudStorageHelper.UploadFileAsync(bookDto.AudioFile.OpenReadStream(),
        //     bookDto.AudioFile.FileName,
        //     StaticFolder.DIRPath_BOOK_AUDIO,
        //     bookDto.AudioFile.ContentType);
        //   audioResource = new Resource
        //   {
        //     Type = "audio",
        //     Name = bookDto.AudioFile.FileName,
        //     SizeByte = bookDto.AudioFile.Length,
        //     FileType = bookDto.AudioFile.ContentType,
        //     LocalUrl = audioFileUrl
        //   };
        //   context.Resources.Add(audioResource);
        //   await context.SaveChangesAsync();
        // }
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
          Thumbnail = bookDto.Thumbnail,
          Kind = bookDto.Kind ?? 0,
          Version = bookDto.Version,
          Publisher = bookDto.Publisher,
          Width = bookDto.Width,
          Height = bookDto.Height,
          Thickness = bookDto.Thickness,
          Weight = bookDto.Weight,
          PublishDate = bookDto.PublishDate,
          Language = bookDto.Language,
          PageCount = bookDto.PageCount ?? 0,
          IsbNumber13 = bookDto.IsbnNumber13,
          IsbNumber10 = bookDto.IsbnNumber10,
          OtherIdentifier = bookDto.OtherIdentifier,
          Price = bookDto.Price,
          TotalCopies = bookDto.TotalCopies ?? 0,
          AvailableCopies = bookDto.AvailableCopies ?? 0,
          CreateDate = DateTime.UtcNow,
          UpdateDate = DateTime.UtcNow
        };
        if (bookDto.CategoryId is not null || (bookDto.Category?.Id is not null && bookDto.Category.Id != 0))
        {
          book.CategoryId = bookDto.CategoryId ?? bookDto.Category?.Id;
        }
        else
          if (bookDto.Category is not null) { book.Category = new Category { Name = bookDto.Category.Name }; }

        
        // if (coverResource is not null)
        // {
        //   book.CoverImageResourceId = coverResource.Id;
        //   book.Thumbnail = coverResource.LocalUrl;
        // }
        //
        // if (pdfResource is not null) { book.PreviewPdfResourceId = pdfResource.Id; }
        //
        // if (audioResource is not null) { book.AudioResourceId = audioResource.Id; }

        context.Books.Add(book);
        await context.SaveChangesAsync();
        
        if (bookDto.AudioResource is not null)
        {
          book.AudioResource = new Resource()
          {
            Type = "audio", LocalUrl = $"{StaticFolder.DIRPath_BOOK_AUDIO}/book_{book.Id}/{bookDto.AudioResource.Name}"
          };
        }

        if (bookDto.CoverImageResource is not null)
        {
          book.CoverImageResource = new Resource()
          {
            Type = "image", LocalUrl = $"{StaticFolder.DIRPath_BOOK_AUDIO}/book_{book.Id}/{bookDto.CoverImageResource.Name}"
          };
        }

        if (bookDto.PreviewPdfResource is not null)
        {
          book.PreviewPdfResource = new Resource()
          {
            Type = "audio", LocalUrl = $"{StaticFolder.DIRPath_BOOK_AUDIO}/book_{book.Id}/{bookDto.PreviewPdfResource.Name}"
          };
        }
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