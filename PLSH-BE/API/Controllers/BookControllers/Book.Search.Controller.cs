using System.Collections.Generic;
using System.Text.Json;
using API.Common;
using API.DTO.Book;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model.Entity.book;

namespace API.Controllers.BookControllers;

public partial class BookController
{
  [HttpGet("{id}")] public async Task<IActionResult> GetBookById(int id)
  {
    var book = await context.Books
                            .Include(b => b.Authors)
                            .Include(b => b.Category)
                            .Include(b => b.AudioResource)
                            .Include(b => b.BookInstances)
                            .Include(b => b.CoverImageResource)
                            .Include(b => b.EpubResource)
                            .Include(b => b.PreviewPdfResource)
                            .FirstOrDefaultAsync(b => b.Id == id);
    var bookDto = mapper.Map<BookNewDto>(book);
    if (bookDto == null) return NotFound();
    return Ok(bookDto);
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
                       .Include(b => b.EpubResource)
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
    var client = httpClientFactory.CreateClient("Book");
    var urlWithLang = baseUrl + "&langRestrict=vi";
    var response = await client.GetAsync(urlWithLang);
    if (!response.IsSuccessStatusCode) { return NotFound("Lỗi khi gọi Google Books API với langRestrict=vi."); }

    var json = await response.Content.ReadAsStringAsync();
    using JsonDocument doc = JsonDocument.Parse(json);
    var root = doc.RootElement;
    List<JsonElement> items = [];
    if (root.TryGetProperty("items", out JsonElement itemsElem) && itemsElem.ValueKind == JsonValueKind.Array)
    {
      items.AddRange(itemsElem.EnumerateArray());
    }

    if (items.Count == 0)
    {
      var urlNoLang = baseUrl;
      response = await client.GetAsync(urlNoLang);
      if (!response.IsSuccessStatusCode) { return NotFound("Lỗi khi gọi Google Books API mà không có langRestrict."); }

      json = await response.Content.ReadAsStringAsync();
      using JsonDocument docFallback = JsonDocument.Parse(json);
      var rootFallback = docFallback.RootElement;
      if (rootFallback.TryGetProperty("items", out JsonElement itemsElemFallback) &&
          itemsElemFallback.ValueKind == JsonValueKind.Array)
      {
        items.Clear();
        items.AddRange(itemsElemFallback.EnumerateArray());
      }
    }

    List<Book> books = [];
    foreach (var item in items)
    {
      if (!item.TryGetProperty("volumeInfo", out JsonElement volumeInfo)) continue;
      var book = new Book
      {
        Id = Generate.Generate6DigitNumber(),
        Title = volumeInfo.TryGetProperty("title", out var titleElem) ? titleElem.GetString() : null,
        Description = volumeInfo.TryGetProperty("description", out var descElem) ? descElem.GetString() : null,
        Publisher = volumeInfo.TryGetProperty("publisher", out var publisherElem) ? publisherElem.GetString() : null,
        Language = volumeInfo.TryGetProperty("language", out var langElem) ? langElem.GetString() : null,
        PageCount = volumeInfo.TryGetProperty("pageCount", out var pageCountElem) ? pageCountElem.GetInt32() : 0,
        Rating = volumeInfo.TryGetProperty("averageRating", out var ratingElem) ? (float?)ratingElem.GetDouble() : null,
        Thumbnail = volumeInfo.TryGetProperty("imageLinks", out var imageLinksElem) &&
                    imageLinksElem.TryGetProperty("thumbnail", out var thumbElem) ?
          thumbElem.GetString() :
          null,
        PublishDate = volumeInfo.TryGetProperty("publishedDate", out var dateElem) ? dateElem.GetString() : null,
        CoverImageResourceId = null,
        PreviewPdfResourceId = null,
        AudioResourceId = null,
        Version = null,
        CategoryId = 0,
        IsbNumber13 = null,
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
        Authors = new List<Author>(),
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
            case "ISBN_13": book.IsbNumber13 = idValue; break;
            case "ISBN_10": book.IsbNumber10 = idValue; break;
            default: book.OtherIdentifier = idValue; break;
          }
        }
      }

      if (volumeInfo.TryGetProperty("authors", out JsonElement authorsElem) &&
          authorsElem.ValueKind == JsonValueKind.Array)
      {
        foreach (var authorName in authorsElem.EnumerateArray()
                                              .Select(author => author.GetString())
                                              .Where(authorName => !string.IsNullOrWhiteSpace(authorName)))
        {
          book.Authors.Add(new Author { FullName = authorName! });
        }
      }

      if (volumeInfo.TryGetProperty("categories", out JsonElement categoriesElem) &&
          categoriesElem.ValueKind == JsonValueKind.Array)
      {
        var categories =
        (
          from cat in categoriesElem.EnumerateArray()
          where cat.ValueKind == JsonValueKind.String
          select cat.GetString()).ToList();
        book.Category = new Category() { Name = categories.FirstOrDefault(), };
      }

      if (volumeInfo.TryGetProperty("contentVersion", out JsonElement contentVersionElem) &&
          contentVersionElem.ValueKind == JsonValueKind.String) { book.Version = contentVersionElem.GetString(); }

      books.Add(book);
    }

    return Ok(mapper.Map<List<BookNewDto>>(books));
  }
}