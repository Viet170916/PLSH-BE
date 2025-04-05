using System.Collections.Generic;
using API.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers.AnalyticControllers;

public partial class AnalyticController
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
      return NotFound(new BaseResponse<string> { message = "No loan records found for this account.", data = null });
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
      message = "Success",
      data = new AccountAnalyticsDto
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
}
