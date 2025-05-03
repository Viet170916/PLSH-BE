using System.Diagnostics.CodeAnalysis;
using Common.Library;
using Microsoft.AspNetCore.Http;

namespace API.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class ResponseExtensions
    {
        public static void AddApplicationError(this HttpResponse response, string message)
        {
            response.Headers.Append(Constants.ApplicationError, message);
            // CORS
            response.Headers.Append(Constants.AccessControlExposeHeaders, Constants.ApplicationError);
        }
    }
}
