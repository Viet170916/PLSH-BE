using System.Threading.Tasks;

namespace BU.Services.Interface
{
  public interface IEmailService
  {
    Task SendWelcomeEmailAsync(string fullName, string email, string password, string appLink);

    Task SendEmailAsync(string email, string subject, string htmlBody);
 
    Task SendOtpEmailAsync(string requestEmail, string otp);
  }
}