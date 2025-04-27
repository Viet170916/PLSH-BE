using System.Collections.Generic;
using System.Security.Claims;
using BU.Models.DTO;
using BU.Models.DTO.Book;
using Microsoft.AspNetCore.Mvc;

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
      allChapters = await epubParserService.GetChaptersAsync<EBookChapterDto.EBookChapterHtmlContentDto>(bookId, accountId, false, chapterIndex);
    }
    else { allChapters = await epubParserService.GetChaptersAsync<EBookChapterDto.EBookChapterHtmlContentDto>(bookId, accountId); }

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
}
