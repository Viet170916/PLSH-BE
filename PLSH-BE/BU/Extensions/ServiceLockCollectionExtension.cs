using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace BU.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class ServiceLockCollectionExtension
    {
        public static void AddLockBusinessLayer(this IServiceCollection services)
        {
            
            // services.AddTransient<IWipFeeService, WipFeeService>();
        }
    }
}
