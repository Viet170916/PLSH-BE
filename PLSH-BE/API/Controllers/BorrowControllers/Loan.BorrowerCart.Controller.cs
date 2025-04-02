using System.Security.Claims;
using API.DTO;
using API.DTO.Loan;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model.Entity.Borrow;

namespace API.Controllers.BorrowControllers;

public partial class LoanController
{
  public class AddToCartRequestDto
  {
    public int BookId { get; set; }
    public int Quantity { get; set; }
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
          BookInstance = availableInstance, BorrowingStatus = "on-loan", BorrowDate = DateTime.Now
        });
        // availableInstance.IsInBorrowing = true;
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
}
