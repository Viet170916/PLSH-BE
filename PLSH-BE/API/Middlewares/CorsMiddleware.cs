using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;

namespace API.Middlewares
{
    [ExcludeFromCodeCoverage]
    public class CorsMiddleware(RequestDelegate next)
    {
        public Task Invoke(HttpContext httpContext)
        {
            httpContext.Response.Headers.Append("Access-Control-Allow-Origin", "*");
            httpContext.Response.Headers.Append("Access-Control-Allow-Credentials", "true");
            httpContext.Response.Headers.Append("Access-Control-Allow-Headers", "Content-Type, Accept");
            httpContext.Response.Headers.Append("Access-Control-Allow-Methods", "POST,GET,PUT,PATCH,DELETE,OPTIONS");
            return next(httpContext);
        }
    }
}
