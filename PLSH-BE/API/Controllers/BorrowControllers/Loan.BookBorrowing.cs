using System.Collections.Generic;
using AutoMapper.QueryableExtensions;
using BU.Models.DTO;
using BU.Models.DTO.Loan;
using Common.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model.Entity.Borrow;

namespace API.Controllers.BorrowControllers;

public partial class LoanController
{
  [HttpGet("book-borrowing")] public async Task<IActionResult> GetBookBorrowings(
    [FromQuery] int? bookId,
    [FromQuery] int? bookInstanceId,
    [FromQuery] int? borrowerId,
    [FromQuery] DateTime? borrowDateFrom,
    [FromQuery] DateTime? borrowDateTo,
    [FromQuery] string? status,
    [FromQuery] int page = 1,
    [FromQuery] int limit = 20
  )
  {
    if (page <= 0) page = 1;
    if (limit is <= 0 or > 100) limit = 20;
    var query = context.BookBorrowings
                       .Include(bb => bb.Loan)
                       .Include(bb => bb.BookInstance)
                       .AsQueryable();
    query = query.Where(bb => bb.Loan != null && !bb.Loan.IsCart &&
                              (bb.Loan.AprovalStatus == "taken" || bb.Loan.AprovalStatus == "return-all"));
    if (bookId.HasValue) { query = query.Where(bb => bb.BookInstance.BookId == bookId.Value); }

    if (bookInstanceId.HasValue) { query = query.Where(bb => bb.BookInstanceId == bookInstanceId.Value); }

    if (borrowerId.HasValue) { query = query.Where(bb => bb.Loan!.BorrowerId == borrowerId.Value); }

    if (borrowDateFrom.HasValue) { query = query.Where(bb => bb.BorrowDate >= borrowDateFrom.Value); }

    if (borrowDateTo.HasValue) { query = query.Where(bb => bb.BorrowDate <= borrowDateTo.Value); }

    if (!string.IsNullOrEmpty(status))
    {
      query = query.Where(bb => bb.BorrowingStatus.Equals(status, StringComparison.CurrentCultureIgnoreCase));

    }
    var totalItems = await query.CountAsync();
    var borrowings = await query
                           .OrderByDescending(bb => bb.BorrowDate)
                           .Skip((page - 1) * limit)
                           .Take(limit)
                           .ProjectTo<BookBorrowingDetailDto>(mapper.ConfigurationProvider)
                           .ToListAsync();
        await CalculateAndUpdateFines(borrowings);
        foreach (var borrowing in borrowings)
        {
            var loan = await context.Loans
                                    .Include(l => l.BookBorrowings)
                                    .FirstOrDefaultAsync(l => l.Id == borrowing.LoanId);

            if (loan != null)
            {
                var allReturned = loan.BookBorrowings.All(bb => bb.BorrowingStatus == "returned");
                if (allReturned)
                {
                    loan.AprovalStatus = "return-all";
                    context.Loans.Update(loan);
                    await context.SaveChangesAsync();
                }
            }
        }
        return Ok(new BaseResponse<List<BookBorrowingDetailDto>>()
    {
      Count = totalItems,
      Page = page,
      CurrentPage = page,
      PageCount = limit,
      Data = borrowings
    });
  }
    private async Task CalculateAndUpdateFines(List<BookBorrowingDetailDto> borrowings)
    {
        foreach (var borrowing in borrowings)
        {
            var bookBorrowing = await context.BookBorrowings
                                             .Include(bb => bb.Loan)
                                             .FirstOrDefaultAsync(bb => bb.Id == borrowing.Id);

            if (bookBorrowing == null || bookBorrowing.ActualReturnDate == null ||
                bookBorrowing.ReturnDates == null || !bookBorrowing.ReturnDates.Any())
                continue;
            var lastReturnDate = bookBorrowing.ReturnDates.Last();
            var daysLate = (bookBorrowing.ActualReturnDate.Value - lastReturnDate).Days;
            if (daysLate <= 0) continue;
            var fine = await context.Fines
                                    .Where(f => f.BookBorrowingId == borrowing.Id && f.Status != "completed")
                                    .Include(f => f.BookBorrowing)
                                    .FirstOrDefaultAsync();
            if (fine == null)
            {
                fine = new Fine
                {
                    FineDate = DateTime.UtcNow,
                    IsFined = true,
                    FineType = 0,
                    FineByDate = 5000,
                    Amount = 0,
                    Note = $"Ph� ph?t t? ??ng t?o do tr? h?n {daysLate} ng�y",
                    BookBorrowingId = borrowing.Id,
                    BorrowerId = bookBorrowing.Loan!.BorrowerId,
                    Status = "completed"
                };
                context.Fines.Add(fine);
                await context.SaveChangesAsync();
            }
            var fineAmount = fine.FineByDate * daysLate;
            fine.Amount = (fine.Amount ?? 0) + fineAmount;
            context.Fines.Update(fine);
            await context.SaveChangesAsync();
        }
    }
}
