using Data.DatabaseContext;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model.Entity.book;
using System.Collections.Generic;
using System.Text.Json;
using BU.Models.DTO;

namespace API.Controllers.AnalyticControllers;

[Route("api/v1/analytic")]
public partial class AnalyticBookController(AppDbContext context) : Controller
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

  [HttpGet("GetBookByDate")] public async Task<IActionResult> GetBorrowedRateByDay()
  {
    var today = DateTime.Today;
    var startDate = today.AddDays(-9);
    var bookInstances = await context.BookInstances
                                     .Include(bi => bi.BookBorrowings)
                                     .ThenInclude(bb => bb.Loan)
                                     .ToListAsync();
    var borrowingsByDay = bookInstances
                          .SelectMany(bi => bi.BookBorrowings)
                          .Where(bb =>
                            bb.Loan != null &&
                            !bb.Loan.IsCart &&
                            bb.Loan.BorrowingDate.Date >= startDate &&
                            bb.Loan.BorrowingDate.Date <= today)
                          .GroupBy(bb => bb.Loan.BorrowingDate.Date)
                          .Select(g => new { Day = g.Key, Count = g.Count() })
                          .ToDictionary(g => g.Day, g => g.Count);
    var rateData = new List<object>();
    var labels = new List<string>();
    for (int i = 0; i < 10; i++)
    {
      var date = startDate.AddDays(i);
      var count = borrowingsByDay.ContainsKey(date) ? borrowingsByDay[date] : 0;
      rateData.Add(new { day = date.ToString("dd/M"), borrowedBookCount = count });
      var thu = "T" + (((int)date.DayOfWeek + 6) % 7 + 2);
      if (thu == "T8") thu = "CN";
      labels.Add($"{thu}-{date:dd/MM/yy}");
    }

    return Ok(new { rateData, labels, highlightedItemIndex = (int?)null });
  }

  [HttpGet("book")] public async Task<IActionResult> GetBookAnalytics()
  {
    var allBooks = await context.Books
                                .Include(b => b.BookInstances)
                                .ToListAsync();
    var totalBookCount = allBooks.Count;
    var damageBookCount = allBooks
                          .SelectMany(b => b.BookInstances ?? new List<BookInstance>())
                          .Count(bi => bi.DeletedAt != null);
    var borrowCount = allBooks
                      .SelectMany(b => b.BookInstances ?? new List<BookInstance>())
                      .Count(bi => bi.IsInBorrowing);
    var normalBookCount = totalBookCount - damageBookCount - borrowCount;
    var threeDaysAgo = DateTime.UtcNow.Date.AddDays(-3);
    var newBookCount = allBooks
      .Count(b => b.CreateDate.HasValue && b.CreateDate.Value.Date >= threeDaysAgo);
    return Ok(new { normalBookCount, newBookCount, damageBookCount, totalBookCount });
  }

  [HttpGet("loan-sort-by-category")] public async Task<IActionResult> GetLoanSortByCategoryAnalytics()
  {
    var books = await context.Books
                             .Where(b => b.DeletedAt == null && b.Category != null)
                             .Include(b => b.Category)
                             .Include(b => b.BookInstances!)
                             .ThenInclude(bi => bi.BookBorrowings!)
                             .ThenInclude(bb => bb.Loan)
                             .ToListAsync();
    var genreBorrowData = books
                          .GroupBy(b => b.Category!.Name)
                          .Select(g => new
                          {
                            Genre = g.Key,
                            BorrowCount = g.SelectMany(b => b.BookInstances!)
                                           .SelectMany(bi => bi.BookBorrowings!)
                                           .Count(bb => bb.Loan != null && !bb.Loan.IsCart)
                          })
                          .OrderBy(g => g.Genre)
                          .Take(7)
                          .ToList();
    var analytics = genreBorrowData.Select(item => new
    {
      genre = item.Genre, borrowCount = item.BorrowCount, color = GetColorByBorrowCount(item.BorrowCount)
    });
    return Ok(new { analyticData = analytics });
  }

  private string GetColorByBorrowCount(int count)
  {
    if (count > 100) return "#ff4500"; // Cam đỏ đậm
    if (count >= 60) return "#ff7f50"; // Cam đỏ
    if (count >= 30) return "#ffa500"; // Cam
    if (count >= 10) return "#ffcc66"; // Cam vàng
    return "#ffe5b4"; // Cam nhạt
  }

  [HttpGet("book/all")] public async Task<ActionResult<BaseResponse<BookStatistics>>> GetBookStatistics()
  {
    try
    {
      var currentDate = DateTime.UtcNow;
      var firstDayOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
      var totalBooks = await context.BookInstances.CountAsync();
      var totalTitles = await context.Books.CountAsync();
      var totalGenres = await context.Categories.CountAsync();
      var booksAddedThisMonth = await context.Books
                                             .CountAsync(b => b.CreateDate >= firstDayOfMonth);
      var availableBooks = await context.BookInstances
                                        .CountAsync(bi => !bi.IsInBorrowing && bi.BookStatus == "normal");
      var borrowedBooks = await context.BookInstances
                                       .CountAsync(bi => bi.IsInBorrowing);
      var lostOrDamagedBooks = await context.BookInstances
                                            .CountAsync(bi => bi.BookStatus != "normal");
      var potentialOverdue = await context.BookBorrowings
                                          .Where(bb => bb.ActualReturnDate == null)
                                          .Select(bb => new { bb.Id, bb.ReturnDatesJson })
                                          .ToListAsync();
      var overdueBooks = potentialOverdue
        .Count(bb => JsonSerializer.Deserialize<List<DateTime>>(bb.ReturnDatesJson)
                                   ?
                                   .Any(rd => rd < currentDate) ?? false);
      var topBorrowedBooks = await context.Books
                                          .OrderByDescending(b => b.BookInstances.Sum(bi => bi.BookBorrowings.Count))
                                          .Take(5)
                                          .Select(b => new
                                          {
                                            title = b.Title ?? "Unknown",
                                            borrowCount = b.BookInstances.Sum(bi => bi.BookBorrowings.Count)
                                          })
                                          .ToListAsync();
      var statistics = new BookStatistics
      {
        TotalBooks = totalBooks,
        TotalTitles = totalTitles,
        TotalGenres = totalGenres,
        BooksAddedThisMonth = booksAddedThisMonth,
        AvailableBooks = availableBooks,
        BorrowedBooks = borrowedBooks,
        OverdueBooks = overdueBooks,
        LostOrDamagedBooks = lostOrDamagedBooks,
        TopBorrowedBooks = topBorrowedBooks
                           .Select(b => new TopBorrowedBook { Title = b.title, BorrowCount = b.borrowCount })
                           .ToList(),
        // ... các mapping khác
      };
      return Ok(new BaseResponse<BookStatistics>
      {
        Message = "Book statistics retrieved successfully", Data = statistics, Status = "success"
      });
    }
    catch (Exception ex)
    {
      return StatusCode(500,
        new BaseResponse<BookStatistics>
        {
          Message = $"An error occurred: {ex.Message}", Data = new BookStatistics(), Status = "error"
        });
    }
  }

  public class BookStatistics
  {
    public int TotalBooks { get; set; }
    public int TotalTitles { get; set; }
    public int TotalGenres { get; set; }
    public int BooksAddedThisMonth { get; set; }
    public int AvailableBooks { get; set; }
    public int BorrowedBooks { get; set; }
    public int OverdueBooks { get; set; }
    public int LostOrDamagedBooks { get; set; }
    public List<TopBorrowedBook> TopBorrowedBooks { get; set; } = new();
    public List<TopGenre> TopGenres { get; set; } = new();
    public List<TopAuthor> TopAuthors { get; set; } = new();
    public List<BorrowHistory> BorrowHistoryByMonth { get; set; } = new();
  }

  public class TopBorrowedBook
  {
    public string Title { get; set; } = string.Empty;
    public int BorrowCount { get; set; }
  }

  public class TopGenre
  {
    public string Genre { get; set; } = string.Empty;
    public int BorrowCount { get; set; }
  }

  public class TopAuthor
  {
    public string Author { get; set; } = string.Empty;
    public int BorrowCount { get; set; }
  }

  public class BorrowHistory
  {
    public string Month { get; set; } = string.Empty;
    public int BorrowCount { get; set; }
  }
}
