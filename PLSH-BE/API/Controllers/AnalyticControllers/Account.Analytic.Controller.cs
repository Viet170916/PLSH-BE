using System.Collections.Generic;
using API.DTO;
using BU.Models.DTO;
using Data.DatabaseContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers.AnalyticControllers;

[Route("api/v1/analytic")]
public partial class AnalyticAccountController(AppDbContext context) : Controller
{
  [Authorize("LibrarianPolicy")] [HttpGet("account/{accountId}")]
  public async Task<IActionResult> GetAccountAnalytics(int accountId)
  {
    var loans = await context.Loans
                             .Include(l => l.BookBorrowings)
                             .ThenInclude(bb => bb.BookInstance)
                             .ThenInclude(bi => bi.Book)
                             .Where(l => l.BorrowerId == accountId)
                             .ToListAsync();
    if (!loans.Any())
    {
      return NotFound(new BaseResponse<string> { Message = "No loan records found for this account.", Data = null });
    }

    var uniqueBookIds = new HashSet<int>();
    var duplicateBookCounts = new Dictionary<int, (int Count, string Title, string Version, string Thumbnail)>();
    var returnedOnTime = 0;
    var returnedLate = 0;
    var booksPerDay = new Dictionary<DateTime, int>();
    var loansPerDay = new Dictionary<DateTime, int>();
    var returnedOnTimePerDay = new Dictionary<DateTime, int>();
    var returnedLatePerDay = new Dictionary<DateTime, int>();
    foreach (var loan in loans)
    {
      var loanDate = loan.BorrowingDate.Date;
      if (!loansPerDay.ContainsKey(loanDate)) loansPerDay[loanDate] = 0;
      loansPerDay[loanDate]++;
      foreach (var bookBorrowing in loan.BookBorrowings)
      {
        var book = bookBorrowing.BookInstance.Book;
        var bookId = book.Id;
        if (!uniqueBookIds.Add((int)bookId))
        {
          if (!duplicateBookCounts.ContainsKey((int)bookId))
          {
            duplicateBookCounts[(int)bookId] = (1, book.Title, book.Version, book.Thumbnail);
          }
          else
          {
            duplicateBookCounts[(int)bookId] =
              (duplicateBookCounts[(int)bookId].Count + 1, book.Title, book.Version, book.Thumbnail);
          }
        }

        if (!booksPerDay.ContainsKey(loanDate)) booksPerDay[loanDate] = 0;
        booksPerDay[loanDate]++;
        if (loan.ReturnDate.HasValue)
        {
          var returnDate = loan.ReturnDate.Value.Date;
          if (returnDate <= loan.BorrowingDate.AddDays(14))
          {
            returnedOnTime++;
            if (!returnedOnTimePerDay.ContainsKey(returnDate)) returnedOnTimePerDay[returnDate] = 0;
            returnedOnTimePerDay[returnDate]++;
          }
          else
          {
            returnedLate++;
            if (!returnedLatePerDay.ContainsKey(returnDate)) returnedLatePerDay[returnDate] = 0;
            returnedLatePerDay[returnDate]++;
          }
        }
      }
    }

    var response = new BaseResponse<AccountAnalyticsDto>
    {
      Message = "Success",
      Data = new AccountAnalyticsDto
      {
        TotalUniqueBooksRead = uniqueBookIds.Count,
        TotalReborrowedBooks = duplicateBookCounts.Values.Sum(v => v.Count),
        ReborrowedBooksDetails =
          duplicateBookCounts.ToDictionary(kvp => kvp.Key,
            kvp => new BookReborrowedDto
            {
              Count = kvp.Value.Count,
              Title = kvp.Value.Title,
              Version = kvp.Value.Version,
              Thumbnail = kvp.Value.Thumbnail
            }),
        TotalReturnedOnTime = returnedOnTime,
        TotalReturnedLate = returnedLate,
        BooksPerDay = booksPerDay,
        LoansPerDay = loansPerDay,
        ReturnedOnTimePerDay = returnedOnTimePerDay,
        ReturnedLatePerDay = returnedLatePerDay
      }
    };
    return Ok(response);
  }

  public class AccountAnalyticsDto
  {
    public int TotalUniqueBooksRead { get; set; }
    public int TotalReborrowedBooks { get; set; }
    public Dictionary<int, BookReborrowedDto> ReborrowedBooksDetails { get; set; }
    public int TotalReturnedOnTime { get; set; }
    public int TotalReturnedLate { get; set; }
    public Dictionary<DateTime, int> BooksPerDay { get; set; }
    public Dictionary<DateTime, int> LoansPerDay { get; set; }
    public Dictionary<DateTime, int> ReturnedOnTimePerDay { get; set; }
    public Dictionary<DateTime, int> ReturnedLatePerDay { get; set; }
  }

  public class BookReborrowedDto
  {
    public int Count { get; set; }
    public string Title { get; set; }
    public string Version { get; set; }
    public string Thumbnail { get; set; }
  }

  [Authorize("LibrarianPolicy")] [HttpGet("account/all")]
  public async Task<ActionResult<BaseResponse<MemberAnalytic>>> GetMemberAnalytics()
  {
    try
    {
      var totalMembers = await context.Accounts.CountAsync();
      var totalStudents = await context.Accounts
                                       .Include(a => a.Role)
                                       .CountAsync(a => a.Role.Name == "Student");
      var totalTeachers = await context.Accounts
                                       .Include(a => a.Role)
                                       .CountAsync(a => a.Role.Name == "Teacher");
      var totalLibrarians = await context.Accounts
                                         .Include(a => a.Role)
                                         .CountAsync(a => a.Role.Name == "Librarian");
      var verifiedAccounts = await context.Accounts.CountAsync(a => a.IsVerified);
      var unverifiedAccounts = await context.Accounts.CountAsync(a => !a.IsVerified);
      var firstDayOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
      var newThisMonth = await context.Accounts
                                      .CountAsync(a => a.CreatedAt >= firstDayOfMonth);
      var deactivatedAccounts = await context.Accounts
                                             .CountAsync(a => a.Status == "inactive" || a.Status == "deleted");
      var analytics = new MemberAnalytic
      {
        TotalMembers = totalMembers,
        TotalStudents = totalStudents,
        TotalTeachers = totalTeachers,
        TotalLibrarians = totalLibrarians,
        VerifiedAccounts = verifiedAccounts,
        UnverifiedAccounts = unverifiedAccounts,
        NewThisMonth = newThisMonth,
        DeactivatedAccounts = deactivatedAccounts
      };
      return Ok(new BaseResponse<MemberAnalytic>
      {
        Message = "Member analytics retrieved successfully", Data = analytics, Status = "success"
      });
    }
    catch (Exception ex)
    {
      return StatusCode(500,
        new BaseResponse<MemberAnalytic>
        {
          Message = $"An error occurred: {ex.Message}", Data = new MemberAnalytic(), Status = "error"
        });
    }
  }

  public class MemberAnalytic
  {
    public int TotalMembers { get; set; }
    public int TotalStudents { get; set; }
    public int TotalTeachers { get; set; }
    public int TotalLibrarians { get; set; }
    public int VerifiedAccounts { get; set; }
    public int UnverifiedAccounts { get; set; }
    public int NewThisMonth { get; set; }
    public int DeactivatedAccounts { get; set; }
  }
}
