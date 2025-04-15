using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using API.Common;
using API.DTO;
using BU.Models.DTO;
using BU.Models.DTO.Loan;
using BU.Models.DTO.Notification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Model.Entity.Borrow;
using Model.Entity.Notification;

namespace API.Controllers.BorrowControllers;

public partial class LoanController
{
  [Authorize(Policy = "BorrowerPolicy")] [HttpPut("{id}/status")]
  public async Task<IActionResult> UpdatePersonalLoanStatus(int id, [FromBody] UpdateLoanStatusDto request)
  {
    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
    if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
    {
      return Unauthorized(new BaseResponse<string>
      {
        status = "fail", message = "Không xác thực: Token không hợp lệ."
      });
    }

    var user = await context.Accounts.FindAsync(userId);
    var validStatuses = new[] { "cancel", };
    if (!validStatuses.Contains(request.Status.ToLower()))
    {
      return BadRequest(new BaseResponse<object?>
      {
        message = "Trạng thái không hợp lệ. Chỉ chấp nhận: pending, approved, rejected.",
        status = "error",
        data = null,
      });
    }

    var loan = await context.Loans.FirstOrDefaultAsync(l => l.Id == id && l.BorrowerId == userId);
    if (loan == null)
    {
      return NotFound(new BaseResponse<object?>
      {
        message = "Không tìm thấy đơn mượn.", status = "error", data = null
      });
    }

    loan.AprovalStatus = request.Status;
    await context.SaveChangesAsync();
    var librarians = await context.Accounts
                                  .Where(a => a.Role.Name == "librarian")
                                  .ToListAsync();
    var notifications = librarians.Select(lib => new Notification
                                  {
                                    Title = "Người mượn cập nhật trạng thái đơn mượn",
                                    Content =
                                      $"Người dùng {user?.FullName} đã cập nhật trạng thái đơn mượn là  {loan.AprovalStatus}.",
                                    Date = DateTime.UtcNow,
                                    Reference = "Loan",
                                    ReferenceId = loan.Id,
                                    AccountId = lib.Id,
                                    IsRead = false,
                                  })
                                  .ToList();
    context.Notifications.AddRange(notifications);
    await context.SaveChangesAsync();
    var notificationDtos = notifications.Select(n =>
                                          mapper.Map<NotificationDto>(n,
                                            opt => opt.Items["ReferenceData"] = mapper.Map<LoanDto>(loan)))
                                        .ToList();
    foreach (var librarian in librarians)
    {
      var userNotification = notificationDtos.FirstOrDefault(n => n.AccountId == librarian.Id);
      await hubContext.Clients.User(librarian.Id.ToString())
                      .SendAsync("ReceiveNotification", userNotification);
    }

    var loanLink = $"{Const.LIB_CLIENT_HOST}/borrow/{loan.Id}";
    foreach (var librarian in librarians)
    {
      _ = emailService.SendStatusUpdateEmailAsync(librarian.Email, user?.FullName, request.Status, user?.FullName,
        user?.PhoneNumber, loan.Id, loanLink);
    }

    return Ok(new BaseResponse<LoanDto>
    {
      message = "Cập nhật trạng thái thành công.", status = "success", data = mapper.Map<Loan, LoanDto>(loan),
    });
  }

  [Authorize(Policy = "BorrowerPolicy")] [HttpGet("personal/{id}")]
  public async Task<ActionResult<LoanDto>> GetPersonalLoan([FromRoute] int id)
  {
    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
    if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
    {
      return Unauthorized(new BaseResponse<string>
      {
        status = "fail", message = "Không xác thực: Token không hợp lệ."
      });
    }

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
                            .Include(l => l.Librarian)
                            .FirstOrDefaultAsync(l => l.Id == id && l.BorrowerId == userId);
    if (loan == null) return NotFound(new BaseResponse<string> { message = "Không tìm thấy lượt mượn", });
    return Ok(new BaseResponse<LoanDto> { data = mapper.Map<LoanDto>(loan), });
  }

