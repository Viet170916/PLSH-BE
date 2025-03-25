using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MailKit.Net.Smtp;
using MimeKit;
using MailKit.Security;
using BU.Services.Interface;

namespace BU.Services.Implementation;

public class EmailService(IOptions<EmailSettings> emailSettings) : IEmailService
{
  private readonly EmailSettings _emailSettings = emailSettings.Value;

  public async Task SendEmailAsync(string email, string subject, string htmlBody)
  {
    var emailMessage = new MimeMessage();
    emailMessage.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
    emailMessage.To.Add(MailboxAddress.Parse(email));
    emailMessage.Subject = subject;
    var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
    emailMessage.Body = bodyBuilder.ToMessageBody();
    using var client = new SmtpClient();
    await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, SecureSocketOptions.StartTls);
    await client.AuthenticateAsync(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword);
    await client.SendAsync(emailMessage);
    await client.DisconnectAsync(true);
  }

  public async Task SendOtpEmailAsync(string email, string otp)
  {
    var subject = "Xác thực OTP";
    var htmlBody = $@"
        <h1>Mã OTP của bạn</h1>
        <p>Mã xác thực: <strong>{otp}</strong></p>
        <p>Mã có hiệu lực trong 5 phút.</p>
         ";
    await SendEmailAsync(email, subject, htmlBody);
  }

  public async Task SendWelcomeEmailAsync(string fullName, string email, string password, string appLink)
  {
    var emailTemplate = GetEmailTemplate();
    var emailBody = emailTemplate
                    .Replace("{{FullName}}", fullName)
                    .Replace("{{Email}}", email)
                    .Replace("{{Password}}", password)
                    .Replace("{{AppLink}}", appLink);
    await SendEmailAsync(email, "Chào mừng đến với Ứng dụng", emailBody);
  }

  private string GetEmailTemplate()
  {
    return @"
            <!DOCTYPE html>
            <html lang='vi'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Chào mừng đến với Ứng dụng</title>
                <style>
                    body {
                        font-family: Arial, sans-serif;
                        background-color: #f5f5f5;
                        margin: 0;
                        padding: 0;
                    }
                    .container {
                        max-width: 600px;
                        margin: 20px auto;
                        background-color: #ffffff;
                        padding: 20px;
                        border-radius: 10px;
                        box-shadow: 0px 4px 10px rgba(0, 0, 0, 0.1);
                        text-align: center;
                    }
                    .header {
                        background-color: #FA7C54;
                        padding: 15px;
                        border-top-left-radius: 10px;
                        border-top-right-radius: 10px;
                        color: #fff;
                        font-size: 24px;
                        font-weight: bold;
                    }
                    .content {
                        padding: 20px;
                        color: #333;
                        text-align: left;
                    }
                    .content p {
                        font-size: 16px;
                        line-height: 1.6;
                    }
                    .info {
                        background-color: #FA7C54;
                        color: #fff;
                        padding: 10px;
                        border-radius: 5px;
                        font-size: 16px;
                        font-weight: bold;
                        margin-top: 15px;
                    }
                    .button {
                        display: inline-block;
                        margin-top: 20px;
                        padding: 12px 20px;
                        background-color: #FA7C54;
                        color: #fff;
                        text-decoration: none;
                        font-weight: bold;
                        border-radius: 5px;
                        transition: 0.3s;
                    }
                    .button:hover {
                        background-color: #e56742;
                    }
                    .footer {
                        margin-top: 20px;
                        font-size: 14px;
                        color: #777;
                    }
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>Chào mừng đến với Ứng dụng của chúng tôi!</div>
                    <div class='content'>
                        <p>Xin chào <strong>{{FullName}}</strong>,</p>
                        <p>Chúc mừng bạn đã được tạo tài khoản trên ứng dụng của chúng tôi. Dưới đây là thông tin tài khoản của bạn:</p>
                        <div class='info'>
                            Email: <strong>{{Email}}</strong><br>
                            Mật khẩu: <strong>{{Password}}</strong>
                        </div>
                        <p>Vui lòng sử dụng thông tin này để đăng nhập vào hệ thống.</p>
                        <a href='{{AppLink}}' class='button'>Truy cập ứng dụng</a>
                    </div>
                    <div class='footer'>
                        Nếu bạn không yêu cầu tài khoản này, vui lòng liên hệ hỗ trợ ngay lập tức.
                    </div>
                </div>
            </body>
            </html>";
  }
}

public class EmailSettings
{
  public string SmtpServer { get; set; }
  public int SmtpPort { get; set; }
  public string SmtpUsername { get; set; }
  public string SmtpPassword { get; set; }
  public string FromEmail { get; set; }
  public string FromName { get; set; }
}