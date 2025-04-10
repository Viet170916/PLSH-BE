using System.Collections.Generic;
using System.Security.Claims;
using API.Common;
using API.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model.Entity;
using System.Text.Json;
using BU.Models.DTO;
using BU.Models.DTO.Loan;
using BU.Models.DTO.Notification;
using EFCore.BulkExtensions;
using Microsoft.AspNetCore.SignalR;
using Model.Entity.Borrow;
using Model.Entity.Notification;
using static System.Int32;

namespace API.Controllers.BorrowControllers;

public partial class LoanController
{
  [Authorize(Policy = "LibrarianPolicy")] [HttpPut("borrowed-book/{bookBorrowingId}/return-confirm")]
  public async Task<IActionResult> UpdateReturnInfo(
    [FromRoute] int bookBorrowingId,
    [FromForm] ReturnInfoRequestDto request
  )
  {
    var tryParse = TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "", out var accountId);
    var librarian = await context.Accounts.FindAsync(accountId);
    var bookBorrowing = await context.BookBorrowings
                                     .Include(bb => bb.BookInstance)
                                     .ThenInclude(bookInstance => bookInstance.Book)
                                     .Include(bb => bb.BookImagesAfterBorrow)
                                     .Include(bookBorrowing => bookBorrowing.Loan)
                                     .ThenInclude(loan => loan.Borrower)
                                     .FirstOrDefaultAsync(bb => bb.Id == bookBorrowingId);
    if (bookBorrowing == null)
    {
      return NotFound(new BaseResponse<string>
      {
        message = "Không tìm thấy thông tin mượn sách.", data = null, status = "error"
      });
    }

    if (bookBorrowing.BorrowingStatus == "returned")
    {
      return BadRequest(new BaseResponse<string> { message = "Cuốn này đã được trả", data = null, status = "error" });
    }

    bookBorrowing.NoteAfterBorrow = request.NoteAfterBorrow;
    var uploadedResources = new List<Resource>();
    if (request.Images is { Count: > 0, })
    {
      foreach (var image in request.Images)
      {
        if (image.Length <= 0) continue;
        var filePath = $"{StaticFolder.DIRPath_BORROWING_AFTER}/{bookBorrowingId}/{Guid.NewGuid()}_{image.FileName}";
        await googleCloudStorageHelper.UploadFileAsync(image.OpenReadStream(), filePath, image.ContentType);
        var resource = new Resource
        {
          Type = "image",
          Name = image.FileName,
          SizeByte = image.Length,
          FileType = image.ContentType,
          LocalUrl = filePath
        };
        uploadedResources.Add(resource);
      }

      bookBorrowing.BookImagesAfterBorrow = uploadedResources;
    }

