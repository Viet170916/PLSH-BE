using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using System.Text.Json;
using API.Common;
using API.DTO;
using AutoMapper.QueryableExtensions;
using BU.Models.DTO;
using BU.Models.DTO.Book;
using Common.Enums;
using Common.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model.Entity.book;
using Quartz.Util;

namespace API.Controllers.BookControllers;
// [Authorize("LibrarianPolicy")]

public partial class BookController
{
  [HttpGet("{id}")] public async Task<IActionResult> GetBookById(int id)
  {
    var accountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    var book = await context.Books
                            .Where(b => b.Id == id)
                            .ProjectTo<BookNewDto>(mapper.ConfigurationProvider, new { UserId = accountId })
                            .FirstOrDefaultAsync();
    if (book == null) return NotFound();
    if (accountId != 0)
    {
      book.IsFavorite =
        await context.Favorites.FirstOrDefaultAsync(f =>
          f.BookId == book.Id && f.BorrowerId == accountId && f.Status == FavoriteStatus.Favorite) is not null;
    }

    var counts = await context.BookInstances
                              .Where(i => i.BookId == id)
                              .GroupBy(i => i.BookId)
                              .Select(g => new
                              {
                                Quantity = g.Count(), AvailableBookCount = g.Count(i => !i.IsInBorrowing)
                              })
                              .FirstOrDefaultAsync();
    if (counts != null)
    {
      book.Quantity = counts.Quantity;
      book.AvailableBookCount = counts.AvailableBookCount;
    }
    else
    {
      book.Quantity = 0;
      book.AvailableBookCount = 0;
    }

    return Ok(book);
  }

  [HttpGet] public async Task<IActionResult> SearchBooks(
    [FromQuery] string? keyword,
    [FromQuery] List<string>? categories,
    [FromQuery] int? authorId,
    [FromQuery] int? rating,
    [FromQuery] int page = 1,
    [FromQuery] int limit = 18,
    [FromQuery] string orderBy = "createdate",
    [FromQuery] bool descending = false
  )
  {
    var accountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    var trimmedKeyword = keyword?.Trim().ToLower();
    limit = Math.Clamp(limit, 1, 40);
    var query = context.Books
                       .AsNoTracking()
                       .Include(b => b.Category)
                       .Include(b => b.Reviews)
                       .Include(b => b.Authors)
                       .Where(x => x.Status != "deactive")
                       .AsQueryable();
    if (!string.IsNullOrWhiteSpace(trimmedKeyword))
    {
      query = query.Where(b =>
        (b.Title != null && b.Title.ToLower().Contains(trimmedKeyword)) ||
        b.Authors.Any(a => a.FullName.ToLower().Contains(trimmedKeyword)));

    }

    if (categories?.Any(c => !string.IsNullOrWhiteSpace(c)) == true)
    {
      var cleanedCategories = categories
                              .Where(c => !string.IsNullOrWhiteSpace(c))
                              .Select(c => c.Trim())
                              .ToList();
      query = query.Where(b => cleanedCategories.Contains(b.Category.Name));
    }

    if (rating.HasValue)
    {
      var lower = rating.Value - 0.5;
      var upper = rating.Value + 0.5;
      query = query.Where(b =>
        b.Reviews.Any() &&
        b.Reviews.Average(r => r.Rating) >= lower &&
        b.Reviews.Average(r => r.Rating) < upper);
    }

    if (authorId.HasValue)
    {
      var bookIds = await context.Books
                                 .Where(b => b.Authors.Any(a => a.Id == authorId.Value))
                                 .Select(b => b.Id)
                                 .ToListAsync();
      query = query.Where(b => bookIds.Contains(b.Id));
    }

    query = orderBy.ToLower() switch
    {
      "title" => descending ? query.OrderByDescending(b => b.Title) : query.OrderBy(b => b.Title),
      "createdate" => descending ? query.OrderByDescending(b => b.CreateDate) : query.OrderBy(b => b.CreateDate),
      _ => query.OrderByDescending(b => b.CreateDate),
    };
    var totalRecords = await query.CountAsync();
    var books = await query
                      .Skip((page - 1) * limit)
                      .Take(limit)
                      .ProjectTo<BookMinimalDto>(mapper.ConfigurationProvider)
                      .ToListAsync();
    var bookFavorites =
      await context.Favorites.Where(f => f.BorrowerId == accountId && f.Status == FavoriteStatus.Favorite)
                   .Select(f => f.BookId)
                   .ToListAsync();
    foreach (var book in books.Where(book => bookFavorites.Contains(book.Id ?? 0))) { book.IsFavorite = true; }

    var response = new BaseResponse<List<BookMinimalDto>>
    {
      PageCount = (int)Math.Ceiling((double)totalRecords / limit),
      CurrentPage = page,
      Count = totalRecords,
      Page = page,
      Data = books
    };
    return Ok(response);
  }