  [Authorize(Policy = "BorrowerPolicy")] [HttpGet("personal")]
  public async Task<IActionResult> GetPersonalLoans(
    [FromQuery] string? keyword,
    [FromQuery] string? approveStatus,
    [FromQuery] string? orderBy,
    [FromQuery] int page = 1,
    [FromQuery] int limit = 10
  )
  {
    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
    if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
    {
      return Unauthorized(new BaseResponse<string>
      {
        status = "fail", message = "Không xác thực: Token không hợp lệ."
      });
    }

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
                       .Where(l => l.BorrowerId == userId)
                       .AsQueryable();
    if (!string.IsNullOrEmpty(approveStatus)) { query = query.Where(l => l.AprovalStatus == approveStatus); }

    if (!string.IsNullOrEmpty(keyword))
    {
      query = query.Where(l => l.Note.Contains(keyword)
                               || string.Concat(l.Id).Contains(keyword)
                               || l.Borrower.PhoneNumber.Contains(keyword)
                               || l.Borrower.CardMemberNumber.Contains(keyword)
                               || l.Borrower.FullName.Contains(keyword));
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
      message = "Lấy danh sách đơn mượn thành công.",
      status = "success",
      data = mapper.Map<List<LoanDto>>(loans),
      count = totalRecords,
      page = page,
      limit = limit,
      currenPage = page,
      pageCount = pageCount // Thêm số tổng số trang vào response
    });
  }

  [Authorize(Policy = "BorrowerPolicy")] [HttpPost("borrowed-book/{bookBorrowingId}/extend-request")]
  public async Task<IActionResult> RequestBookExtension(
    [FromRoute] int bookBorrowingId,
    [FromBody] ExtendRequestDto request
  )
  {
    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
    if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
    {
      return Unauthorized(new BaseResponse<string>
      {
        message = "Không xác thực: Token không hợp lệ.", status = "fail", data = null
      });
    }

    var bookBorrowing = await context.BookBorrowings
                                     .Include(bb => bb.Loan)
                                     .FirstOrDefaultAsync(bb =>
                                       bb.Id == bookBorrowingId && bb.Loan != null && bb.Loan.BorrowerId == userId);
    if (bookBorrowing == null)
    {
      return NotFound(new BaseResponse<string>
      {
        message = "Không tìm thấy thông tin mượn sách.", status = "error", data = null
      });
    }

    if (bookBorrowing.BorrowingStatus != "on-loan")
    {
      return BadRequest(new BaseResponse<string>
      {
        message = "Không thể yêu cầu gia hạn. Cuốn sách đã được trả.", status = "error", data = null
      });
    }

    var librarians = await context.Accounts
                                  .Where(a => a.Role.Name == "librarian")
                                  .ToListAsync();
    var notifications = librarians.Select(lib => new Notification
                                  {
                                    Title = "Yêu cầu gia hạn mượn sách",
                                    Content =
                                      $"Người mượn {bookBorrowing.Loan.Borrower?.FullName} đã gửi yêu cầu gia hạn mượn sách (ID: {bookBorrowingId}).",
                                    Date = DateTime.UtcNow,
                                    Reference = "BookBorrowing",
                                    ReferenceId = bookBorrowing.Id,
                                    AccountId = lib.Id,
                                    IsRead = false
                                  })
                                  .ToList();
    context.Notifications.AddRange(notifications);
    await context.SaveChangesAsync();
    var notiDtos = notifications.Select(n =>
                                  mapper.Map<NotificationDto>(n,
                                    opt => opt.Items["ReferenceData"] = mapper.Map<BookBorrowingDto>(bookBorrowing)))
                                .ToList();
    foreach (var lib in librarians)
    {
      var userNotification = notiDtos.FirstOrDefault(n => n.AccountId == lib.Id);
      await hubContext.Clients.User(lib.Id.ToString())
                      .SendAsync("ReceiveNotification", userNotification);
    }

    return Ok(new BaseResponse<string>
    {
      message = "Đã gửi yêu cầu gia hạn tới thư viện.", status = "success", data = null
    });
  }
}
