using Microsoft.AspNetCore.Mvc.ApiExplorer;
using System.Reflection;
using Common.Options;
using System.Linq;
using SampleProject.Installers.Extensions;
using System.Text;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Any;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Common.Shared;

namespace SampleProject.Installers.InstallServices.Swagger
{
    /// <summary>
    /// Installer for Swagger services
    /// </summary>
    public class SwaggerInstaller : IInstaller
    {
        /// <summary>
        /// Gets the installation order priority
        /// </summary>
        public int Order => 9;

        /// <summary>
        /// Configures Swagger services in the web application
        /// </summary>
        /// <param name="app">Web application</param>
        public void ConfigureServices(WebApplication app)
        {
            var swaggerOptions = app.Configuration.GetSection("Swagger").Get<SwaggerOptions>() ?? new SwaggerOptions();

            // Only enable Swagger if configured to do so
            if (!swaggerOptions.Enabled)
            {
                return;
            }

            // Enable Swagger only in Development or if explicitly enabled
            if (app.Environment.IsDevelopment() || swaggerOptions.Enabled)
            {
                app.UseSwagger();
                
                // Add Swagger UI authentication middleware if enabled
                if (swaggerOptions.Auth.Enabled)
                {
                    app.Use(async (context, next) =>
                    {
                        try
                        {
                            if (context.Request.Path.StartsWithSegments("/swagger"))
                            {
                                var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                                if (authHeader != null && authHeader.StartsWith("Basic "))
                                {
                                    var encodedUsernamePassword = authHeader.Substring("Basic ".Length).Trim();
                                    var decodedUsernamePassword = Encoding.UTF8.GetString(Convert.FromBase64String(encodedUsernamePassword));
                                    var credentials = decodedUsernamePassword.Split(':', 2);
                                    
                                    if (credentials.Length == 2)
                                    {
                                        var username = credentials[0];
                                        var password = credentials[1];

                                        if (username == swaggerOptions.Auth.Username && password == swaggerOptions.Auth.Password)
                                        {
                                            await next();
                                            return;
                                        }
                                    }
                                }

                                context.Response.Headers["WWW-Authenticate"] = "Basic realm=\"Swagger UI\"";
                                context.Response.StatusCode = 401;
                                await context.Response.WriteAsync("Unauthorized", context.RequestAborted);
                                return;
                            }

                            await next();
                        }
                        catch (OperationCanceledException)
                        {
                            // Request was canceled, this is normal behavior
                            // Don't log this as an error
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "Error in Swagger authentication middleware");
                            // Re-throw the exception to let the global exception handler deal with it
                            throw;
                        }
                    });
                }
                
                app.UseSwaggerUI(options =>
                {
                    // Configure Swagger UI with dynamic API versioning
                    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

                    foreach (var description in provider.ApiVersionDescriptions)
                    {
                        // Main API documentation
                        options.SwaggerEndpoint(
                            $"/swagger/{description.GroupName}/swagger.json",
                            $"{swaggerOptions.Title} {description.ApiVersion}");

                        // Diagnostic API documentation
                        options.SwaggerEndpoint(
                            $"/swagger/{description.GroupName}-Diagnostic/swagger.json",
                            $"{swaggerOptions.Title} Diagnostic {description.ApiVersion}");
                    }

                    options.RoutePrefix = "swagger";
                    options.DisplayRequestDuration();
                    options.EnableDeepLinking();
                    options.EnableFilter();
                    options.EnableTryItOutByDefault();
                    options.EnableValidator();
                    options.ShowExtensions();
                    options.ShowCommonExtensions();
                    options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
                    options.DefaultModelsExpandDepth(1);
                    options.DefaultModelRendering(Swashbuckle.AspNetCore.SwaggerUI.ModelRendering.Model);
                });

                Log.Information(StringMessages.SwaggerUIEnabledAt);
            }
        }

        /// <summary>
        /// Installs Swagger services
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Configuration</param>
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            var swaggerOptions = configuration.GetSection("Swagger").Get<SwaggerOptions>() ?? new SwaggerOptions();

            // Only enable Swagger if configured to do so
            if (!swaggerOptions.Enabled)
            {
                Log.Information(StringMessages.SwaggerDisabledInConfiguration);
                return;
            }

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                // Configure API versioning for Swagger
                var provider = services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();

