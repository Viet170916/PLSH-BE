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
      // add automapper auto binding
      services.AddAutoMapper(Assembly.GetExecutingAssembly());
      services.AddTransient<IAccountService, AccountService>();
      services.AddTransient<IBookInstanceService, BookInstanceService>();
      services.AddTransient<IAuthorService, AuthorService>();
    }
  }
}