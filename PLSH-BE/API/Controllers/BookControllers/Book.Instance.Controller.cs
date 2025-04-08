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
    [FromQuery] string? isbnOrBookCode = null
  )
  {
    var query = context.BookInstances
                       .Include(i => i.Book)
                       .ThenInclude(b => b.CoverImageResource)
                       .Include(b=>b.RowShelf)
                       .ThenInclude(r=>r.Shelf)
                       .Where(bi => bi.DeletedAt == null && bi.IsInBorrowing == false);
    if (!string.IsNullOrEmpty(isbnOrBookCode))
    {
      query = query.Where(bi => bi.Book.IsbNumber10.Contains(isbnOrBookCode) ||
                                bi.Book.IsbNumber13.Contains(isbnOrBookCode) ||
                                bi.Book.OtherIdentifier.Contains(isbnOrBookCode) ||
                                bi.Code.Contains(isbnOrBookCode) ||
                                isbnOrBookCode.Contains(bi.Book.IsbNumber10) ||
                                isbnOrBookCode.Contains(bi.Book.IsbNumber13) ||
                                isbnOrBookCode.Contains(bi.Book.OtherIdentifier) ||
                                isbnOrBookCode.Contains(bi.Code));
    }
    else { query = query.Where(bi => bi.BookId == bookId); }

    var bookInstances = await query.ToListAsync();
    var response = mapper.Map<List<LibraryRoomDto.BookInstanceDto>>(bookInstances);
    return Ok(new BaseResponse<List<LibraryRoomDto.BookInstanceDto>> { count = bookInstances.Count, data = response });
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
