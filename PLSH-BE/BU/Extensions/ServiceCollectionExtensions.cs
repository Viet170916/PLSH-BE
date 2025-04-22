using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using BU.Services.Implementation;
using BU.Services.Interface;
using Microsoft.Extensions.DependencyInjection;

namespace BU.Extensions
{
  [ExcludeFromCodeCoverage]
  public static class ServiceCollectionExtensions
  {
    public static void AddBusinessLayer(this IServiceCollection services)
    {
      services.AddHttpClient<GeminiService>();
      services.AddAutoMapper(Assembly.GetExecutingAssembly());
      services.AddTransient<IAccountService, AccountService>();
      services.AddTransient<IBookInstanceService, BookInstanceService>();
      services.AddTransient<IAuthorService, AuthorService>();
      services.AddTransient<INotificationService, NotificationService>();
      services.AddTransient<ILoanService, LoanService>();
      services.AddTransient<IGeminiService, GeminiService>();
      services.AddTransient<IGgServices, GgServices>();
      services.AddTransient<IChatGptService, ChatGptService>();
      services.AddTransient<IEpubParserService, EpubParserService>();
    }
  }
}
