using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers.BookControllers;

public partial class BookController
{
  [HttpGet("{bookId}/book-instance")] public async Task<IActionResult> GetBookInstancesByBookId([FromRoute] int bookId)
  {
    var bookInstances = await context.BookInstances
                                     .Where(bi => bi.BookId == bookId && bi.DeletedAt == null)
                                     .ToListAsync();
    return Ok(new { bookInstances.Count, Data = bookInstances, });
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