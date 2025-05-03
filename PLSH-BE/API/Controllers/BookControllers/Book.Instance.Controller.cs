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
  public class UpdatePositionDto
  {
    public int? Position { get; set; }
    public int? RowShelfId { get; set; }
  }

  [HttpPut("book-instances/{id}/update-position")]
  public async Task<IActionResult> UpdatePositionAndRowShelf(int id, [FromBody] UpdatePositionDto dto)
  {
    var bookInstance = await context.BookInstances.FindAsync(id);
    if (bookInstance == null) { return NotFound(new BaseResponse<object?> { Message = "BookInstance not found.", }); }

    bookInstance.Position = dto.Position;
    bookInstance.RowShelfId = dto.RowShelfId;
    await context.SaveChangesAsync();
    return Ok(new BaseResponse<object> { Message = "BookInstance updated successfully.", });
  }

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
                       .Where(bi => bi.DeletedAt == null && bi.Book.Status != "deactive");
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

  [HttpGet("book-instances/{id}")] public async Task<IActionResult> GetBookInstanceById([FromRoute] int id)
  {
    var bookInstance = await context.BookInstances
                                    .Include(i => i.Book)
                                    .ThenInclude(b => b.CoverImageResource)
                                    .Include(b => b.RowShelf)
                                    .ThenInclude(r => r.Shelf)
                                    .FirstOrDefaultAsync(bi => bi.DeletedAt == null && bi.Id == id);
    if (bookInstance == null) { return NotFound(new BaseResponse<string> { Message = "BookInstance not found.", }); }

    var response = mapper.Map<LibraryRoomDto.BookInstanceDto>(bookInstance);
    return Ok(new BaseResponse<LibraryRoomDto.BookInstanceDto> { Data = response, });
  }

  [Authorize("LibrarianPolicy")] [HttpDelete("book-instance/{id}")]
  public async Task<IActionResult> SoftDeleteBookInstance([FromRoute] int id)
  {
    var bookInstance = await context.BookInstances.Include(b => b.Book)
            .Where(x => x.Book.Status != "deactive").FirstOrDefaultAsync(x => x.Id == id);
    if (bookInstance == null) { return NotFound(new { message = "Book instance not found", }); }

    bookInstance.DeletedAt = DateTime.UtcNow;
    bookInstance.RowShelfId = null;
    bookInstance.BookIdRestore = bookInstance.BookId;
    bookInstance.BookId = null;
    await context.SaveChangesAsync();
    return Ok(new { message = "Book instance deleted successfully" });
  }

  public class AddInstanceRequest
  {
    public int Quantity { get; set; } = 1;
  }

  [Authorize("LibrarianPolicy")] [HttpPost("{id}/instance/add")]
  public async Task<IActionResult> AddBookInstance([FromRoute] int id, [FromBody] AddInstanceRequest? request)
  {
    var book = await context.Books.FindAsync(id);
    if (book == null) { return NotFound(new BaseResponse<string?> { Message = "Không tìm thấy sách", }); }

    await bookInstanceService.AddBookInstances(id, request?.Quantity ?? 1);
    book.UpdateDate = DateTime.UtcNow;
    await context.SaveChangesAsync();
    return Ok(new BaseResponse<string?> { Message = $"Thêm thành công ${request?.Quantity ?? 1} cuốn sách", });
  }
}