  [HttpGet("global/search")] public async Task<IActionResult> GetIsbn(string? isbn, string? keyword)
  {
    var trimedKeyword = keyword?.Trim().ToLower();
    if (string.IsNullOrWhiteSpace(isbn) && string.IsNullOrWhiteSpace(trimedKeyword))
    {
      return BadRequest("Bạn phải cung cấp isbn hoặc keyword để tìm kiếm.");
    }

    var query = !string.IsNullOrWhiteSpace(isbn) ? $"isbn:{isbn}" : trimedKeyword;
    var apiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY");
    var baseUrl = string.IsNullOrWhiteSpace(apiKey) ?
      $"https://www.googleapis.com/books/v1/volumes?q={Uri.EscapeDataString(query)}" :
      $"https://www.googleapis.com/books/v1/volumes?q={Uri.EscapeDataString(query)}&key={apiKey}";
    var client = httpClientFactory.CreateClient("Book");
    var urlWithLang = baseUrl + "&langRestrict=vi";
    var response = await client.GetAsync(urlWithLang);
    if (!response.IsSuccessStatusCode) { return NotFound("Lỗi khi gọi Google Books API với langRestrict=vi."); }

    var json = await response.Content.ReadAsStringAsync();
    using var doc = JsonDocument.Parse(json);
    var root = doc.RootElement;
    if (!root.TryGetProperty("items", out JsonElement itemsElem) || itemsElem.ValueKind != JsonValueKind.Array ||
        !itemsElem.EnumerateArray().Any())
    {
      var urlNoLang = baseUrl;
      response = await client.GetAsync(urlNoLang);
      if (!response.IsSuccessStatusCode) { return NotFound("Lỗi khi gọi Google Books API mà không có langRestrict."); }

      json = await response.Content.ReadAsStringAsync();
      using JsonDocument docFallback = JsonDocument.Parse(json);
      var rootFallback = docFallback.RootElement;
      if (!rootFallback.TryGetProperty("items", out JsonElement itemsElemFallback) ||
          itemsElemFallback.ValueKind != JsonValueKind.Array || !itemsElemFallback.EnumerateArray().Any())
      {
        return NotFound("Không tìm thấy sách phù hợp với yêu cầu tìm kiếm.");
      }

      itemsElem = itemsElemFallback;
    }

    List<Book> books = [];
    foreach (var item in itemsElem.EnumerateArray())
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
        Authors = new List<Author>(),
        Category = null,
        Version = volumeInfo.TryGetProperty("contentVersion", out var contentVersionElem) ?
          contentVersionElem.GetString() :
          null,
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
        book.Category = new Category { Name = categoriesElem.EnumerateArray().FirstOrDefault().GetString() };
      }

      books.Add(book);
    }

    return Ok(mapper.Map<List<BookNewDto>>(books));
  }

  [HttpPost("generate-from-gemini")]
  public async Task<ActionResult<BaseResponse<BookNewDto>>> GetBookFromGemini([FromBody] BookNewDto input)
  {
    var geminiResult = await geminiService.GetBookFromGeminiPromptAsync(input);
    if (geminiResult == null)
    {
      return BadRequest(new BaseResponse<BookNewDto>
      {
        Message = "Có lỗi trong quá trình phân tích, vui lòng thử lại!", Data = null!, Status = "error"
      });
    }

    return Ok(new BaseResponse<BookNewDto>
    {
      Message = "Lấy thông tin thành công", Data = geminiResult, Status = "success"
    });
  }
}
