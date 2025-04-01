using System.Collections.Generic;
using API.Common;
using API.DTO;
using API.DTO.Loan;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model.Entity;
using System.Text.Json;
using Model.Entity.Borrow;

namespace API.Controllers.BorrowControllers;
[Authorize("LibrarianPolicy")]

public partial class LoanController
{
  [Authorize(Policy = "LibrarianPolicy")] [HttpPut("borrowed-book/{bookBorrowingId}/return-confirm")]
  public async Task<IActionResult> UpdateReturnInfo(
    [FromRoute] int bookBorrowingId,
    [FromForm] ReturnInfoRequestDto request
  )
  {
    var bookBorrowing = await context.BookBorrowings
                                     .Include(bb => bb.BookInstance) // Thêm Include để lấy thông tin book instance
                                     .Include(bb => bb.BookImagesAfterBorrow)
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
    bookBorrowing.ActualReturnDate = DateTime.Now;
    await context.SaveChangesAsync();
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
    var validStatuses = new[] { "pending", "approved", "rejected" };
    if (!validStatuses.Contains(request.Status.ToLower()))
    {
      return BadRequest(new BaseResponse<object?>
      {
        message = "Trạng thái không hợp lệ. Chỉ chấp nhận: pending, approved, rejected.",
        status = "error",
        data = null
      });
    }

    var loan = await context.Loans.FindAsync(id);
    if (loan == null)
    {
      return NotFound(new BaseResponse<object?>
      {
        message = "Không tìm thấy đơn mượn.", status = "error", data = null
      });
    }

    loan.AprovalStatus = request.Status;
    await context.SaveChangesAsync();
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

    var bookBorrowing = await context.BookBorrowings.FindAsync(bookBorrowingId);
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
