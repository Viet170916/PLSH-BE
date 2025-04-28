using System.Collections.Generic;
using System.Security.Claims;
using BU.Models.DTO;
using BU.Models.DTO.Book;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model.Entity.Book.e_book;

namespace API.Controllers.BookControllers;

public partial class BookController
{
  [HttpGet("e-book/{bookId}/chapters")]
  public async Task<IActionResult> GetChapters(int bookId, [FromQuery] int? chapterIndex = null)
  {
    if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var accountId))
      return Unauthorized(new BaseResponse<string?> { Message = "Unauthorized", Status = "error", Data = null, });
    var allChapters = new List<EBookChapterDto.EBookChapterHtmlContentDto>();
    if (chapterIndex is not null)
    {
      allChapters =
        await epubParserService.GetChaptersAsync<EBookChapterDto.EBookChapterHtmlContentDto>(bookId, accountId, false,
          chapterIndex);
    }
    else
    {
      allChapters =
        await epubParserService.GetChaptersAsync<EBookChapterDto.EBookChapterHtmlContentDto>(bookId, accountId);
    }

    if (allChapters == null || allChapters.Count == 0)
    {
      return NotFound(new BaseResponse<string?>
      {
        Message = "Không tìm thấy chương nào cho sách này", Status = "error", Data = null,
      });
    }

    var totalChapters = await epubParserService.GetTotalChapter(bookId);
    if (!chapterIndex.HasValue)
      return Ok(new BaseResponse<IEnumerable<object>>
      {
        Message = "Lấy toàn bộ chương thành công", Status = "success", Data = allChapters, PageCount = totalChapters,
      });
    if (chapterIndex < 1 || chapterIndex > totalChapters)
    {
      return BadRequest(new BaseResponse<string?>
      {
        Message = "Chapter index không hợp lệ", Status = "error", Data = null,
      });
    }

    var chapter = allChapters.FirstOrDefault();
    return Ok(new BaseResponse<object>
    {
      Message = "Lấy chương thành công",
      Status = "success",
      Data = chapter,
      CurrentPage = chapterIndex.Value,
      PageCount = totalChapters,
    });
  }

  [HttpGet("e-book/{bookId}/chapters/text")]
  public async Task<IActionResult> GetTextChapters(int bookId, [FromQuery] int? chapterIndex = null)
  {
    if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var accountId))
      return Unauthorized(new BaseResponse<string?> { Message = "Unauthorized", Status = "error", Data = null, });
    var allChapters = new List<EBookChapterDto.EBookChapterPlainTextDto>();
    if (chapterIndex is not null)
    {
      allChapters =
        await epubParserService.GetChaptersAsync<EBookChapterDto.EBookChapterPlainTextDto>(bookId, accountId, false,
          chapterIndex);
    }
    else
    {
      allChapters =
        await epubParserService.GetChaptersAsync<EBookChapterDto.EBookChapterPlainTextDto>(bookId, accountId);
    }

    if (allChapters == null || allChapters.Count == 0)
    {
      return NotFound(new BaseResponse<string?>
      {
        Message = "Không tìm thấy chương nào cho sách này", Status = "error", Data = null,
      });
    }

    var totalChapters = await epubParserService.GetTotalChapter(bookId);
    if (!chapterIndex.HasValue)
      return Ok(new BaseResponse<IEnumerable<object>>
      {
        Message = "Lấy toàn bộ chương thành công", Status = "success", Data = allChapters, PageCount = totalChapters,
      });
    if (chapterIndex < 1 || chapterIndex > totalChapters)
    {
      return BadRequest(new BaseResponse<string?>
      {
        Message = "Chapter index không hợp lệ", Status = "error", Data = null,
      });
    }

    var chapter = allChapters.FirstOrDefault();
    return Ok(new BaseResponse<object>
    {
      Message = "Lấy chương thành công",
      Status = "success",
      Data = chapter,
      CurrentPage = chapterIndex.Value,
      PageCount = totalChapters,
    });
  }

  [HttpGet("e-book/{bookId}/chapters/summary")]
  public async Task<IActionResult> GetChaptersSummary(int bookId, [FromQuery] int page = 1, [FromQuery] int limit = 50)
  {
    if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var accountId))
      return Unauthorized(new BaseResponse<string?> { Message = "Unauthorized", Status = "error", Data = null });
    var query = context.EBookChapters
                       .Where(c => c.BookId == bookId)
                       .Select(c => new EBookChapterSummaryDto
                       {
                         Id = c.Id, ChapterIndex = c.ChapterIndex, ShortContent = c.PlainText,
                       });
    var totalCount = await query.CountAsync();
    var pageCount = (int)Math.Ceiling(totalCount / (double)limit);
    if (page < 1) page = 1;
    if (page > pageCount) page = pageCount;
    var pagedSummaries = await query
                               .OrderBy(c => c.ChapterIndex)
                               .Skip((page - 1) * limit)
                               .Take(limit)
                               .ToListAsync();
    foreach (var chapter in pagedSummaries) { chapter.ShortContent = GenerateShortContent(chapter.ShortContent); }

    return Ok(new BaseResponse<IEnumerable<EBookChapterSummaryDto>>
    {
      Message = "Lấy danh sách chương tóm tắt thành công",
      Status = "success",
      Data = pagedSummaries,
      Count = pagedSummaries.Count,
      Page = page,
      CurrentPage = page,
      Limit = limit,
      PageCount = pageCount
    });
  }

  private static string GenerateShortContent(string content)
  {
    if (string.IsNullOrWhiteSpace(content)) return string.Empty;
    var words = content.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    if (words.Length <= 100) return content.Trim();
    return string.Join(' ', words.Take(100)) + " ...";
  }

  public class EBookChapterSummaryDto
  {
    public int Id { get; set; }
    public int ChapterIndex { get; set; }
    public string ShortContent { get; set; } = string.Empty;
  }

  [Authorize("LibrarianPolicy")] [HttpDelete("e-book/chapters/{chapterId}")]
  public async Task<IActionResult> DeleteChapter(int chapterId)
  {
    if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var accountId))
      return Unauthorized(new BaseResponse<string?> { Message = "Unauthorized", Status = "error", Data = null });
    var chapterToDelete = await context.EBookChapters
                                       .FirstOrDefaultAsync(c => c.Id == chapterId);
    if (chapterToDelete == null)
    {
      return NotFound(new BaseResponse<string?> { Message = "Chapter không tồn tại", Status = "error", Data = null, });
    }

    var chaptersToUpdate = await context.EBookChapters
                                        .Where(c => c.BookId == chapterToDelete.BookId &&
                                                    c.ChapterIndex > chapterToDelete.ChapterIndex)
                                        .ToListAsync();
    context.EBookChapters.Remove(chapterToDelete);
    foreach (var chapter in chaptersToUpdate) { chapter.ChapterIndex -= 1; }

    await context.SaveChangesAsync();
    return Ok(new BaseResponse<string?> { Message = "Xoá chương thành công", Status = "success", Data = null });
  }

  [Authorize("LibrarianPolicy")] [HttpPost("e-book/{bookId}/chapters")]
  public async Task<IActionResult> AddChapter(int bookId, [FromQuery] int chapterIndex)
  {
    if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var accountId))
      return Unauthorized(new BaseResponse<string?> { Message = "Unauthorized", Status = "error", Data = null });
    if (chapterIndex < 1)
    {
      return BadRequest(
        new BaseResponse<string?> { Message = "ChapterIndex phải >= 1", Status = "error", Data = null, });
    }

    var totalChapters = await context.EBookChapters
                                     .Where(c => c.BookId == bookId)
                                     .CountAsync();
    if (chapterIndex > totalChapters + 1)
    {
      return BadRequest(new BaseResponse<string?>
      {
        Message = $"Chapter không hợp lệ, tối đa có thể chèn ở vị trí {totalChapters + 1}",
        Status = "error",
        Data = null,
      });
    }

    var chaptersToUpdate = await context.EBookChapters
                                        .Where(c => c.BookId == bookId && c.ChapterIndex >= chapterIndex)
                                        .OrderByDescending(c =>
                                          c.ChapterIndex)
                                        .ToListAsync();
    foreach (var chapter in chaptersToUpdate) { chapter.ChapterIndex += 1; }

    var newChapter =
      new EBookChapter
      {
        BookId = bookId, ChapterIndex = chapterIndex, PlainText = "", HtmlContent = "",
      };
    context.EBookChapters.Add(newChapter);
    await context.SaveChangesAsync();
    return Ok(new BaseResponse<object?>
    {
      Message = "Thêm chương thành công", Status = "success", Data = new { newChapter.Id, newChapter.ChapterIndex },
    });
  }

  [Authorize("LibrarianPolicy")] [HttpPut("e-book/chapters/{chapterId}/content")]
  public async Task<IActionResult> UpdateChapterContent(int chapterId, [FromBody] UpdateChapterContentRequest request)
  {
    if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var accountId))
      return Unauthorized(new BaseResponse<string?> { Message = "Unauthorized", Status = "error", Data = null });
    var chapter = await context.EBookChapters.FirstOrDefaultAsync(c => c.Id == chapterId);
    if (chapter == null)
    {
      return NotFound(new BaseResponse<string?> { Message = "Chapter không tồn tại", Status = "error", Data = null });
    }

    if (string.IsNullOrWhiteSpace(request.HtmlContent))
    {
      return BadRequest(new BaseResponse<string?>
      {
        Message = "TextHtml không được để trống", Status = "error", Data = null
      });
    }

    var plainText = HtmlToPlainText(request.HtmlContent);
    chapter.HtmlContent = request.HtmlContent;
    chapter.HtmlContent = plainText;
    await context.SaveChangesAsync();
    return Ok(new BaseResponse<object?>
    {
      Message = "Cập nhật nội dung thành công", Status = "success", Data = new { chapter.Id }
    });
  }

  public class UpdateChapterContentRequest
  {
    public string HtmlContent { get; set; }
  }

  private string HtmlToPlainText(string html)
  {
    if (string.IsNullOrWhiteSpace(html)) return string.Empty;
    var doc = new HtmlAgilityPack.HtmlDocument();
    doc.LoadHtml(html);
    return doc.DocumentNode.InnerText.Trim();
  }
}
