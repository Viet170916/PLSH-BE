using BU.Models.DTO.Notification;
using BU.Services.Interface;
using Data.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using Model.Entity.Borrow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace BU.Services.Implementation
{
    public class FineAutoCreationService
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly INotificationService _notificationService;
        public FineAutoCreationService(AppDbContext context, IEmailService emailService, INotificationService notificationService)
        {
            _context = context;
            _emailService = emailService;
            _notificationService = notificationService;
        }
        public async Task CheckAndCreateFinesAsync()
        {
            var borrowings = await _context.BookBorrowings
             .Include(bb => bb.Loan)
             .ThenInclude(l => l.Borrower)
             .Where(bb => bb.BorrowingStatus != "returned" && bb.ActualReturnDate.HasValue)
             .ToListAsync();
            foreach (var bb in borrowings)
            {
                var lastReturnDate = bb.ReturnDates?.LastOrDefault();
                if (lastReturnDate.HasValue && bb.ActualReturnDate > lastReturnDate)
                {
                    var daysLate = (bb.ActualReturnDate.Value - lastReturnDate.Value).Days;
                    if (daysLate > 0)
                    {
                        var fine = new Fine
                        {
                            FineDate = DateTime.UtcNow,
                            IsFined = true,
                            FineType = 0, 
                            FineByDate = 5000,
                            Amount = 5000.0 * daysLate,
                            BookBorrowingId = bb.Id,
                            BorrowerId = bb.Loan?.BorrowerId ?? 0,
                            Status = 0, 
                            Note = $"Trả muộn {daysLate} ngày"
                        };
                        _context.Fines.Add(fine);
                        await _context.SaveChangesAsync();
                        var notificationDto = new NotificationDto
                        {
                            Title = "Phí phạt mới",
                            Content = $"Bạn đã bị phạt {fine.Amount} VNĐ do {fine.Note}.",
                            Reference = "Fine",
                            ReferenceId = fine.Id,
                            AccountId = fine.BorrowerId
                        };
                        await _notificationService.SendNotificationToUserAsync(fine.BorrowerId, notificationDto);
                        var CLIENT_HOST = "https://book-hive.space";
                        var link = $"{CLIENT_HOST}/fines/{fine.Id}";

                        await _emailService.SendFineNotificationEmailAsync(
                            bb.Loan.Borrower.Email ?? "",
                            bb.Loan.Borrower.FullName ?? "",
                            fine.Amount ?? 0,
                            fine.Note ?? "",
                            bb.Loan.Id,
                            link
                            );
                    }
                }
            }
        }
    }
}
