using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;

namespace API.Common
{
  [ExcludeFromCodeCoverage]
  public class SwaggerFileOperationFilter : IOperationFilter
  {
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
      const string fileUploadMime = "multipart/form-data";
      if (operation?.RequestBody == null || !operation.RequestBody.Content.Any(x => x.Key.Equals(fileUploadMime, StringComparison.InvariantCultureIgnoreCase)))
      {
        return;
      }

      var fileParams = context?.MethodInfo.GetParameters().Where(p => p.ParameterType == typeof(IFormFile));
      operation.RequestBody.Content[fileUploadMime].Schema.Properties =
        fileParams.ToDictionary(k => k.Name, v => new OpenApiSchema()
        {
          Type = "string",
          Format = "binary",
                    
        });
    }
  }
}