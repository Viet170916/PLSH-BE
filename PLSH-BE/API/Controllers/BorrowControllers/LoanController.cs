using System.Collections.Generic;
using System.Security.Claims;
using API.Common;
using API.DTO;
using AutoMapper;
using BU.Hubs;
using BU.Models.DTO;
using BU.Models.DTO.Loan;
using BU.Services.Interface;
using Data.DatabaseContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Model.Entity;
using Model.Entity.Borrow;

namespace API.Controllers.BorrowControllers;

[Authorize]
[Route("api/v1/loan")]
[ApiController]
public partial class LoanController(
  AppDbContext context,
  IMapper mapper,
  GoogleCloudStorageHelper googleCloudStorageHelper,
  IHubContext<ReviewHub> hubContext,
  INotificationService notificationService,
  ILoanService loanService,
  IEmailService emailService
) : ControllerBase
{
  public class UpdateLoanStatusDto
  {
    public string Status { get; set; } = string.Empty;
  }

  [Authorize(Policy = "LibrarianPolicy")] [HttpGet]
  public async Task<IActionResult> GetLoans(
    [FromQuery] string? keyword,
    [FromQuery] string? approveStatus,
    [FromQuery] string? orderBy,
    [FromQuery] int page = 1,
    [FromQuery] int limit = 10
  )
  {
    var trimedKeyword = keyword?.ToLower().Trim();
    if (page < 1) page = 1;
    if (limit < 1) limit = 10;
    var query = context.Loans
                       // .Include(l => l.BookBorrowings)
                       // .ThenInclude(b=>b.BookInstance)
                       // .ThenInclude(b=>b.Book)
                       // .ThenInclude(b=>b.CoverImageResource)
                       .Include(l => l.BookBorrowings)
                       // .ThenInclude(l=>l.BookImagesBeforeBorrow)
                       // .Include(l => l.BookBorrowings)
                       // .ThenInclude(l=>l.BookImagesAfterBorrow)
                       .Include(l => l.Borrower)
                       .Include(l => l.Librarian)
                       .AsQueryable();
    if (!string.IsNullOrEmpty(approveStatus)) { query = query.Where(l => l.AprovalStatus == approveStatus); }

    if (!string.IsNullOrEmpty(trimedKeyword))
    {
      query = query.Where(l => l.Note.Contains(trimedKeyword)
                               || string.Concat(l.Id).Contains(trimedKeyword)
                               || string.Concat(l.BorrowerId).Contains(trimedKeyword)
                               || l.Borrower.PhoneNumber.Contains(trimedKeyword)
                               || l.Borrower.CardMemberNumber.Contains(trimedKeyword)
                               || l.Borrower.FullName.Contains(trimedKeyword));
    }

    query = orderBy?.ToLower() switch
    {
      "id" => query.OrderBy(l => l.Id),
      "borrowingDate" => query.OrderBy(l => l.BorrowingDate),
      "returnDate" => query.OrderBy(l => l.ReturnDate),
      _ => query.OrderByDescending(l => l.BorrowingDate)
    };
    var totalRecords = await query.CountAsync();
    var pageCount = (int)Math.Ceiling((double)totalRecords / limit);
    var loans = await query
                      .Skip((page - 1) * limit)
                      .Take(limit)
                      .ToListAsync();
    return Ok(new BaseResponse<List<LoanDto>>
    {
      Message = "Lấy danh sách đơn mượn thành công.",
      Status = "success",
      Data = mapper.Map<List<LoanDto>>(loans),
      Count = totalRecords,
      Page = page,
      Limit = limit,
      CurrentPage = page,
      PageCount = pageCount // Thêm số tổng số trang vào response
    });
  }

  [Authorize(Policy = "LibrarianPolicy")] [HttpGet("{id}")]
  public async Task<ActionResult<LoanDto>> GetLoan(int id)
  {
    var loan = await context.Loans
                            .Include(l => l.BookBorrowings)
                            .ThenInclude(b => b.BookInstance)
                            .ThenInclude(b => b.Book)
                            .ThenInclude(b => b.CoverImageResource)
                            .Include(l => l.BookBorrowings)
                            .ThenInclude(l => l.BookImagesBeforeBorrow)
                            .Include(l => l.BookBorrowings)
                            .ThenInclude(l => l.BookImagesAfterBorrow)
                            .Include(l => l.Borrower)
                            .ThenInclude(b => b.Role)
                            // .Include(l => l.Librarian)
                            .FirstOrDefaultAsync(l => l.Id == id);
    if (loan == null) return NotFound(new BaseResponse<string> { Message = "Loan not found", });
    return Ok(new BaseResponse<LoanDto> { Data = mapper.Map<LoanDto>(loan), });
  }

  [Authorize(Policy = "LibrarianPolicy")] [HttpPost("create")]
  public async Task<IActionResult> CreateLoan([FromBody] LoanDto loanDto)
  {
    var librarianId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
    var loan = mapper.Map<Loan>(loanDto);
    loanDto.BookBorrowings.ForEach(bb =>
    {
      var addedBorrowedBook = loan.BookBorrowings.FirstOrDefault(br => br.BookInstanceId == bb.BookInstanceId);
      if (addedBorrowedBook is null) return;
      addedBorrowedBook.BorrowingStatus = "on-loan";
      addedBorrowedBook.BookImagesBeforeBorrow = bb
                                                 .BookImagesBeforeBorrow
                                                 .Select(image => new Resource
                                                 {
                                                   Type = image.Type,
                                                   FileType = image.FileType,
                                                   Name = image.Name,
                                                   SizeByte = image.SizeByte,
                                                   LocalUrl =
                                                     $"{StaticFolder.DIRPath_BORROWING_BEFORE}/{DateTimeOffset.Now.ToUnixTimeSeconds()}-{image.Name}",
                                                 })
                                                 .ToList();
    });
    loan.LibrarianId = librarianId;
    loan.AprovalStatus = "taken";
    context.Loans.Add(loan);
    await context.SaveChangesAsync();
    loan = await context.Loans
                        .Include(l => l.BookBorrowings)
                        .ThenInclude(bb => bb.BookInstance)
                        .FirstOrDefaultAsync(l => l.Id == loan.Id);
    if (loan == null) { return NotFound("Loan not found after save."); }

    loan.BookBorrowings
        .ToList()
        .ForEach(bi => { bi.BookInstance.IsInBorrowing = true; });
    await context.SaveChangesAsync();
    var beforeResponse = mapper.Map<LoanDto>(loan);
    return Ok(new BaseResponse<LoanDto> { Data = beforeResponse });
  }
}
