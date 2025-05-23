﻿using System;
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

  public async Task SendBorrowConfirmationEmailAsync(
    string email,
    string borrowerName,
    string phoneNumber,
    int borrowId,
    string borrowLink
  )
  {
    const string subject = "Xác nhận mượn sách";
    var htmlBody = GetBorrowConfirmationTemplate()
                   .Replace("{{BorrowerName}}", borrowerName)
                   .Replace("{{Email}}", email)
                   .Replace("{{PhoneNumber}}", phoneNumber)
                   .Replace("{{BorrowId}}", borrowId.ToString())
                   .Replace("{{BorrowLink}}", borrowLink);
    await SendEmailAsync(email, subject, htmlBody);
  }

  public async Task SendStatusUpdateEmailAsync(
    string email,
    string updater,
    string newStatus,
    string borrowerName,
    string phoneNumber,
    int borrowId,
    string borrowLink
  )
  {
    var subject = "Cập nhật trạng thái mượn sách";
    var htmlBody = GetStatusUpdateTemplate()
                   .Replace("{{Updater}}", updater)
                   .Replace("{{NewStatus}}", newStatus)
                   .Replace("{{BorrowerName}}", borrowerName)
                   .Replace("{{PhoneNumber}}", phoneNumber)
                   .Replace("{{BorrowId}}", borrowId.ToString())
                   .Replace("{{BorrowLink}}", borrowLink);
    await SendEmailAsync(email, subject, htmlBody);
  }

  public async Task SendReturnConfirmationEmailAsync(
    string email,
    string borrowerName,
    string bookTitle,
    int borrowId,
    string returnDate,
    string libraryName,
    string phoneNumber,
    string borrowLink
  )
  {
    var subject = "Xác nhận trả sách thành công";
    var htmlBody = GetReturnConfirmationTemplate()
                   .Replace("{{BorrowerName}}", borrowerName)
                   .Replace("{{BookTitle}}", bookTitle)
                   .Replace("{{BorrowId}}", borrowId.ToString())
                   .Replace("{{ReturnDate}}", returnDate)
                   .Replace("{{LibraryName}}", libraryName)
                   .Replace("{{Email}}", email)
                   .Replace("{{PhoneNumber}}", phoneNumber)
                   .Replace("{{BorrowLink}}", borrowLink);
    await SendEmailAsync(email, subject, htmlBody);
  }

  public async Task SendExtendConfirmationEmailAsync(
    string email,
    string borrowerName,
    DateTime newReturnDate,
    int borrowId,
    string borrowLink
  )
  {
    var subject = "Xác nhận gia hạn mượn sách";
    var htmlBody = GetExtendConfirmationTemplate()
                   .Replace("{{BorrowerName}}", borrowerName)
                   .Replace("{{NewReturnDate}}", newReturnDate.ToString("dd/MM/yyyy"))
                   .Replace("{{BorrowId}}", borrowId.ToString())
                   .Replace("{{BorrowLink}}", borrowLink);
    await SendEmailAsync(email, subject, htmlBody);
  }

    public async Task SendFineNotificationEmailAsync(string email, string borrowerName, double amount, string note, int borrowId, string fineLink)
    {
        var subject = "Thông báo phí phạt mới";
        var htmlBody = $@"
            <!DOCTYPE html>
            <html>
            <head><meta charset='UTF-8'></head>
            <body>
                <p>Chào <strong>{borrowerName}</strong>,</p>
                <p>Bạn đã bị phạt <strong>{amount} VNĐ</strong> do: {note}.</p>
                <p>Mã mượn: <strong>{borrowId}</strong></p>
                <p>Xem chi tiết tại: <a href='{fineLink}'>{fineLink}</a></p>
            </body>
            </html>";
        await SendEmailAsync(email, subject, htmlBody);
    }

    public async Task SendFineStatusUpdateEmailAsync(string email, string borrowerName, double amount, string note, int status, string fineLink)
    {
        var statusText = status switch
        {
            0 => "Chờ",
            1 => "Đã hủy",
            2 => "Thất bại",
            3 => "Hoàn thành",
            _ => "Không xác định"
        };
        var subject = "Cập nhật trạng thái phí phạt";
        var htmlBody = $@"
            <!DOCTYPE html>
            <html>
            <head><meta charset='UTF-8'></head>
            <body>
                <p>Chào <strong>{borrowerName}</strong>,</p>
                <p>Trạng thái phí phạt <strong>{amount} VNĐ</strong> (do: {note}) đã được cập nhật thành: <strong>{statusText}</strong>.</p>
                <p>Xem chi tiết tại: <a href='{fineLink}'>{fineLink}</a></p>
            </body>
            </html>";
        await SendEmailAsync(email, subject, htmlBody);
    }

    private string GetExtendConfirmationTemplate()
  {
    return @"<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        body { font-family: Arial, sans-serif; background-color: #f9f9f9; margin: 0; padding: 0; }
        .container { max-width: 600px; margin: 20px auto; background: #fff; padding: 20px; border-radius: 8px; box-shadow: 0px 0px 5px rgba(0, 0, 0, 0.1); }
        .header { background-color: #28a745; color: #fff; padding: 15px; font-size: 18px; text-align: center; border-radius: 8px 8px 0 0; }
        .content { padding: 20px; color: #333; line-height: 1.5; }
        .button { display: inline-block; background: #28a745; color: white; text-decoration: none; padding: 10px 15px; border-radius: 5px; font-weight: bold; }
        .footer { font-size: 12px; color: #777; text-align: center; padding: 10px; border-top: 1px solid #ddd; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>Xác nhận gia hạn sách</div>
        <div class='content'>
            <p>Chào <strong>{{BorrowerName}}</strong>,</p>
            <p>Chúng tôi xin thông báo rằng yêu cầu gia hạn sách của bạn đã được chấp thuận.</p>
            <p><strong>Ngày trả mới:</strong> {{NewReturnDate}}</p>
            <p><strong>Mã mượn:</strong> {{BorrowId}}</p>
            <p>Bạn có thể xem chi tiết đơn mượn tại đường link bên dưới:</p>
            <p><a href='{{BorrowLink}}' class='button'>Xem chi tiết</a></p>
        </div>
        <div class='footer'>
            <p>Thư viện BookHive | Hỗ trợ: support@library.com</p>
        </div>
    </div>
</body>
</html>";
  }

  private string GetBorrowConfirmationTemplate()
  {
    return @"<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        body { font-family: Arial, sans-serif; background-color: #f9f9f9; margin: 0; padding: 0; }
        .container { max-width: 600px; margin: 20px auto; background: #fff; padding: 20px; border-radius: 8px; box-shadow: 0px 0px 5px rgba(0, 0, 0, 0.1); }
        .header { background-color: #007BFF; color: #fff; padding: 15px; font-size: 18px; text-align: center; border-radius: 8px 8px 0 0; }
        .content { padding: 20px; color: #333; line-height: 1.5; }
        .button { display: inline-block; background: #007BFF; color: white; text-decoration: none; padding: 10px 15px; border-radius: 5px; font-weight: bold; }
        .footer { font-size: 12px; color: #777; text-align: center; padding: 10px; border-top: 1px solid #ddd; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>Xác nhận mượn sách</div>
        <div class='content'>
            <p>Chào <strong>{{BorrowerName}}</strong>,</p>
            <p>Yêu cầu mượn sách của bạn đã được xác nhận.</p>
            <p><strong>Mã mượn:</strong> {{BorrowId}}</p>
            <p><strong>Email:</strong> {{Email}}</p>
            <p><strong>Số điện thoại:</strong> {{PhoneNumber}}</p>
            <p>Bạn có thể xem chi tiết đơn mượn tại đường link bên dưới:</p>
            <p><a href='{{BorrowLink}}' class='button'>Xem chi tiết</a></p>
        </div>
        <div class='footer'>
            <p>Thư viện BookHive | Hỗ trợ: support@library.com</p>
        </div>
    </div>
</body>
</html>";
  }

  private string GetStatusUpdateTemplate()
  {
    return @"<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        body { font-family: Arial, sans-serif; background-color: #f9f9f9; margin: 0; padding: 0; }
        .container { max-width: 600px; margin: 20px auto; background: #fff; padding: 20px; border-radius: 8px; box-shadow: 0px 0px 5px rgba(0, 0, 0, 0.1); }
        .header { background-color: #FA7C54; color: #fff; padding: 15px; font-size: 18px; text-align: center; border-radius: 8px 8px 0 0; }
        .content { padding: 20px; color: #333; line-height: 1.5; }
        .info { margin-top: 15px; padding: 10px; background: #f9f9f9; border-left: 4px solid #FA7C54; }
        .button { display: inline-block; background: #FA7C54; color: white; text-decoration: none; padding: 10px 15px; border-radius: 5px; font-weight: bold; }
        .footer { font-size: 12px; color: #777; text-align: center; padding: 10px; border-top: 1px solid #ddd; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>Cập nhật trạng thái mượn sách</div>
        <div class='content'>
            <p>Chào <strong>{{BorrowerName}}</strong>,</p>
            <p>Trạng thái mới của lượt mượn sách có mã <strong>#{{BorrowId}}</strong> là: <strong>{{NewStatus}}</strong>.</p>
            <p><strong>Người cập nhật:</strong> {{Updater}}</p>

            <div class='info'>
                <p><strong>Thông tin người mượn:</strong></p>
                <p><strong>Email:</strong> {{Email}}</p>
                <p><strong>Số điện thoại:</strong> {{PhoneNumber}}</p>
            </div>

            <p>Bạn có thể xem chi tiết đơn mượn tại đường link bên dưới:</p>
            <p><a href='{{BorrowLink}}' class='button'>Xem chi tiết</a></p>
        </div>
        <div class='footer'>
            <p>Thư viện BookHive | Hỗ trợ: support@library.com</p>
        </div>
    </div>
</body>
</html>";
  }

  private string GetReturnConfirmationTemplate()
  {
    return @"<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        body { font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0; }
        .container { max-width: 600px; margin: 20px auto; background: #fff; padding: 20px; border-radius: 8px; box-shadow: 0px 0px 10px rgba(0, 0, 0, 0.1); }
        .header { background-color: #FA7C54; color: #fff; padding: 15px; font-size: 22px; text-align: center; font-weight: bold; border-radius: 8px 8px 0 0; }
        .content { padding: 20px; line-height: 1.6; color: #333; }
        .info { margin-top: 15px; padding: 10px; background: #f9f9f9; border-left: 4px solid #FA7C54; }
        .footer { text-align: center; font-size: 14px; color: #777; margin-top: 20px; padding-top: 10px; border-top: 1px solid #ddd; }
        .link { display: inline-block; background: #FA7C54; color: #fff; padding: 10px 15px; border-radius: 5px; text-decoration: none; font-weight: bold; margin-top: 15px; }
        .link:hover { background: #e76b45; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>Xác nhận trả sách</div>
        <div class='content'>
            <p>Xin chào <strong>{{BorrowerName}}</strong>,</p>
            <p>Bạn đã trả thành công sách: <strong>{{BookTitle}}</strong></p>
            <p><strong>Mã lượt mượn:</strong> #{{BorrowId}}</p>
            <p><strong>Ngày trả:</strong> {{ReturnDate}}</p>
            <p><strong>Thư viện nhận sách:</strong> {{LibraryName}}</p>

            <div class='info'>
                <p><strong>Thông tin người mượn:</strong></p>
                <p>Email: {{Email}}</p>
                <p>Số điện thoại: {{PhoneNumber}}</p>
            </div>

            <p>Để xem chi tiết, vui lòng truy cập vào liên kết bên dưới:</p>
            <a class='link' href='{{BorrowLink}}'>Xem chi tiết lượt mượn</a>
        </div>
        <div class='footer'>
            <p>Thư viện BookHive | Liên hệ hỗ trợ: support@library.com</p>
        </div>
    </div>
</body>
</html>";
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
    public async Task SendReturnReminderEmailAsync(
    string email,
    string borrowerName,
    string bookTitle,
    int borrowId,
    DateTime returnDate,
    string borrowLink)
    {
        var subject = "Nhắc nhở trả sách";
        var htmlBody = GetReturnReminderTemplate()
                       .Replace("{{BorrowerName}}", borrowerName)
                       .Replace("{{BookTitle}}", bookTitle)
                       .Replace("{{BorrowId}}", borrowId.ToString())
                       .Replace("{{ReturnDate}}", returnDate.ToString("dd/MM/yyyy"))
                       .Replace("{{BorrowLink}}", borrowLink);
        await SendEmailAsync(email, subject, htmlBody);
    }

    private string GetReturnReminderTemplate()
    {
        return @"<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        body { font-family: Arial, sans-serif; background-color: #f9f9f9; margin: 0; padding: 0; }
        .container { max-width: 600px; margin: 20px auto; background: #fff; padding: 20px; border-radius: 8px; box-shadow: 0px 0px 5px rgba(0, 0, 0, 0.1); }
        .header { background-color: #FFC107; color: #000; padding: 15px; font-size: 18px; text-align: center; border-radius: 8px 8px 0 0; }
        .content { padding: 20px; color: #333; line-height: 1.5; }
        .button { display: inline-block; background: #FFC107; color: #000; text-decoration: none; padding: 10px 15px; border-radius: 5px; font-weight: bold; }
        .footer { font-size: 12px; color: #777; text-align: center; padding: 10px; border-top: 1px solid #ddd; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>Nhắc nhở trả sách</div>
        <div class='content'>
            <p>Chào <strong>{{BorrowerName}}</strong>,</p>
            <p>Hạn trả sách <strong>{{BookTitle}}</strong> của bạn còn một ngày.</p>
            <p><strong>Ngày trả dự kiến:</strong> {{ReturnDate}}</p>
            <p><strong>Mã mượn:</strong> {{BorrowId}}</p>
            <p>Vui lòng mang sách đến trả đúng hạn để tránh bị phạt.</p>
            <p>Bạn có thể xem chi tiết đơn mượn tại đường link bên dưới:</p>
            <p><a href='{{BorrowLink}}' class='button'>Xem chi tiết</a></p>
        </div>
        <div class='footer'>
            <p>Thư viện BookHive | Hỗ trợ: support@library.com</p>
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
