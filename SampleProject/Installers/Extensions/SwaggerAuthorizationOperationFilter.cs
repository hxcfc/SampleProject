using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;

namespace SampleProject.Installers.Extensions
{
    /// <summary>
    /// Swagger operation filter that automatically adds authorization information to endpoint descriptions
    /// based on [Authorize] attributes
    /// </summary>
    public class SwaggerAuthorizationOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var methodInfo = context.MethodInfo;
            var controllerType = context.MethodInfo.DeclaringType;

            // Get authorization attributes from method and controller
            var methodAuthorize = methodInfo.GetCustomAttribute<AuthorizeAttribute>();
            var controllerAuthorize = controllerType?.GetCustomAttribute<AuthorizeAttribute>();

            // Use method-level authorization if available, otherwise use controller-level
            var authorizeAttribute = methodAuthorize ?? controllerAuthorize;

            if (authorizeAttribute != null)
            {
                var roles = authorizeAttribute.Roles;
                var policy = authorizeAttribute.Policy;

                // Build access description
                var accessDescription = BuildAccessDescription(roles, policy);

                // Add to operation description
                if (!string.IsNullOrEmpty(operation.Description))
                {
                    operation.Description += $"\n\n**Access:** {accessDescription}";
                }
                else
                {
                    operation.Description = $"**Access:** {accessDescription}";
                }

                // Add security requirement
                AddSecurityRequirement(operation, roles, policy);
            }
            else
            {
                // No authorization required
                var accessDescription = "Anonymous (no authentication required)";
                
                if (!string.IsNullOrEmpty(operation.Description))
                {
                    operation.Description += $"\n\n**Access:** {accessDescription}";
                }
                else
                {
                    operation.Description = $"**Access:** {accessDescription}";
                }
            }
        }

        private static string BuildAccessDescription(string? roles, string? policy)
        {
            if (!string.IsNullOrEmpty(policy))
            {
                return $"Policy: {policy}";
            }

            if (!string.IsNullOrEmpty(roles))
            {
                var roleList = roles.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(r => r.Trim())
                    .ToList();

                if (roleList.Count == 1)
                {
                    return $"{roleList[0]} only";
                }
                else if (roleList.Count == 2)
                {
                    // Special handling for User,Admin combination
                    if (roleList.Contains("User") && roleList.Contains("Admin"))
                    {
                        return "User (own data only) or Admin (any data)";
                    }
                    return $"{roleList[0]} or {roleList[1]}";
                }
                else
                {
                    return string.Join(", ", roleList.Take(roleList.Count - 1)) + $" or {roleList.Last()}";
                }
            }

            return "Authentication required";
        }

        private static void AddSecurityRequirement(OpenApiOperation operation, string? roles, string? policy)
        {
            if (operation.Security == null)
            {
                operation.Security = new List<OpenApiSecurityRequirement>();
            }

            // Add JWT Bearer security requirement
            operation.Security.Add(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] { }
                }
            });
        }
    }
}
