using System.Collections.Generic;
using API.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers.AnalyticControllers;

public partial class AnalyticController
{
  [HttpGet("borrower/{borrowerId}")] public async Task<IActionResult> GetAnalyticsCounts(int borrowerId)
  {
    var bookBorrowingsQuery = context.BookBorrowings
                                     .AsNoTracking()
                                     .Where(bb => bb.Loan != null
                                                  && bb.Loan.BorrowerId == borrowerId
                                                  && bb.Loan.IsCart == false
                                                  && (bb.BorrowingStatus == "taken-on-loan" ||
                                                      bb.BorrowingStatus == "returned"))
                                     .Select(bb => new
                                     {
                                       bb.BorrowDate, bb.ReturnDates, bb.ExtendDates, bb.ActualReturnDate
                                     });
    var loansQuery = context.Loans
                            .AsNoTracking()
                            .Where(l => l.BorrowerId == borrowerId
                                        && l.IsCart == false
                                        && (l.AprovalStatus == "taken" || l.AprovalStatus == "return-all"));
    var favoritesQuery = context.Favorites
                                .AsNoTracking()
                                .Where(f => f.BorrowerId == borrowerId);
    var finesQuery = context.Fines
                            .AsNoTracking()
                            .Where(f => f.BorrowerId == borrowerId && f.Status != "completed");

    var borrowBookCount = await bookBorrowingsQuery.CountAsync();
    var loanCount = await loansQuery.CountAsync();
    var favoriteCount = await favoritesQuery.CountAsync();

    var overdueBook = 0;
    double overdueFee = 0;
    var bookBorrowings = await bookBorrowingsQuery.ToListAsync();
    foreach (var bb in bookBorrowings)
    {
      if (bb.ActualReturnDate == null) continue; // chưa trả sách thì bỏ qua tính overdue
      int overdueDays =
        DateTimeConverter.CalculateOverdueDays(bb.BorrowDate, bb.ReturnDates, bb.ExtendDates,
          bb.ActualReturnDate.Value);
      if (overdueDays > 0) { overdueBook++; }
    }

    var fines = await finesQuery
                      .Where(f => f.Status != "completed" && f.BookBorrowing != null)
                      .Include(f => f.BookBorrowing)
                      .ToListAsync();
    var totalOverdueDays = fines
      .Sum(f => f.BookBorrowing != null ?
        DateTimeConverter.CalculateOverdueDays(f.BookBorrowing.BorrowDate, f.BookBorrowing.ReturnDates,
          f.BookBorrowing.ExtendDates, f.BookBorrowing.ActualReturnDate.Value) :
        0);
    var totalOverdueFee = totalOverdueDays * 5;
    var result = new AnalyticsCountData
    {
      BorrowBookCount = borrowBookCount,
      LoanCount = loanCount,
      Favorite = favoriteCount,
      OverdueBook = overdueBook,
      OverdueFee = totalOverdueFee
    };
    return Ok(result);
  }

  public class AnalyticsCountData
  {
    public int BorrowBookCount { get; set; }
    public int LoanCount { get; set; }
    public int Favorite { get; set; }
    public int OverdueBook { get; set; }
    public double OverdueFee { get; set; }
  }
}
