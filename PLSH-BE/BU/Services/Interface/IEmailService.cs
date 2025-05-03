using System;
using System.Threading.Tasks;

namespace BU.Services.Interface
{
  public interface IEmailService
  {
    Task SendWelcomeEmailAsync(string fullName, string email, string password, string appLink);
    Task SendEmailAsync(string email, string subject, string htmlBody);
    Task SendOtpEmailAsync(string requestEmail, string otp);

    Task SendBorrowConfirmationEmailAsync(
      string email,
      string borrowerName,
      string phoneNumber,
      int borrowId,
      string borrowLink
    );

    Task SendStatusUpdateEmailAsync(
      string email,
      string updater,
      string newStatus,
      string borrowerName,
      string phoneNumber,
      int borrowId,
      string borrowLink
    );

    Task SendReturnConfirmationEmailAsync(
      string email,
      string borrowerName,
      string bookTitle,
      int borrowId,
      string returnDate,
      string libraryName,
      string phoneNumber,
      string borrowLink
    );

    Task SendExtendConfirmationEmailAsync(
      string borrowerEmail,
      string borrowerFullName,
      DateTime newReturnDate,
      int loanId,
      string link
    );
        Task SendReturnReminderEmailAsync(string email, string borrowerName, string bookTitle, int borrowId, DateTime returnDate, string borrowLink);
        Task SendFineNotificationEmailAsync(string email, string borrowerName, double amount, string note, int borrowId, string fineLink);
        Task SendFineStatusUpdateEmailAsync(string email, string borrowerName, double amount, string note, int status, string fineLink);
    }
}
