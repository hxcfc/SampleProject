using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace SampleProject.Installers.InstallServices.Swagger
{
    /// <summary>
    /// Global operation filter for adding common response types to all endpoints
    /// </summary>
    public class GlobalResponseTypeOperationFilter : IOperationFilter
    {
        /// <summary>
        /// Applies the filter to add global response types
        /// </summary>
        /// <param name="operation">OpenAPI operation</param>
        /// <param name="context">Operation filter context</param>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Skip if already has ProducesResponseType attributes
            var hasResponseTypes = context.MethodInfo.GetCustomAttributes<ProducesResponseTypeAttribute>().Any();
            if (hasResponseTypes)
            {
                return;
            }

            // Add common response types for all endpoints
            AddCommonResponseTypes(operation, context);
        }

        /// <summary>
        /// Adds common response types to the operation
        /// </summary>
        /// <param name="operation">OpenAPI operation</param>
        /// <param name="context">Operation filter context</param>
        private static void AddCommonResponseTypes(OpenApiOperation operation, OperationFilterContext context)
        {
            // 400 Bad Request - for validation errors
            if (!operation.Responses.ContainsKey("400"))
            {
                operation.Responses.Add("400", new OpenApiResponse
                {
                    Description = "Bad Request - Validation error or invalid input",
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/json"] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Type = "object",
                                Properties = new Dictionary<string, OpenApiSchema>
                                {
                                    ["success"] = new OpenApiSchema { Type = "boolean", Example = new Microsoft.OpenApi.Any.OpenApiBoolean(false) },
                                    ["message"] = new OpenApiSchema { Type = "string", Example = new Microsoft.OpenApi.Any.OpenApiString("Validation failed") },
                                    ["errors"] = new OpenApiSchema 
                                    { 
                                        Type = "array", 
                                        Items = new OpenApiSchema { Type = "string" },
                                        Example = new Microsoft.OpenApi.Any.OpenApiArray
                                        {
                                            new Microsoft.OpenApi.Any.OpenApiString("Field is required")
                                        }
                                    }
                                }
                            }
                        }
                    }
                });
            }

            // 401 Unauthorized - for authentication errors
            if (!operation.Responses.ContainsKey("401"))
            {
                operation.Responses.Add("401", new OpenApiResponse
                {
                    Description = "Unauthorized - Authentication required or token invalid",
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/json"] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Type = "object",
                                Properties = new Dictionary<string, OpenApiSchema>
                                {
                                    ["success"] = new OpenApiSchema { Type = "boolean", Example = new Microsoft.OpenApi.Any.OpenApiBoolean(false) },
                                    ["message"] = new OpenApiSchema { Type = "string", Example = new Microsoft.OpenApi.Any.OpenApiString("Unauthorized") }
                                }
                            }
                        }
                    }
                });
            }

            // 403 Forbidden - for authorization errors
            if (!operation.Responses.ContainsKey("403"))
            {
                operation.Responses.Add("403", new OpenApiResponse
                {
                    Description = "Forbidden - Insufficient permissions",
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/json"] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Type = "object",
                                Properties = new Dictionary<string, OpenApiSchema>
                                {
                                    ["success"] = new OpenApiSchema { Type = "boolean", Example = new Microsoft.OpenApi.Any.OpenApiBoolean(false) },
                                    ["message"] = new OpenApiSchema { Type = "string", Example = new Microsoft.OpenApi.Any.OpenApiString("Forbidden") }
                                }
                            }
                        }
                    }
                });
            }

            // 500 Internal Server Error - for server errors
            if (!operation.Responses.ContainsKey("500"))
            {
                operation.Responses.Add("500", new OpenApiResponse
                {
                    Description = "Internal Server Error - Unexpected server error",
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/json"] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Type = "object",
                                Properties = new Dictionary<string, OpenApiSchema>
                                {
                                    ["success"] = new OpenApiSchema { Type = "boolean", Example = new Microsoft.OpenApi.Any.OpenApiBoolean(false) },
                                    ["message"] = new OpenApiSchema { Type = "string", Example = new Microsoft.OpenApi.Any.OpenApiString("Internal server error") }
                                }
                            }
                        }
                    }
                });
            }
        }
    }
}
