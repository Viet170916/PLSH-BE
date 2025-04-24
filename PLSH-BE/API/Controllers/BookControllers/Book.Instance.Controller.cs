using System.Collections.Generic;
using API.DTO;
using BU.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model.Entity.book.Dto;

namespace API.Controllers.BookControllers;
// [Authorize("LibrarianPolicy")]

public partial class BookController
{
  [HttpGet("book-instances")] public async Task<IActionResult> GetBookInstancesByBookIdOrIsbn(
    [FromQuery] int? bookId,
    [FromQuery] string? isbnOrBookCode = null,
    [FromQuery] int page = 1,
    [FromQuery] int limit = 10
  )
  {
    var query = context.BookInstances
                       .Include(i => i.Book)
                       .ThenInclude(b => b.CoverImageResource)
                       .Include(b => b.RowShelf)
                       .ThenInclude(r => r.Shelf)
                       .Where(bi => bi.DeletedAt == null && bi.IsInBorrowing == false);
    if (!string.IsNullOrEmpty(isbnOrBookCode))
    {
      query = query.Where(bi =>
        (bi.Book.IsbNumber10 ?? "").Contains(isbnOrBookCode) ||
        (bi.Book.IsbNumber13 ?? "").Contains(isbnOrBookCode) ||
        (bi.Book.OtherIdentifier ?? "").Contains(isbnOrBookCode) ||
        (bi.Code ?? "").Contains(isbnOrBookCode) ||
        (bi.Book.Title ?? "").Contains(isbnOrBookCode));
    }
    else
      if (bookId is not null) { query = query.Where(bi => bi.BookId == bookId); }

    var totalCount = await query.CountAsync();
    var pageCount = (int)Math.Ceiling((double)totalCount / limit);
    var bookInstances = await query
                              .OrderByDescending(bi => bi.CreatedAt)
                              .Skip((page - 1) * limit)
                              .Take(limit)
                              .ToListAsync();
    var response = mapper.Map<List<LibraryRoomDto.BookInstanceDto>>(bookInstances);
    return Ok(new BaseResponse<List<LibraryRoomDto.BookInstanceDto>>
    {
      Count = totalCount, Page = page, PageCount = pageCount, Data = response
    });
  }

  [HttpDelete("book-instance/{id}")] public async Task<IActionResult> SoftDeleteBookInstance([FromRoute] int id)
  {
    var bookInstance = await context.BookInstances.FindAsync(id);
    if (bookInstance == null) { return NotFound(new { message = "Book instance not found", }); }

    bookInstance.DeletedAt = DateTime.UtcNow;
    bookInstance.RowShelfId = null;
    bookInstance.BookIdRestore = bookInstance.BookId;
    bookInstance.BookId = null;
    await context.SaveChangesAsync();
    return Ok(new { message = "Book instance deleted successfully" });
  }
}
