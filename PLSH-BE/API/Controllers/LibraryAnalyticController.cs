using BU.Services.Interface;
using Data.DatabaseContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Triangulate.Tri;
using System.Net.Http;
using Common.Enums;

namespace API.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class LibraryAnalyticController : ControllerBase
  {
    private readonly AppDbContext _context;
    private readonly ILogger<ManageAccountController> _logger;

    public LibraryAnalyticController(AppDbContext context, ILogger<ManageAccountController> logger)
    {
      _context = context;
      _logger = logger;
    }

    //lấy số lượng thành viên,
    [HttpGet("total-member")] public async Task<IActionResult> GetTotalMembers()
    {
      try
      {
        var totalMembers = await _context.Accounts.CountAsync();
        return Ok(totalMembers);
      }
      catch (HttpRequestException ex)
      {
        _logger.LogError(ex, "/api/library-analytic/total-member - An error occurred");
        return StatusCode(500, 0);
      }
    }

    //Lấy số lượng thành viên đang Active
    [HttpGet("total-active-member")] public async Task<IActionResult> GetTotalActiveMembers()
    {
      try
      {
        var totalActiveMembers = await _context.Accounts.CountAsync(x => x.Status == 1);
        return Ok(totalActiveMembers);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "/api/library-analytic/total-active-member - An error occurred");
        return StatusCode(500, 0);
      }
    }

    //Lấy số lượng thành viên Active và role = Borrower
    [HttpGet("total-borrower-member")] public async Task<IActionResult> GetTotalBorrowerMemberActive()
    {
      try
      {
        //var totalBorrowerMembers = await _context.Accounts
        //.Include(a => a.Role)
        //.CountAsync(a => a.Status == 1 && a.Role.Name == "Borrower");
        // return Ok(totalBorrowerMembers);
        return Ok();
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "/api/library-analytic/total-borrower-member - An error occurred");
        return StatusCode(500, 0);
      }
    }

    //số lượng sách
    [HttpGet("total-books")] public async Task<IActionResult> GetTotalBook()
    {
      try
      {
        var totalBooks = await _context.Books.SumAsync(b => b.TotalCopies);
        return Ok(totalBooks);
      }
      catch (Exception) { return StatusCode(500, 0); }
    }

    //số lượng sách đang được mượn,
    [HttpGet("total-borrowed-books")] public async Task<IActionResult> GetTotalBorrowedBooks()
    {
      try
      {
        // Tính tổng số sách đang được mượn (trạng thái "Borrowed")
        var totalBorrowedBooks = await _context.BookBorrowings
                                               .Where(bb => bb.BorrowingStatus == BorrowingStatus.Borrowed)
                                               .CountAsync();
        return Ok(totalBorrowedBooks);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "/api/library-analytic/total-borrowed-books - An error occurred");
        return StatusCode(500, 0);
      }
    }

    //số lượng sách quá hạn
    //B1: Cập nhật sách quá hạn
    [HttpPost("update-overdue-books")] public async Task<IActionResult> UpdateOverdueBooks()
    {
      try
      {
        // Lấy danh sách sách đang mượn nhưng quá hạn 14 ngày kể từ khi mượn
        var overdueBooks = await _context.BookBorrowings
                                         .Where(bb => bb.BorrowingStatus == BorrowingStatus.Borrowed &&
                                                      bb.ReturnDate == null
                                                      && bb.BorrowingDate != null &&
                                                      bb.BorrowingDate.Value.AddDays(14) < DateTime.Now)
                                         .ToListAsync();
        if (!overdueBooks.Any()) { return Ok("Không có sách nào quá hạn."); }

        // Cập nhật trạng thái thành "Overdue"
        foreach (var book in overdueBooks)
        {
          book.BorrowingStatus = BorrowingStatus.Overdue;
          book.IsFined = true; // Đánh dấu bị phạt
          book.FineType = FineType.LateReturn;
          book.Note = "Quá hạn trả sách.";
        }

        // Lưu thay đổi vào database
        await _context.SaveChangesAsync();
        return Ok($"Cập nhật {overdueBooks.Count} sách quá hạn thành công.");
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Lỗi khi cập nhật trạng thái sách quá hạn");
        return StatusCode(500, "Lỗi hệ thống khi cập nhật trạng thái sách quá hạn.");
      }
    }

    //B2:Lấy số lượng sách quá hạn
    [HttpGet("total-overdue-books")] public async Task<IActionResult> GetTotalOverdueBooks()
    {
      try
      {
        var totalOverdueBooks = await _context.BookBorrowings
                                              .CountAsync(bb => bb.BorrowingStatus == BorrowingStatus.Overdue);
        return Ok(totalOverdueBooks);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Lỗi khi lấy số lượng sách quá hạn.");
        return StatusCode(500, "Lỗi hệ thống khi lấy số lượng sách quá hạn.");
      }
    }

    //số lượng sách gia hạn(ĐKGH: là sách đang mượn và gia hạn trước 3 ngày trả sách,,...)
    //B1: Tự động gia hạn nếu sách quá hạn 

    //B2: Giới hạn số lần gia hạn là 2 lần 

    //B3: Các điều kiện từ chối gia hạn
    // Có người đặt trước sách → Không thể gia hạn.
    // Vượt quá số lần gia hạn (thường là 1-3 lần).
    // Tài khoản bị khóa(do quá hạn, mất sách, chưa thanh toán phí phạt).
    // Sách thuộc danh mục đặc biệt(sách hiếm, sách tham khảo, giáo trình).

    //[HttpPost("renew-book/{borrowingId}")]
    //public async Task<IActionResult> RenewBook(int borrowingId)
    //{
    //    var borrowing = await _context.BookBorrowings
    //        .Include(bb => bb.BookDetail)
    //        .Include(bb => bb.Loan)
    //        .ThenInclude(l => l.Borrower)
    //        .FirstOrDefaultAsync(bb => bb.Id == borrowingId);

    //    if (borrowing == null)
    //        return NotFound("Không tìm thấy thông tin mượn sách.");

    //    var account = borrowing.Loan?.AccountControllers;
    //    if (account == null)
    //        return BadRequest("Không tìm thấy tài khoản của người mượn.");

    //    // Kiểm tra điều kiện gia hạn
    //    bool hasReservation = await _context.BookReservations
    //        .AnyAsync(r => r.BookId == borrowing.BookDetailId && r.Status == ReservationStatus.Pending);

    //    if (hasReservation)
    //        return BadRequest("Sách đã có người đặt trước, không thể gia hạn.");

    //    int maxRenewals = 2;
    //    int currentRenewals = await _context.RenewalLogs
    //        .CountAsync(r => r.BookBorrowingId == borrowingId);

    //    if (currentRenewals >= maxRenewals)
    //        return BadRequest("Số lần gia hạn đã đạt giới hạn.");

    //    // Nếu sách gần đến hạn (2 ngày trước khi hết hạn), gọi API gửi thông báo
    //    if (borrowing.ReturnDate.HasValue && (borrowing.ReturnDate.Value - DateTime.UtcNow).TotalDays <= 2)
    //    {
    //        await SendNotification(account.Id, "Nhắc nhở: Sắp đến hạn trả sách",
    //            $"Sách '{borrowing.BookDetail?.StatusDescription}' sẽ đến hạn vào ngày {borrowing.ReturnDate?.ToString("yyyy-MM-dd")}. Hãy gia hạn hoặc trả sách đúng hạn!");
    //    }

    //    // Gia hạn sách thêm 14 ngày
    //    borrowing.ReturnDate = borrowing.ReturnDate?.AddDays(14);
    //    await _context.SaveChangesAsync();

    //    // Ghi lại log gia hạn
    //    var renewalLog = new RenewalLog
    //    {
    //        BookBorrowingId = borrowingId,
    //        RenewalDate = DateTime.UtcNow
    //    };
    //    _context.RenewalLogs.Add(renewalLog);
    //    await _context.SaveChangesAsync();

    //    // Gửi thông báo gia hạn thành công
    //    await SendNotification(account.Id, "Gia hạn sách thành công",
    //        $"Sách '{borrowing.BookDetail?.StatusDescription}' của bạn đã được gia hạn đến ngày {borrowing.ReturnDate?.ToString("yyyy-MM-dd")}.");

    //    return Ok("Gia hạn sách thành công và đã gửi thông báo.");
    //}
    /// <summary>
    /// API để kiểm tra và xử lý sách bị trả muộn.
    /// </summary>
    /// 

    //[HttpPost("process-overdue-books")]
    //public async Task<IActionResult> ProcessOverdueBooks()
    //{
    //    var overdueBooks = await _context.BookBorrowings
    //        .Where(bb => bb.ReturnDate < DateTime.UtcNow && bb.BorrowingStatus == BorrowingStatus.Borrowed)
    //        .Include(bb => bb.Loan)
    //        .ThenInclude(l => l.AccountControllers)
    //        .ToListAsync();

    //    foreach (var borrowing in overdueBooks)
    //    {
    //        borrowing.BorrowingStatus = BorrowingStatus.Overdue;
    //        borrowing.isFined = true;
    //        borrowing.FineType = FineType.LateReturn;
    //        await SendNotification(borrowing.Loan.AccountControllers.Id, "Thông báo: Bạn bị phạt do trả sách muộn",
    //            $"Sách '{borrowing.BookDetail?.StatusDescription}' đã quá hạn. Bạn sẽ bị tính phí phạt.");
    //    }

    //    await _context.SaveChangesAsync();

    //    return Ok($"Đã cập nhật {overdueBooks.Count} sách bị quá hạn và gửi thông báo.");
    //}
    /// <summary>
    /// Gửi thông báo đến người dùng
    /// </summary>
    //private async Task SendNotification(int accountId, string title, string content)
    //{
    //    var notification = new Notification
    //    {
    //        AccountId = accountId,
    //        Title = title,
    //        Content = content,
    //        Status = NotificationStatus.Unread,
    //        CreatedAt = DateTime.UtcNow
    //    };
    //    _context.Notifications.Add(notification);
    //    await _context.SaveChangesAsync();
    //}
  }
}