                foreach (var description in provider.ApiVersionDescriptions)
                {
                    // Build description with demo credentials if enabled
                    var apiDescription = swaggerOptions.Description;
                    if (swaggerOptions.DemoCredentials.Enabled)
                    {
                        apiDescription += "\n\n**Demo Credentials:**";
                        
                        // Add individual users if configured
                        if (swaggerOptions.DemoCredentials.Users.Any())
                        {
                            foreach (var user in swaggerOptions.DemoCredentials.Users)
                            {
                                apiDescription += $"\n- **{user.Role}:** `{user.Username}` / `{user.Password}`";
                            }
                        }
                        else
                        {
                            // Fallback to single user configuration
                            apiDescription += $"\n- Username: `{swaggerOptions.DemoCredentials.Username}`\n- Password: `{swaggerOptions.DemoCredentials.Password}`";
                        }
                    }

                    // Main API documentation
                    options.SwaggerDoc(description.GroupName, new OpenApiInfo
                    {
                        Title = $"{swaggerOptions.Title} {description.ApiVersion}",
                        Version = description.ApiVersion.ToString(),
                        Description = apiDescription + "\n\n**Available Controllers:**\n- **Auth** - Authentication endpoints\n- **Users** - User management\n- **Helpers** - Helper utility endpoints",
                        Contact = new OpenApiContact
                        {
                            Name = swaggerOptions.ContactName,
                            Email = swaggerOptions.ContactEmail,
                            Url = new Uri(swaggerOptions.ContactUrl)
                        },
                        License = new OpenApiLicense
                        {
                            Name = swaggerOptions.LicenseName,
                            Url = new Uri(swaggerOptions.LicenseUrl)
                        },
                        TermsOfService = new Uri(swaggerOptions.TermsOfServiceUrl)
                    });

                    // Diagnostic API documentation
                    options.SwaggerDoc($"{description.GroupName}-Diagnostic", new OpenApiInfo
                    {
                        Title = $"{swaggerOptions.Title} Diagnostic {description.ApiVersion}",
                        Version = description.ApiVersion.ToString(),
                        Description = "Diagnostic and monitoring endpoints for system health, version information, and debugging.\n\n**Available Controllers:**\n- **Common** - System utilities and time\n- **Health** - Health check endpoints\n- **VersionInfo** - Application version information",
                        Contact = new OpenApiContact
                        {
                            Name = swaggerOptions.ContactName,
                            Email = swaggerOptions.ContactEmail,
                            Url = new Uri(swaggerOptions.ContactUrl)
                        },
                        License = new OpenApiLicense
                        {
                            Name = swaggerOptions.LicenseName,
                            Url = new Uri(swaggerOptions.LicenseUrl)
                        },
                        TermsOfService = new Uri(swaggerOptions.TermsOfServiceUrl)
                    });
                }

                // Add JWT authentication to Swagger
                if (swaggerOptions.EnableAuthentication)
                {
                    options.AddSecurityDefinition(StringMessages.SwaggerBearerScheme, new OpenApiSecurityScheme
                    {
                        Description = StringMessages.SwaggerBearerDescription,
                        Name = StringMessages.SwaggerAuthorizationHeader,
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = StringMessages.SwaggerBearerScheme
                    });

                    options.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = StringMessages.SwaggerBearerScheme
                                }
                            },
                            Array.Empty<string>()
                        }
                    });
                }

                // Configure document groups based on ApiExplorerSettings GroupName
                options.DocInclusionPredicate((documentName, apiDesc) =>
                {
                    // Get the controller action descriptor
                    var controllerActionDescriptor = apiDesc.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor;

                    if (controllerActionDescriptor?.ControllerTypeInfo != null)
                    {
                        // Check ApiExplorerSettings attribute on the controller
                        var apiExplorerSettings = controllerActionDescriptor.ControllerTypeInfo
                            .GetCustomAttributes(typeof(ApiExplorerSettingsAttribute), false)
                            .FirstOrDefault() as ApiExplorerSettingsAttribute;

                        if (apiExplorerSettings != null)
                        {
                            // Use the GroupName from ApiExplorerSettings
                            return documentName == apiExplorerSettings.GroupName;
                        }

                        // Fallback: check namespace for backward compatibility
                        var namespaceName = controllerActionDescriptor.ControllerTypeInfo.Namespace;
                        if (namespaceName?.Contains("Diagnostic") == true)
                        {
                            // Use dynamic diagnostic group name based on API version
                            var apiVersion = apiDesc.ActionDescriptor.EndpointMetadata
                                .OfType<ApiVersionAttribute>()
                                .FirstOrDefault()?.Versions.FirstOrDefault();

                            if (apiVersion != null)
                            {
                                var diagnosticGroupName = $"v{apiVersion.MajorVersion}.{apiVersion.MinorVersion}-Diagnostic";
                                return documentName == diagnosticGroupName;
                            }
                        }
                    }

                    // Default to main API group for controllers without explicit grouping
                    // Use dynamic group name based on API version
                    var defaultApiVersion = apiDesc.ActionDescriptor.EndpointMetadata
                        .OfType<ApiVersionAttribute>()
                        .FirstOrDefault()?.Versions.FirstOrDefault();

                    if (defaultApiVersion != null)
                    {
                        var mainGroupName = $"v{defaultApiVersion.MajorVersion}.{defaultApiVersion.MinorVersion}";
                        return documentName == mainGroupName;
                    }

                    return false;
                });

                // Include XML comments if enabled
                if (swaggerOptions.EnableXmlComments)
                {
                    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                    if (File.Exists(xmlPath))
                    {
                        options.IncludeXmlComments(xmlPath);
                    }
                }

                // Configure additional options
                options.EnableAnnotations();

                // Add global response types for common HTTP status codes
                options.MapType<object>(() => new OpenApiSchema
                {
                    Type = "object",
                    Properties = new Dictionary<string, OpenApiSchema>
                    {
                        ["success"] = new OpenApiSchema { Type = "boolean" },
                        ["message"] = new OpenApiSchema { Type = "string" },
                        ["data"] = new OpenApiSchema { Type = "object" },
                        ["errors"] = new OpenApiSchema
                        {
                            Type = "array",
                            Items = new OpenApiSchema { Type = "string" }
                        }
                    }
                });

                // Add global response types
                options.OperationFilter<GlobalResponseTypeOperationFilter>();

                // Add authorization information filter
                options.OperationFilter<SwaggerAuthorizationOperationFilter>();

                // Add custom filter if enabled
                if (swaggerOptions.EnableFilter)
                {
                    // You can add custom filters here if needed
                }

                // Configure servers
                if (swaggerOptions.Servers.Any())
                {
                    // Note: Servers configuration is handled in SwaggerUI, not SwaggerGen
                }
            });

            Log.Information(StringMessages.SwaggerServicesConfiguredSuccessfully);
        }
    }
}