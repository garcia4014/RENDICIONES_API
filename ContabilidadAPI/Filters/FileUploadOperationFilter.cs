using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace ContabilidadAPI.Filters
{
    /// <summary>
    /// Filtro para manejar file uploads en Swagger
    /// </summary>
    public class FileUploadOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var fileUploadMime = "multipart/form-data";
            
            if (operation.RequestBody == null || 
                !operation.RequestBody.Content.Any(x => x.Key.Equals(fileUploadMime, StringComparison.InvariantCultureIgnoreCase)))
                return;

            var fileParams = context.MethodInfo.GetParameters().Where(p => p.ParameterType == typeof(IFormFile));
            if (!fileParams.Any())
                return;

            operation.RequestBody.Content[fileUploadMime].Schema.Properties =
                operation.RequestBody.Content[fileUploadMime].Schema.Properties
                    .Where(p => p.Key != "file")
                    .ToDictionary(p => p.Key, p => p.Value);

            var fileProperty = new OpenApiSchema()
            {
                Type = "string",
                Format = "binary"
            };

            operation.RequestBody.Content[fileUploadMime].Schema.Properties.Add("file", fileProperty);
        }
    }
}