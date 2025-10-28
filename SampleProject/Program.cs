using SampleProject.Installers.Extensions;
using SampleProject.Installers.InstallServices.Logging;
using SampleProject.Middleware;
using Common.Shared;
using SampleProject.Application;

namespace SampleProject
{
    /// <summary>
    /// Main application entry point
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Application entry point
        /// </summary>
        /// <param name="args">Command line arguments</param>
        public static void Main(string[] args)
        {
            try
            {
                // Create and configure application
                var app = CreateApplication(args);
                
                // Display startup information
                DisplayStartupInformation(app);
                
                // Run application
                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, StringMessages.ApplicationShutdownMessage());
                throw;
            }
            finally
            {
                Log.Information(StringMessages.ApplicationShutdownMessage());
                Log.CloseAndFlush();
            }
        }

        /// <summary>
        /// Creates and configures the web application
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <returns>Configured web application</returns>
        private static WebApplication CreateApplication(string[] args)
        {
            // Create application builder
            var builder = WebApplication.CreateBuilder(args);

            // Configure logging first
            ConfigureLogging(builder);

            // Configure services
            ConfigureServices(builder);

            // Build application
            var app = builder.Build();

            // Configure middleware pipeline
            ConfigureMiddlewarePipeline(app);

            // Configure application services
            ConfigureApplicationServices(app);

            return app;
        }

        /// <summary>
        /// Configures logging for the application
        /// </summary>
        /// <param name="builder">Web application builder</param>
        private static void ConfigureLogging(WebApplicationBuilder builder)
        {
            LoggingInstaller.ConfigureSerilog(builder, builder.Configuration);
            Log.Information(StringMessages.ApplicationStartupMessage());
        }

        /// <summary>
        /// Configures application services
        /// </summary>
        /// <param name="builder">Web application builder</param>
        private static void ConfigureServices(WebApplicationBuilder builder)
        {
            // Configure CORS
            builder.Services.AddCors(options =>
            {
                if (builder.Environment.IsDevelopment())
                {
                    // Development: Allow all origins for testing
                    options.AddDefaultPolicy(policy =>
                    {
                        policy.AllowAnyOrigin()
                              .AllowAnyMethod()
                              .AllowAnyHeader();
                    });
                    
                    Log.Information("CORS configured for Development - allowing all origins");
                }
                else
                {
                    // Production: Restrict to specific origins
                    options.AddDefaultPolicy(policy =>
                    {
                        var allowedOrigins = builder.Configuration
                            .GetSection("Cors:AllowedOrigins")
                            .Get<string[]>() ?? new[] { "https://yourdomain.com" };
                        
                        policy.WithOrigins(allowedOrigins)
                              .AllowAnyMethod()
                              .AllowAnyHeader()
                              .AllowCredentials();
                    });
                    
                    Log.Information("CORS configured for Production with restricted origins");
                }
            });

            // Add controllers
            builder.Services.AddControllers();

            // Install all services from assembly (includes Application, Infrastructure, and other services)
            builder.Services.InstallServicesInAssembly(builder.Configuration);
        }

        /// <summary>
        /// Configures the middleware pipeline
        /// </summary>
        /// <param name="app">Web application</param>
        private static void ConfigureMiddlewarePipeline(WebApplication app)
        {
            // CORS must be before other middleware
            app.UseCors();
            
            // Security headers
            app.UseSecurityHeaders();
            
            // HTTPS redirection (only in non-development)
            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }
            
            // Request logging
            app.UseRequestLogging();

            // Metrics collection
            app.UseMetrics();

            // Correlation ID before anything that logs/authenticates
            app.UseCorrelationId();

            // Rate limiting before auth
            app.UseRateLimiting();

            // Routing must be before authentication
            app.UseRouting();
            
            // JWT Token middleware - extract token from cookies/header and set HttpContext.User
            app.UseMiddleware<SampleProject.Middleware.JwtTokenMiddleware>();
            
            // Authentication and Authorization
            app.UseAuthentication();
            app.UseAuthorization();
        }

        /// <summary>
        /// Configures application-specific services
        /// </summary>
        /// <param name="app">Web application</param>
        private static void ConfigureApplicationServices(WebApplication app)
        {
            // Map controllers first
            app.MapControllers();

            // Configure services in assembly (includes Swagger)
            app.ConfigureServicesInAssembly();
            
            // Exception handling must be last to catch all exceptions from downstream middleware
            app.UseExceptionHandling();

            // Database seeding is handled by DatabaseSeedHostedService

            // Static files disabled - not using wwwroot
        }


        /// <summary>
        /// Displays startup information using StartupDisplay
        /// </summary>
        /// <param name="app">Web application</param>
        private static void DisplayStartupInformation(WebApplication app)
        {
            // Get logger from DI container
            using var scope = app.Services.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            // Display startup banner (includes logo and application info)
            StartupDisplay.DisplayStartupBanner(logger);

            // Display environment-specific information
            StartupDisplay.DisplayEnvironmentInfo(logger, app.Environment.EnvironmentName);

            // Log startup completion
            Log.Information(StringMessages.ApplicationStarted);
            Log.Information(StringMessages.ApplicationIsListeningOn, string.Join(", ", app.Urls));
        }
    }
}