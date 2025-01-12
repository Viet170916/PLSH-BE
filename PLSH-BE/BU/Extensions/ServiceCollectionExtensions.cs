using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace BU.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class ServiceCollectionExtensions
    {
        public static void AddBusinessLayer(this IServiceCollection services)
        {
            // add automapper auto binding
            // services.AddAutoMapper(Assembly.GetExecutingAssembly());

          
            // services.AddTransient<IAPICLockService, APICLockService>();
            // // Get options from app settings
            // var configuration = ServiceLocator.Current.GetInstance<Microsoft.Extensions.Configuration.IConfiguration>();
            //
            // var appSettingOptions = configuration.GetSection(nameof(AppConfigSection));
            // services.Configure<AppConfigSection>(options =>
            // {
            //     options.DebugAccount = appSettingOptions[nameof(AppConfigSection.DebugAccount)];
            //     options.DomainCrsApiUrl = appSettingOptions[nameof(AppConfigSection.DomainCrsApiUrl)];
            //     options.DomainBrpUrl = appSettingOptions[nameof(AppConfigSection.DomainBrpUrl)];
            //     options.DomainCrsUrl = appSettingOptions[nameof(AppConfigSection.DomainCrsUrl)];
            //     options.TimeToUnLock = int.Parse(appSettingOptions[nameof(AppConfigSection.TimeToUnLock)]);
            // });
            // // Read email settings
            // services.Configure<EmailConfig>(configuration.GetSection("EmailConfiguration"));
            // services.AddLockBusinessLayer();
        }
    }
}
