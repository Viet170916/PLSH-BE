using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;

namespace API.Middlewares
{
    [ExcludeFromCodeCoverage]
    public static class CorsMiddlewareExtensions
    {
        public static IApplicationBuilder UseCorsMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CorsMiddleware>();
        }
    }
}
