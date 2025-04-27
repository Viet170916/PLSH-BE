using System.Collections.Generic;
using AutoMapper.QueryableExtensions;
using BU.Models.DTO;
using BU.Models.DTO.Loan;
using Common.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    return Ok(new BaseResponse<List<BookBorrowingDetailDto>>()
    {
      Count = totalItems,
      Page = page,
      CurrentPage = page,
      PageCount = limit,
      Data = borrowings
    });
  }
}
