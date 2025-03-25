using BU.Services.Implementation;
using BU.Services.Interface;
using Microsoft.Extensions.DependencyInjection;

namespace API.Configs;

public static class EmailConfig
{
  public static void ConfigureEmailService(this IServiceCollection services)
  {
    var emailFrom = Environment.GetEnvironmentVariable("EMAIL_FROM");
    var smtpServer = Environment.GetEnvironmentVariable("EMAIL_SMTP_SERVER");
    var emailUsername = Environment.GetEnvironmentVariable("EMAIL_USERNAME");
    var emailPassword = Environment.GetEnvironmentVariable("EMAIL_PASSWORD");
    var smtpPort = Environment.GetEnvironmentVariable("EMAIL_SMTP_PORT")!;
    var fromName = Environment.GetEnvironmentVariable("EMAIL_FROM_NAME")!;
    services.Configure<EmailSettings>(options =>
    {
      options.SmtpServer = smtpServer;
      options.SmtpPort = int.Parse(smtpPort);
      options.SmtpUsername = emailUsername;
      options.SmtpPassword = emailPassword;
      options.FromEmail = emailFrom;
      options.FromName = fromName;
    });
    services.AddTransient<IEmailService, EmailService>();
  }
}