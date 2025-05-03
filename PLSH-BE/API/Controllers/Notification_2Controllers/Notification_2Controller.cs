using AutoMapper;
using BU.Models.DTO.Notification;
using BU.Services.Interface;
using Data.DatabaseContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace API.Controllers.Notification_2Controllers
{
    [Route("api/v1/notification")]
    [ApiController]
    public partial class NotificationController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;

        public NotificationController(
            AppDbContext context,
            IEmailService emailService,
            INotificationService notificationService,
            IMapper mapper)
        {
            _context = context;
            _emailService = emailService;
            _notificationService = notificationService;
            _mapper = mapper;
        }

        [HttpPost("return-reminder")]
        [Authorize(Roles = "Admin,Librarian")]
        public async Task<IActionResult> SendReturnReminder(int borrowId)
        {
            var borrowing = await _context.BookBorrowings
                .Include(b => b.BookInstance)
                .ThenInclude(bi => bi.Book)
                .Include(b => b.Loan)
                .ThenInclude(l => l.Borrower)
                .FirstOrDefaultAsync(b => b.Id == borrowId);

            if (borrowing == null)
            {
                return NotFound("Borrow record not found");
            }

            var user = borrowing.Loan.Borrower;
            var tomorrow = DateTime.UtcNow.AddDays(1).Date;
            if (!borrowing.ReturnDates.Any(rd => rd.Date == tomorrow))
            {
                return BadRequest("This book is not due tomorrow");
            }

            // Send notification
            var notificationDto = new NotificationDto
            {
                Title = "Nhắc nhở trả sách",
                Content = "Hạn trả sách của bạn còn một ngày. Hãy mang sách đến trả vào ngày mai.",
                Reference = "BookBorrowing",
                ReferenceId = borrowing.Id
            };

            await _notificationService.SendNotificationToUserAsync(user.Id, notificationDto);

            // Send email
            var emailSubject = "Nhắc nhở trả sách";
            var emailBody = $@"
                <h1>Nhắc nhở trả sách</h1>
                <p>Hạn trả sách của bạn còn một ngày. Hãy mang sách đến trả vào ngày mai.</p>
                <p>Tên sách: {borrowing.BookInstance.Book.Title}</p>
                <p>Mã mượn: {borrowing.Id}</p>
                <p>Ngày trả dự kiến: {tomorrow.ToString("dd/MM/yyyy")}</p>
            ";

            await _emailService.SendEmailAsync(user.Email, emailSubject, emailBody);

            return Ok(new { message = "Return reminder sent successfully" });
        }
    }
}