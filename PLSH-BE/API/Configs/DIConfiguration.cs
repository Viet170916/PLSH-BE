using System.Diagnostics.CodeAnalysis;
using Data.Repository.Implementation;
using Data.Repository.Interfaces;
using Data.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;

namespace API.Configs;

[ExcludeFromCodeCoverage]
public static class DiConfiguration
{
  public static void Initialize(IServiceCollection services)
  {
    services.AddTransient(typeof(IGenericRepository<>), typeof(GenericRepository<>));
    services.AddTransient<IUnitOfWork, UnitOfWork>();
    services.AddTransient<IHttpClientWrapper, HttpClientWrapper>();
  }
}