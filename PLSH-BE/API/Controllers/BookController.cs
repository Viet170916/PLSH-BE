using System.Collections.Generic;
using Common.Helper;
using Data.DatabaseContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Model.ResponseModel;
using System.IO;
using API.DTO.Book;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using Common.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Model.Entity;

namespace API.Controllers
{
  [Route("api/v1/book")]
  [ApiController]
  public class BookController(
    AppDbContext context,
    ILogger<ManageAccountController> logger,
    IHttpClientFactory httpClientFactory
  )
    : ControllerBase
  {
    private readonly ILogger<ManageAccountController> _logger = logger;

    [HttpGet("{id}")] public async Task<IActionResult> GetBookById(int id)
    {
      var bookDto = await (
        from book in context.Books
        join author in context.Authors on book.AuthorId equals author.Id
        join category in context.Categories on book.CategoryId equals category.Id
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
          IsbnNumber = book.ISBNumber13,
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
          ISBNumber13 = bookDto.IsbnNumber,
          Price = bookDto.Price,
          TotalCopies = bookDto.TotalCopies,
          AvailableCopies = bookDto.AvailableCopies,
          CreateDate = DateTime.UtcNow,
          UpdateDate = DateTime.UtcNow
        };
        context.Books.Add(book);
        await context.SaveChangesAsync();
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

    [HttpGet("global/search")] public async Task<IActionResult> GetIsbn(string? isbn, string? keyword)
    {
      if (string.IsNullOrWhiteSpace(isbn) && string.IsNullOrWhiteSpace(keyword))
      {
        return BadRequest("Bạn phải cung cấp isbn hoặc keyword để tìm kiếm.");
      }

      var query = !string.IsNullOrWhiteSpace(isbn) ? $"isbn:{isbn}" : keyword!;
      var apiKey = Environment.GetEnvironmentVariable("GOOGLE_BOOK_API_KEY");
      var baseUrl = string.IsNullOrWhiteSpace(apiKey) ?
        $"https://www.googleapis.com/books/v1/volumes?q={Uri.EscapeDataString(query)}" :
        $"https://www.googleapis.com/books/v1/volumes?q={Uri.EscapeDataString(query)}&key={apiKey}";
      var client = httpClientFactory.CreateClient();
      string urlWithLang = baseUrl + "&langRestrict=vi";
      var response = await client.GetAsync(urlWithLang);
      if (!response.IsSuccessStatusCode) { return NotFound("Lỗi khi gọi Google Books API với langRestrict=vi."); }

      var json = await response.Content.ReadAsStringAsync();
      using JsonDocument doc = JsonDocument.Parse(json);
      var root = doc.RootElement;
      List<JsonElement> items = [];
      if (root.TryGetProperty("items", out JsonElement itemsElem) && itemsElem.ValueKind == JsonValueKind.Array)
      {
        foreach (var item in itemsElem.EnumerateArray()) { items.Add(item); }
      }

      if (items.Count == 0)
      {
        string urlNoLang = baseUrl;
        response = await client.GetAsync(urlNoLang);
        if (!response.IsSuccessStatusCode)
        {
          return NotFound("Lỗi khi gọi Google Books API mà không có langRestrict.");
        }

        json = await response.Content.ReadAsStringAsync();
        using JsonDocument docFallback = JsonDocument.Parse(json);
        var rootFallback = docFallback.RootElement;
        if (rootFallback.TryGetProperty("items", out JsonElement itemsElemFallback) &&
            itemsElemFallback.ValueKind == JsonValueKind.Array)
        {
          items.Clear();
          foreach (var item in itemsElemFallback.EnumerateArray()) { items.Add(item); }
        }
      }

      List<Book> books = [];
      var booksCount = 0;
      foreach (var item in items)
      {
        if (!item.TryGetProperty("volumeInfo", out JsonElement volumeInfo)) continue;
        var book = new Book
        {
          Id = booksCount++,
          Title = volumeInfo.TryGetProperty("title", out var titleElem) ? titleElem.GetString() : null,
          Description = volumeInfo.TryGetProperty("description", out var descElem) ? descElem.GetString() : null,
          Publisher =
            volumeInfo.TryGetProperty("publisher", out var publisherElem) ? publisherElem.GetString() : null,
          Language = volumeInfo.TryGetProperty("language", out var langElem) ? langElem.GetString() : null,
          PageCount = volumeInfo.TryGetProperty("pageCount", out var pageCountElem) ? pageCountElem.GetInt32() : 0,
          Rating = volumeInfo.TryGetProperty("averageRating", out var ratingElem) ?
            (float?)ratingElem.GetDouble() :
            null,
          Thumbnail = volumeInfo.TryGetProperty("imageLinks", out var imageLinksElem) &&
                      imageLinksElem.TryGetProperty("thumbnail", out var thumbElem) ?
            thumbElem.GetString() :
            null,
          PublishDate = volumeInfo.TryGetProperty("publishedDate", out var dateElem) &&
                        DateTime.TryParse(dateElem.GetString(), out DateTime pubDate) ?
            pubDate :
            null,
          AuthorId = 0,
          CoverImageResourceId = null,
          PreviewPdfResourceId = null,
          AudioResourceId = null,
          Version = null,
          CategoryId = 0,
          ISBNumber13 = null,
          IsbNumber10 = null,
          TotalCopies = 0,
          AvailableCopies = 0,
          Price = null,
          Fine = null,
          CreateDate = DateTime.UtcNow,
          UpdateDate = null,
          DeletedAt = null,
          IsChecked = false,
          BookReviewId = 0,
          Quantity = 0,
          Availabilities = [],
        };
        if (volumeInfo.TryGetProperty("industryIdentifiers", out JsonElement identifiers) &&
            identifiers.ValueKind == JsonValueKind.Array)
        {
          foreach (var identifier in identifiers.EnumerateArray())
          {
            if (!identifier.TryGetProperty("type", out var typeElem) ||
                !identifier.TryGetProperty("identifier", out var idElem))
              continue;
            var type = typeElem.GetString() ?? "";
            var idValue = idElem.GetString() ?? "";
            switch (type)
            {
              case "ISBN_13": book.ISBNumber13 = idValue; break;
              case "ISBN_10": book.IsbNumber10 = idValue; break;
              default: book.OtherIdentifier = idValue; break;
            }
          }
        }

        if (volumeInfo.TryGetProperty("authors", out JsonElement authorsElem) &&
            authorsElem.ValueKind == JsonValueKind.Array)
        {
          string? firstAuthor = authorsElem[0].GetString();
          if (!string.IsNullOrWhiteSpace(firstAuthor)) { book.Author = new Author { FullName = firstAuthor }; }
        }

        if (volumeInfo.TryGetProperty("categories", out JsonElement categoriesElem) &&
            categoriesElem.ValueKind == JsonValueKind.Array)
        {
          var categories = new List<string>();
          foreach (var cat in categoriesElem.EnumerateArray())
          {
            if (cat.ValueKind == JsonValueKind.String) categories.Add(cat.GetString()!);
          }

          book.Category = new Category() { Name = categories.FirstOrDefault() };
        }

        if (volumeInfo.TryGetProperty("contentVersion", out JsonElement contentVersionElem) &&
            contentVersionElem.ValueKind == JsonValueKind.String) { book.Version = contentVersionElem.GetString(); }

        books.Add(book);
      }

      return Ok(books);
    }
  }
}