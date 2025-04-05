using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers.AnalyticControllers;

public partial class AnalyticController
{
  [HttpGet("book/{bookId}")] public async Task<IActionResult> GetBookAnalytics([FromRoute] int bookId)
  {
    var book = await context.Books.Include(b => b.BookInstances)!
                            .ThenInclude(i => i.RowShelf)
                            .ThenInclude(i => i.Shelf)
                            .Include(b => b.BookInstances)
                            .ThenInclude(i => i.BookBorrowings)
                            .ThenInclude(bookBorrowing => bookBorrowing.Loan)
                            .FirstOrDefaultAsync(b => b.Id == bookId);
    if (book == null) { return NotFound(new { Message = "Không tìm thấy sách" }); }

    var bookInstances = book.BookInstances?
                            .Where(bi =>
                              bi.DeletedAt == null)
                            .ToList();
    var totalBookCount = bookInstances?.Count;
    var statusCount = bookInstances?
                      .GroupBy(bi => bi.DeletedAt.HasValue ? "damaged" : "normal")
                      .Select(group => new { Status = group.Key, Count = group.Count() })
                      .ToList();
    var borrowedCount = bookInstances?.Select(b => b.BookBorrowings.Count(b => !b.Loan.IsCart)).Sum();
    var borrowCount = bookInstances?.Count(bi => bi.IsInBorrowing);
    var damagedCount = bookInstances?.Count(bi => bi.DeletedAt.HasValue);
    var availableCount = totalBookCount - borrowCount - damagedCount;
    var report = new
    {
      BookId = book.Id,
      BookTitle = book.Title,
      TotalBooks = totalBookCount,
      AvailableBooks = availableCount,
      BorrowedBooks = borrowedCount,
      DamagedBooks = damagedCount,
      StatusBreakdown = statusCount,
      TotalBorrowCount = borrowCount,
    };
    return Ok(report);
  }
}
