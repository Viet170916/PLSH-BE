using System;
using System.Threading;
using System.Threading.Tasks;
using BU.Models.DTO.Notification;
using BU.Services.Interface;
using Data.DatabaseContext;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace BU.Services.Implementation
{
    public class ReturnDateNotificationService : BackgroundService
    {
        private readonly ILogger<ReturnDateNotificationService> _logger;
        private readonly IServiceProvider _services;

        public ReturnDateNotificationService(
            ILogger<ReturnDateNotificationService> logger,
            IServiceProvider services)
        {
            _logger = logger;
            _services = services;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Return Date Notification Service running.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _services.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                        var today = DateTime.UtcNow.Date;
                        var tomorrow = today.AddDays(1);

                        // 📌 1. Các phiếu mượn sắp đến hạn vào ngày mai
                        var borrowingsDueTomorrow = context.BookBorrowings
                            .Where(b => b.ReturnDates.Any(rd => rd.Date == tomorrow) &&
                                        b.BorrowingStatus != "returned")
                            .ToList();

                        foreach (var borrowing in borrowingsDueTomorrow)
                        {
                            var user = context.Borrowers.FirstOrDefault(u => u.Id == borrowing.Loan.BorrowerId);
                            var account = context.Accounts.FirstOrDefault(x => x.Id == user.AccountId);
                            if (user == null) continue;

                            var notificationDto = new NotificationDto
                            {
                                Title = "Nhắc nhở sắp đến hạn trả sách",
                                Content = "Hạn trả sách của bạn còn một ngày. Hãy chuẩn bị mang sách đến trả.",
                                Reference = "BookBorrowing",
                                ReferenceId = borrowing.Id
                            };

                            await notificationService.SendNotificationToUserAsync(user.Id, notificationDto);

                            var emailSubject = "Nhắc nhở sắp đến hạn trả sách";
                            var emailBody = $@"
                            <h1>Nhắc nhở sắp đến hạn trả sách</h1>
                            <p>Hạn trả sách của bạn còn một ngày. Hãy chuẩn bị mang sách đến thư viện.</p>
                            <p>Tên sách: {borrowing.BookInstance.Book.Title}</p>
                            <p>Mã mượn: {borrowing.Id}</p>
                            <p>Ngày trả dự kiến: {tomorrow:dd/MM/yyyy}</p>
                    ";

                            await emailService.SendEmailAsync(account.Email, emailSubject, emailBody);
                        }

                        // 📌 2. Các phiếu mượn đã quá hạn
                        var overdueBorrowings = context.BookBorrowings
                            .Where(b => b.ReturnDates.Any(rd => rd.Date < today) &&
                                        b.BorrowingStatus != "returned")
                            .ToList();

                        foreach (var borrowing in overdueBorrowings)
                        {
                            var user = context.Borrowers.FirstOrDefault(u => u.Id == borrowing.Loan.BorrowerId);
                            var account = context.Accounts.FirstOrDefault(x => x.Id == user.AccountId);
                            if (user == null) continue;

                            var firstReturnDate = borrowing.ReturnDates.OrderBy(r => r.Date).FirstOrDefault();
                            var dueDate = firstReturnDate != null ? firstReturnDate.Date : today;

                            var overdueDays = (today - dueDate).Days;

                            var notificationDto = new NotificationDto
                            {
                                Title = "Thông báo quá hạn trả sách",
                                Content = $"Bạn đã quá hạn trả sách {overdueDays} ngày. Vui lòng trả sách sớm để tránh bị phạt.",
                                Reference = "BookBorrowing",
                                ReferenceId = borrowing.Id
                            };

                            await notificationService.SendNotificationToUserAsync(user.Id, notificationDto);

                            var emailSubject = "Thông báo quá hạn trả sách";
                            var emailBody = $@"
                        <h1>Thông báo quá hạn trả sách</h1>
                        <p>Bạn đã quá hạn {overdueDays} ngày. Vui lòng đến thư viện trả sách để tránh bị phạt.</p>
                        <p>Tên sách: {borrowing.BookInstance.Book.Title}</p>
                        <p>Mã mượn: {borrowing.Id}</p>
                        <p>Ngày trả dự kiến: {dueDate:dd/MM/yyyy}</p>
                    ";

                            await emailService.SendEmailAsync(account.Email, emailSubject, emailBody);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in Return Date Notification Service");
                }

                // Chờ tới ngày mai
                var nextRun = DateTime.Today.AddDays(1);
                var delay = nextRun - DateTime.Now;
                await Task.Delay(delay, stoppingToken);
            }
        }


    }
}