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
using Common.Enums;
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
        Message = "Không tìm thấy thông tin mượn sách.", Data = null, Status = "error"
      });
    }

    if (bookBorrowing.BorrowingStatus == "returned")
    {
      return BadRequest(new BaseResponse<string> { Message = "Cuốn này đã được trả", Data = null, Status = "error" });
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
      Message = "Cập nhật thông tin trả sách thành công.", Data = uploadedResources, Status = "success"
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
        Message =
          "Trạng thái không hợp lệ. Chỉ chấp nhận: \"pending\", \"approved\", \"rejected\", \"cancel\", \"taken\", \"return-all\"",
        Status = "error",
        Data = null
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
        Message = "Không tìm thấy đơn mượn.", Status = "error", Data = null
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
      Message = "Cập nhật trạng thái thành công.", Status = "success", Data = mapper.Map<Loan, LoanDto>(loan),
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
        Message = "ReturnDate là bắt buộc.", Data = null, Status = "error"
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
        Message = "Không tìm thấy thông tin mượn sách.", Data = null, Status = "error"
      });
    }

    if (bookBorrowing.BorrowingStatus != "on-loan")
    {
      return BadRequest(new BaseResponse<string>
      {
        Message = "Không thể gia hạn. Cuốn sách đã được trả.", Data = null, Status = "error"
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
        Message = "Gia hạn thành công.", Data = resultDto, Status = "success"
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
      Message = "Gia hạn thành công.", Data = resultDto, Status = "success"
    });
  }

  public class ExtendRequestDto
  {
    public DateTime? ExtendDate { get; set; }

    [Required]
    public DateTime ReturnDate { get; set; }
  }

  [Authorize(Policy = "LibrarianPolicy")] [HttpDelete("lib/book-borrowing/{id}/delete")]
  public async Task<IActionResult> LibDeleteBookBorrowing(int id)
  {
    var accountIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
    if (accountIdClaim == null || !int.TryParse(accountIdClaim.Value, out var userId))
    {
      return Unauthorized(new BaseResponse<string>
      {
        Status = "fail", Message = "Không xác thực hoặc token không hợp lệ."
      });
    }

    var bookBorrowing = await context.BookBorrowings
                                     .Include(bb => bb.Loan)
                                     .FirstOrDefaultAsync(bb => bb.Id == id);
    if (bookBorrowing == null)
    {
      return NotFound(new BaseResponse<string> { Status = "fail", Message = "Không tìm thấy thông tin mượn sách." });
    }

    var isLibrarian = User.IsInRole("librarian");
    // var isBorrower = bookBorrowing.Loan?.BorrowerId == userId;
    // if (!isBorrower && !isLibrarian) { return Forbid(); }
    if (bookBorrowing.Loan == null || bookBorrowing.Loan.IsCart)
    {
      return BadRequest(new BaseResponse<string>
      {
        Status = "fail", Message = "Thủ thư không thể xoá vì thông tin mượn sách đang nằm trong giỏ người dùng."
      });
    }

    if (bookBorrowing.Loan.AprovalStatus != "pending" && bookBorrowing.Loan.AprovalStatus != "approved")
    {
      return BadRequest(new BaseResponse<string>
      {
        Status = "fail",
        Message = "Chỉ có thể xoá khi trạng thái là \"Đang chờ duyệt\" hoặc \"Đã được duyệt nhưng chưa lấy sách\"."
      });
    }

    var borrowerId = bookBorrowing.Loan.BorrowerId;
    context.BookBorrowings.Remove(bookBorrowing);
    await context.SaveChangesAsync();
    if (isLibrarian && borrowerId != null)
    {
      await notificationService.SendNotificationToUserAsync(borrowerId, new NotificationDto
      {
        AccountId = borrowerId,
        Title = "Sách đã bị xoá khỏi lượt mượn",
        Content = $"Một cuốn sách trong lượt mượn của bạn đã bị huỷ bởi thủ thư.",
        ReferenceId = bookBorrowing.LoanId,
        Reference = "Loan",
        Date = DateTime.UtcNow,
      });
    }

    return Ok(new BaseResponse<string> { Status = "success", Message = "Xoá thông tin mượn sách thành công." });
  }

  public class AddBookBorrowingRequest
  {
    public int LoanId { get; set; }
    public int BookId { get; set; }
  }

  [Authorize(Policy = "LibrarianPolicy")] [HttpPost("book-borrowing/add")]
  public async Task<IActionResult> AddBookBorrowing([FromBody] AddBookBorrowingRequest request)
  {
    var loan = await context.Loans
                            .Include(l => l.BookBorrowings)
                            .FirstOrDefaultAsync(l => l.Id == request.LoanId);
    if (loan == null)
    {
      return NotFound(new BaseResponse<string> { Status = "fail", Message = "Không tìm thấy lượt mượn." });
    }

    if (loan.AprovalStatus != "pending" && loan.AprovalStatus != "approved")
    {
      return BadRequest(new BaseResponse<string>
      {
        Status = "fail",
        Message =
          "Chỉ có thể thêm sách vào lượt mượn có trạng thái \"Đang chờ duyệt\" hoặc \"Đã được duyệt nhưng chưa lấy sách\"."
      });
    }

    if (loan.BookBorrowings.Any(bb => bb.BookInstanceId == request.BookId))
    {
      return BadRequest(new BaseResponse<string> { Status = "fail", Message = "Sách đã tồn tại trong lượt mượn." });
    }

    var bookBorrowing = new BookBorrowing
    {
      LoanId = loan.Id, BookInstanceId = request.BookId, BorrowingStatus = "on-loan", CreatedAt = DateTime.UtcNow,
    };
    context.BookBorrowings.Add(bookBorrowing);
    await context.SaveChangesAsync();
    return Ok(new BaseResponse<string> { Status = "success", Message = "Thêm sách vào lượt mượn thành công." });
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

  [Authorize(Policy = "LibrarianPolicy")] [HttpPut("book-borrowing/update/single")]
  public async Task<IActionResult> UpdateBorrowingDate([FromForm] UpdateBookBorrowingForm dto)
  {
    var borrowing = await context.BookBorrowings
                                 .Include(b => b.Loan)
                                 .FirstOrDefaultAsync(b => b.Id == dto.BookBorrowingId);
    if (borrowing == null) return NotFound(new BaseResponse<string> { Message = "Không tìm thấy bản ghi." });
    if (borrowing.Loan?.AprovalStatus != "pending" && borrowing.Loan?.AprovalStatus != "approved")
    {
      return BadRequest(new BaseResponse<string>
      {
        Status = "fail", Message = "Không thể chỉnh sửa sách đã được mượn"
      });
    }

    var now = DateTime.UtcNow;
    if (dto.BorrowedDate < now.AddMinutes(-30))
      return BadRequest(new BaseResponse<string> { Message = "Ngày mượn không được cách hiện tại quá 30 phút." });
    if (dto.ReturnDate <= dto.BorrowedDate.AddMinutes(30))
      return BadRequest(new BaseResponse<string> { Message = "Ngày trả phải lớn hơn ngày mượn ít nhất 30 phút." });
    borrowing.BorrowDate = dto.BorrowedDate;
    borrowing.ReturnDates = [dto.ReturnDate];
    borrowing.NoteBeforeBorrow = dto.NoteBeforeBorrow;
    if (dto.ImagesBeforeBorrow != null && dto.ImagesBeforeBorrow.Count != 0)
    {
      var folderPath = $"{StaticFolder.DIRPath_BORROWING_BEFORE}/{borrowing.Id}";
      var resources = new List<Resource>();
      foreach (var formFile in dto.ImagesBeforeBorrow.Where(formFile => formFile.Length > 0))
      {
        await using var stream = formFile.OpenReadStream();
        var uploadedPath = await googleCloudStorageHelper.UploadFileAsync(stream,
          formFile.FileName,
          folderPath,
          formFile.ContentType);
        resources.Add(new Resource
        {
          LocalUrl = uploadedPath,
          Type = "image",
          FileType = formFile.ContentType,
          Name = formFile.Name,
          SizeByte = formFile.Length,
        });
      }

      if (resources.Count != 0) { borrowing.BookImagesBeforeBorrow = resources; }
    }

    context.BookBorrowings.Update(borrowing);
    await context.SaveChangesAsync();
    return Ok(new BaseResponse<string> { Message = "Cập nhật thành công." });
  }

  [Authorize(Policy = "LibrarianPolicy")] [HttpPut("book-borrowing/update/all")]
  public async Task<IActionResult> BulkUpdateBorrowingDate([FromBody] BulkUpdateBookBorrowingDto dto)
  {
    var now = DateTime.UtcNow;
    foreach (var b in dto.BookBorrowings)
    {
      if (b.BorrowedDate < now.AddMinutes(-30))
        return BadRequest(new BaseResponse<string>
        {
          Message = $"Ngày mượn của bản ghi {b.BookBorrowingId} không hợp lệ."
        });
      if (b.ReturnDate <= b.BorrowedDate.AddMinutes(30))
        return BadRequest(new BaseResponse<string>
        {
          Message = $"Ngày trả của bản ghi {b.BookBorrowingId} không hợp lệ."
        });
    }

    var loan = await context.Loans.FindAsync(dto.LoanId);
    if (loan?.AprovalStatus != "pending" && loan?.AprovalStatus != "approved")
    {
      return BadRequest(
        new BaseResponse<string> { Status = "fail", Message = "Không thể chỉnh sửa sách đã được mượn", });
    }

    var ids = dto.BookBorrowings.Select(x => x.BookBorrowingId).ToList();
    var borrowings = await context.BookBorrowings
                                  .Where(b => ids.Contains(b.Id) && b.LoanId == dto.LoanId)
                                  .ToListAsync();
    foreach (var b in borrowings)
    {
      var updated = dto.BookBorrowings.First(x => x.BookBorrowingId == b.Id);
      b.BorrowDate = updated.BorrowedDate;
      b.ReturnDates = [updated.ReturnDate,];
    }

    await context.SaveChangesAsync();
    return Ok(new BaseResponse<string> { Message = "Cập nhật tất cả bản ghi thành công." });
  }

  public class UpdateBookBorrowingDateDto
  {
    public int BookBorrowingId { get; set; }
    public DateTime BorrowedDate { get; set; }
    public DateTime ReturnDate { get; set; }
  }

  public class UpdateBookBorrowingForm
  {
    public int BookBorrowingId { get; set; }
    public DateTime BorrowedDate { get; set; }
    public DateTime ReturnDate { get; set; }
    public List<IFormFile>? ImagesBeforeBorrow { get; set; }
    public string? NoteBeforeBorrow { get; set; }
  }

  public class BulkUpdateBookBorrowingDto
  {
    public int LoanId { get; set; }
    public List<UpdateBookBorrowingDateDto> BookBorrowings { get; set; } = new();
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