    bookBorrowing.BorrowingStatus = "returned";
    bookBorrowing.BookInstance.IsInBorrowing = false;
    bookBorrowing.ActualReturnDate = DateTime.UtcNow;
    await context.SaveChangesAsync();
    var link = $"{Const.CLIENT_HOST}/borrow/{bookBorrowing.Loan?.Id}";
    _ = emailService.SendReturnConfirmationEmailAsync(bookBorrowing.Loan?.Borrower?.Email,
      bookBorrowing.Loan?.Borrower?.FullName,
      bookBorrowing.BookInstance.Book.Title,
      bookBorrowing.Loan?.Id ?? 0,
      bookBorrowing.ActualReturnDate.ToString(),
      librarian?.FullName,
      bookBorrowing.Loan?.Borrower?.PhoneNumber,
      link);
    return Ok(new BaseResponse<List<Resource>>
    {
      message = "Cập nhật thông tin trả sách thành công.", data = uploadedResources, status = "success"
    });
  }

  public class ReturnInfoRequestDto
  {
    public string? NoteAfterBorrow { get; set; }
    public List<IFormFile>? Images { get; set; }
  }

  [Authorize(Policy = "LibrarianPolicy")] [HttpPut("{id}/approve-status")]
  public async Task<IActionResult> UpdateLoanStatus(int id, [FromBody] UpdateLoanStatusDto request)
  {
    var tryParse = TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "", out var accountId);
    var librarian = await context.Accounts.FindAsync(accountId);
    var validStatuses = new[] { "pending", "approved", "rejected", "cancel", "taken", "return-all", };
    if (!validStatuses.Contains(request.Status.ToLower()))
    {
      return BadRequest(new BaseResponse<object?>
      {
        message =
          "Trạng thái không hợp lệ. Chỉ chấp nhận: \"pending\", \"approved\", \"rejected\", \"cancel\", \"taken\", \"return-all\"",
        status = "error",
        data = null
      });
    }

    var loan = await context.Loans
                            .Include(l => l.BookBorrowings)
                            .ThenInclude(b => b.BookInstance)
                            .Include(loan => loan.Borrower)
                            .FirstOrDefaultAsync(l => l.Id == id);
    if (loan == null)
    {
      return NotFound(new BaseResponse<object?>
      {
        message = "Không tìm thấy đơn mượn.", status = "error", data = null
      });
    }

    loan.AprovalStatus = request.Status;
    switch (request.Status)
    {
      case "taken":
        if (loan.BookBorrowings?.Count > 0)
        {
          foreach (var loanBookBorrowing in loan.BookBorrowings)
          {
            loanBookBorrowing.BorrowingStatus = "on-loan";
            var now = DateTime.Now;
            if (loanBookBorrowing.ReturnDates.Count != 0)
            {
              loanBookBorrowing.ReturnDates = loanBookBorrowing.ReturnDates
                                                               .Select(date =>
                                                                 now + ((date - loanBookBorrowing.BorrowDate)))
                                                               .ToList();
            }

            loanBookBorrowing.BorrowDate = now;
            loanBookBorrowing.BookInstance.IsInBorrowing = true;
          }

          await context.BulkUpdateAsync(loan.BookBorrowings);
        }

        break;
      case "return-all":
        if (loan.BookBorrowings?.Count > 0)
        {
          foreach (var loanBookBorrowing in loan.BookBorrowings)
          {
            loanBookBorrowing.BorrowingStatus = "returned";
            loanBookBorrowing.ActualReturnDate = DateTime.UtcNow;
            loanBookBorrowing.BookInstance.IsInBorrowing = false;
          }

          await context.BulkUpdateAsync(loan.BookBorrowings);
        }

        break;
    }

    await context.SaveChangesAsync();
    var loanDto = mapper.Map<LoanDto>(loan);
    var notificationDto = new NotificationDto
    {
      Title = "Trạng thái đơn mượn đã cập nhật",
      Content = $"Trạng thái đơn mượn #{loan.Id} đã được cập nhật thành \"{request.Status}\".",
      Reference = "Loan",
      ReferenceId = loan.Id,
      ReferenceData = loanDto,
    };
    await notificationService.SendNotificationToUserAsync(loan.Borrower.Id, notificationDto);
    var link = $"{Const.CLIENT_HOST}/borrow/{loan.Id}";
    _ = emailService.SendStatusUpdateEmailAsync(loan.Borrower.Email, librarian.FullName, loan.AprovalStatus,
      loan.Borrower.FullName, loan.Borrower.PhoneNumber, loan.Id, link);
    return Ok(new BaseResponse<LoanDto>
    {
      message = "Cập nhật trạng thái thành công.", status = "success", data = mapper.Map<Loan, LoanDto>(loan),
    });
  }

  [Authorize(Policy = "LibrarianPolicy")] [HttpPut("borrowed-book/{bookBorrowingId}/extend")]
  public async Task<IActionResult> ExtendBookBorrowing(
    [FromRoute] int bookBorrowingId,
    [FromBody] ExtendRequestDto request
  )
  {
    if (request.ReturnDate == null)
    {
      return BadRequest(new BaseResponse<string>
      {
        message = "ReturnDate là bắt buộc.", data = null, status = "error"
      });
    }

    var bookBorrowing =
      await context.BookBorrowings
                   .Include(b => b.Loan)
                   .FirstOrDefaultAsync(b => b.Id == bookBorrowingId);
    if (bookBorrowing == null)
    {
      return NotFound(new BaseResponse<string>
      {
        message = "Không tìm thấy thông tin mượn sách.", data = null, status = "error"
      });
    }

    if (bookBorrowing.BorrowingStatus != "on-loan")
    {
      return BadRequest(new BaseResponse<string>
      {
        message = "Không thể gia hạn. Cuốn sách đã được trả.", data = null, status = "error"
      });
    }

    var extendDates = bookBorrowing.ExtendDates;
    extendDates.Add(request.ExtendDate ?? DateTime.Now);
    bookBorrowing.ExtendDatesJson = JsonSerializer.Serialize(extendDates);
    //..
    var returnDates = bookBorrowing.ReturnDates;
    returnDates.Add(request.ReturnDate);
    bookBorrowing.ReturnDatesJson = JsonSerializer.Serialize(returnDates);
    await context.SaveChangesAsync();
    var resultDto = mapper.Map<BookBorrowingDto>(bookBorrowing);
    var borrower = await context.Accounts.FindAsync(bookBorrowing.Loan?.BorrowerId);
    if (borrower == null)
      return Ok(new BaseResponse<BookBorrowingDto>
      {
        message = "Gia hạn thành công.", data = resultDto, status = "success"
      });
    var notification = new Notification
    {
      Title = "Gia hạn mượn sách thành công",
      Content = $"Yêu cầu gia hạn sách (ID: {bookBorrowing.Id}) đã được chấp thuận.",
      Date = DateTime.UtcNow,
      Reference = "BookBorrowing",
      ReferenceId = bookBorrowing.Id,
      AccountId = borrower.Id,
      IsRead = false,
    };
    context.Notifications.Add(notification);
    await context.SaveChangesAsync();
    var notificationDto = mapper.Map<NotificationDto>(notification,
      opt => opt.Items["ReferenceData"] = resultDto);
    await hubContext.Clients.User(borrower.Id.ToString())
                    .SendAsync("ReceiveNotification", notificationDto);
    var link = $"{Const.CLIENT_HOST}/borrow/{bookBorrowing.Loan?.Id}";
    _ = emailService.SendExtendConfirmationEmailAsync(borrower.Email,
      borrower.FullName,
      request.ReturnDate,
      bookBorrowing.Loan?.Id ?? 0,
      link);
    return Ok(new BaseResponse<BookBorrowingDto>
    {
      message = "Gia hạn thành công.", data = resultDto, status = "success"
    });
  }

  public class ExtendRequestDto
  {
    public DateTime? ExtendDate { get; set; }

    [Required]
    public DateTime ReturnDate { get; set; }
  }

  [Authorize(Policy = "LibrarianPolicy")] [HttpPut("update/{id}")]
  public async Task<IActionResult> UpdateLoan(int id, [FromBody] LoanDto loanDto)
  {
    if (id != loanDto.Id) return BadRequest();
    var existingLoan = await context.Loans.Include(l => l.BookBorrowings).FirstOrDefaultAsync(l => l.Id == id);
    if (existingLoan == null) return NotFound();
    mapper.Map(loanDto, existingLoan);
    await context.SaveChangesAsync();
    return NoContent();
  }

  [HttpDelete("delete/{id}")] public async Task<IActionResult> DeleteLoan(int id)
  {
    var loan = await context.Loans.FindAsync(id);
    if (loan == null) return NotFound();
    context.Loans.Remove(loan);
    await context.SaveChangesAsync();
    return NoContent();
  }
}
