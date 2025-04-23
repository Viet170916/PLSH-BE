using Data.DatabaseContext;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model.Entity.book;
using System.Collections.Generic;

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
    [HttpGet("GetBookByDate")]
    public async Task<IActionResult> GetBorrowedRateByDay()
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
            .Select(g => new {
                Day = g.Key,
                Count = g.Count()
            })
            .ToDictionary(g => g.Day, g => g.Count);

        var rateData = new List<object>();
        var labels = new List<string>();
        for (int i = 0; i < 10; i++)
        {
            var date = startDate.AddDays(i);
            var count = borrowingsByDay.ContainsKey(date) ? borrowingsByDay[date] : 0;
            rateData.Add(new
            {
                day = date.ToString("dd/M"),
                borrowedBookCount = count
            });
            var thu = "T" + (((int)date.DayOfWeek + 6) % 7 + 2);
            if (thu == "T8") thu = "CN";
            labels.Add($"{thu}-{date:dd/MM/yy}");
        }
        return Ok(new
        {
            rateData,
            labels,
            highlightedItemIndex = (int?)null
        });
    }

    [HttpGet("book")]
    public async Task<IActionResult> GetBookAnalytics()
    {
        var book = await context.Books
            .Include(b => b.BookInstances)!
                .ThenInclude(i => i.RowShelf)
                .ThenInclude(i => i.Shelf)
            .Include(b => b.BookInstances)
                .ThenInclude(i => i.BookBorrowings)
                .ThenInclude(bookBorrowing => bookBorrowing.Loan)
            .FirstOrDefaultAsync();
        if (book == null)
        {
            return NotFound(new { Message = "Không tìm thấy sách" });
        }
        var bookInstances = book.BookInstances?
            .ToList();
        var totalBookCount = bookInstances?.Count ?? 0;
        var damageBookCount = bookInstances?.Count(bi => bi.DeletedAt.HasValue) ?? 0;
        var borrowCount = bookInstances?.Count(bi => bi.IsInBorrowing) ?? 0;
        var normalBookCount = totalBookCount - damageBookCount - borrowCount;
        // ✅ Sách mới thêm trong vòng 3 ngày trở lại đây
        var threeDaysAgo = DateTime.UtcNow.Date.AddDays(-3);
        var newBookCount = bookInstances.Count(bi => bi.CreatedAt.Date >= threeDaysAgo);
        var report = new
        {
            bookQuantityAnalyticsData = new
            {
                normalBookCount = normalBookCount,
                newBookCount = newBookCount,
                damageBookCount = damageBookCount,
                totalBookCount = totalBookCount
            }
        };
        return Ok(report);
    }
    [HttpGet("loansortbycategory")]
    public async Task<IActionResult> GetLoanSortByCategoryAnalytics()
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
            genre = item.Genre,
            borrowCount = item.BorrowCount,
            color = GetColorByBorrowCount(item.BorrowCount)
        });

        return Ok(new
        {
            analyticData = analytics
        });
    }

    private string GetColorByBorrowCount(int count)
    {
        if (count > 100) return "#ff4500";   // Cam đỏ đậm
        if (count >= 60) return "#ff7f50";   // Cam đỏ
        if (count >= 30) return "#ffa500";   // Cam
        if (count >= 10) return "#ffcc66";   // Cam vàng
        return "#ffe5b4";                    // Cam nhạt
    }



}
