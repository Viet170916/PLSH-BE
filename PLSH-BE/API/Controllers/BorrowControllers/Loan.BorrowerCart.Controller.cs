using System.Collections.Generic;
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
  public class AddToCartRequestDto
  {
    public int BookId { get; set; }
    public int Quantity { get; set; }
  }

  [Authorize("BorrowerPolicy")] [HttpDelete("book-borrowing/{id}")]
  public async Task<IActionResult> DeleteBookBorrowing(int id)
  {
    var accountId = User.FindFirst(ClaimTypes.NameIdentifier);
    if (accountId == null)
    {
      return Unauthorized(new BaseResponse<string>
      {
        status = "fail", message = "Không xác thực: Thiếu thông tin người dùng trong token."
      });
    }

    if (!int.TryParse(accountId.Value, out var userId))
    {
      return Unauthorized(new BaseResponse<string>
      {
        status = "fail", message = "Token không hợp lệ: Không thể lấy ID người dùng."
      });
    }

    var bookBorrowing = await context.BookBorrowings
                                     .Include(bb => bb.Loan)
                                     .FirstOrDefaultAsync(bb => bb.Id == id);
    if (bookBorrowing == null)
    {
      return NotFound(new BaseResponse<string> { status = "fail", message = "Không tìm thấy thông tin mượn sách." });
    }

    if (bookBorrowing.Loan == null || bookBorrowing.Loan.BorrowerId != userId || !bookBorrowing.Loan.IsCart)
    {
      return BadRequest(new BaseResponse<string>
      {
        status = "fail", message = "Không thể xoá vì yêu cầu không hợp lệ hoặc quyền truy cập bị từ chối."
      });
    }

    context.BookBorrowings.Remove(bookBorrowing);
    await context.SaveChangesAsync();
    return Ok(new BaseResponse<string> { status = "success", message = "Xoá thông tin mượn sách thành công." });
  }

  [Authorize("BorrowerPolicy")] [HttpPost("add-to-cart")]
  public async Task<ActionResult<BaseResponse<string>>> AddToCart([FromBody] AddToCartRequestDto request)
  {
    try
    {
      if (!(User.IsInRole("student") || User.IsInRole("teacher")))
      {
        return Unauthorized(new BaseResponse<string>
        {
          message = "Chỉ sinh viên và giáo viên mới được mượn sách.", status = "unsuccessful"
        });
      }

      var accountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
      var account = await context.Accounts.FindAsync(accountId);
      if (account is not { IsVerified: true, })
      {
        return BadRequest(new BaseResponse<string>
        {
          message = "Tài khoản chưa được xác thực.", status = "unsuccessful"
        });
      }

      if (account.Status == "restricted")
      {
        return BadRequest(new BaseResponse<string>
        {
          message = "Tài khoản bị hạn chế quyền mượn sách.", status = "unsuccessful"
        });
      }

      var book = await context.Books
                              .Include(b => b.BookInstances!.Where(i => !i.IsInBorrowing))
                              .FirstOrDefaultAsync(b => b.Id == request.BookId);
      if (book == null)
      {
        return NotFound(new BaseResponse<string> { message = "Không tìm thấy sách.", status = "unsuccessful" });
      }

      var loan = await context.Loans
                              .Include(l => l.BookBorrowings)
                              .ThenInclude(bookBorrowing => bookBorrowing.BookInstance)
                              .FirstOrDefaultAsync(l => l.BorrowerId == accountId && l.IsCart);
      if (loan == null)
      {
        loan = new Loan { BorrowerId = accountId, IsCart = true };
        context.Loans.Add(loan);
        await context.SaveChangesAsync();
      }

      var quantityInCart = loan.BookBorrowings.Count(b => b.BookInstance.BookId == request.BookId);
      if (quantityInCart > 0 && book.BookInstances?.Count(i => !i.IsInBorrowing) <
          request.Quantity + quantityInCart)
      {
        return BadRequest(new BaseResponse<string>
        {
          message =
            $"Bạn đã chọn cuốn này trong danh sách chờ mượn ({quantityInCart} quyển). Hiện giờ không có đủ bản sao sách '{book.Title}' có sẵn. ",
          status = "unsuccessful"
        });
      }

      if (book.BookInstances?.Count(i => !i.IsInBorrowing) <
          request.Quantity + quantityInCart)
      {
        return BadRequest(new BaseResponse<string>
        {
          message = $"Không có đủ bản sao sách '{book.Title}' có sẵn.", status = "unsuccessful"
        });
      }

      for (var i = 0; i < request.Quantity; i++)
      {
        var availableInstance = book.BookInstances?.FirstOrDefault(i =>
          !loan.BookBorrowings.Select(b => b.BookInstance.Id).Contains(i.Id) && !i.IsInBorrowing);
        if (availableInstance == null) continue;
        loan.BookBorrowings.Add(new BookBorrowing
        {
          BookInstance = availableInstance, BorrowingStatus = "on-loan", BorrowDate = DateTime.UtcNow
        });
      }

      await context.SaveChangesAsync();
      return Ok(new BaseResponse<string> { message = "Đã thêm sách vào giỏ mượn thành công.", status = "success" });
    }
    catch (Exception ex)
    {
      return StatusCode(500,
        new BaseResponse<string>
        {
          message = $"Thêm sách vào giỏ mượn thất bại: {ex.Message}", status = "unsuccessful"
        });
    }
  }

  [Authorize("BorrowerPolicy")] [HttpGet("cart")]
  public async Task<ActionResult<BaseResponse<Loan>>> GetCart()
  {
    try
    {
      var accountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
      var account = await context.Accounts.FindAsync(accountId);
      if (account == null)
      {
        return BadRequest(new BaseResponse<string> { message = "Tài khoản không tồn tại.", status = "unsuccessful" });
      }

      var loan = await context.Loans
                              .Include(l => l.BookBorrowings)
                              .ThenInclude(bb => bb.BookInstance)
                              .ThenInclude(bb => bb.Book)
                              .ThenInclude(bb => bb.CoverImageResource)
                              .Include(l => l.BookBorrowings)
                              .ThenInclude(bb => bb.BookInstance)
                              .ThenInclude(bb => bb.RowShelf)
                              .ThenInclude(bb => bb.Shelf)
                              .Include(l => l.Borrower)
                              .ThenInclude(bb => bb.Role)
                              // .Include(l => l.Librarian)
                              .FirstOrDefaultAsync(l => l.BorrowerId == accountId && l.IsCart);
      return loan == null ?
        Ok(new BaseResponse<string> { message = "Giỏ mượn trống", status = "empty", }) :
        Ok(new BaseResponse<LoanDto>
        {
          message = "Lấy giỏ mượn thành công.", status = "success", data = mapper.Map<LoanDto>(loan),
        });
    }
    catch (Exception ex)
    {
      return StatusCode(500,
        new BaseResponse<string> { message = $"Lỗi khi lấy giỏ mượn: {ex.Message}", status = "unsuccessful" });
    }
  }

  [Authorize("BorrowerPolicy")] [Authorize("BorrowerPolicy")] [HttpPost("confirm-loan")]
  public async Task<IActionResult> ConfirmLoan([FromBody] LoanDto loanDto)
  {
    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
    if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
    {
      return Unauthorized(new BaseResponse<string>
      {
        status = "fail", message = "Không xác thực: Token không hợp lệ."
      });
    }

    var user = await loanService.GetUserByIdAsync(userId);
    if (user == null)
      return Unauthorized(new BaseResponse<string> { status = "fail", message = "Tài khoản không tồn tại." });
    var loan = await loanService.GetLoanByIdAsync(loanDto.Id);
    if (loan == null)
    {
      return NotFound(new BaseResponse<string> { status = "fail", message = "Không tìm thấy yêu cầu mượn sách." });
    }

    if (loan.BorrowerId != userId || !loan.IsCart)
    {
      return BadRequest(new BaseResponse<string>
      {
        status = "fail", message = "Bạn không có quyền xác nhận yêu cầu này hoặc yêu cầu đã được gửi trước đó."
      });
    }

    loan.IsCart = false;
    loan.AprovalStatus = "pending";
    loan.BorrowingDate = loanDto.BorrowingDate;
    foreach (var item in loan.BookBorrowings)
    {
      var updated = loanDto.BookBorrowings.FirstOrDefault(b => b.Id == item.Id);
      if (updated == null) continue;
      item.BorrowDate = updated.BorrowDate;
      if (updated.ReturnDates is { Count: > 0 }) item.ReturnDates = updated.ReturnDates;
    }

    await loanService.UpdateLoanAsync(loan);
    var librarians = await loanService.GetLibrariansAsync();
    var notifications = librarians.Select(librarian => new NotificationDto
                                  {
                                    Title = "Yêu cầu mượn sách mới",
                                    Content = $"Người dùng {user.FullName} vừa gửi yêu cầu mượn sách.",
                                    AccountId = librarian.Id,
                                    Reference = "Loan",
                                    ReferenceId = loan.Id,
                                    ReferenceData = loanDto
                                  })
                                  .ToList();
    await notificationService.SendNotificationToUsersAsync(librarians.Select(l => l.Id), notifications.First());
    var loanLink = $"{Const.LIB_CLIENT_HOST}/borrow/{loan.Id}";
    foreach (var librarian in librarians)
    {
      await emailService.SendBorrowConfirmationEmailAsync(librarian.Email, user.FullName, user.PhoneNumber, loan.Id,
        loanLink);
    }

    return Ok(new BaseResponse<LoanDto>
    {
      status = "success", message = "Gửi yêu cầu mượn sách thành công.", data = loanDto,
    });
  }
}
