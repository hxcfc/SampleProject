using Microsoft.EntityFrameworkCore;
using Common.Shared.Interfaces;
using SampleProject.Application.Interfaces;
using SampleProject.Domain.Entities;
using SampleProject.Domain.Enums;
using SampleProject.Persistence.Data;
using Common.Options;

namespace SampleProject.Infrastructure.Implementations
{
    /// <summary>
    /// Service for seeding initial data into the database
    /// </summary>
    public class DatabaseSeeder : IDatabaseSeeder
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordService _passwordService;
        private readonly ILogger<DatabaseSeeder> _logger;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseSeeder"/> class
        /// </summary>
        /// <param name="context">Database context</param>
        /// <param name="passwordService">Password hashing service</param>
        /// <param name="logger">Logger instance</param>
        /// <param name="configuration">Configuration instance</param>
        public DatabaseSeeder(
            ApplicationDbContext context, 
            IPasswordService passwordService, 
            ILogger<DatabaseSeeder> logger,
            IConfiguration configuration)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <inheritdoc />
        public async Task SeedAsync()
        {
            try
            {
                _logger.LogInformation("Starting database seeding");

                // Check if data already exists
                if (await _context.Users.AnyAsync())
                {
                    _logger.LogInformation("Database already contains data, skipping seed");
                    return;
                }

                // Get Swagger configuration
                var swaggerOptions = _configuration.GetSection("Swagger").Get<SwaggerOptions>();
                if (swaggerOptions?.DemoCredentials?.Enabled != true || swaggerOptions.DemoCredentials.Users == null)
                {
                    _logger.LogWarning("Demo credentials not enabled or not configured, skipping user seeding");
                    return;
                }

                _logger.LogInformation("Creating {UserCount} demo users from configuration", swaggerOptions.DemoCredentials.Users.Count);

                // Create users from configuration
                foreach (var demoUser in swaggerOptions.DemoCredentials.Users)
                {
                    try
                    {
                        // Parse role from string to enum
                        var userRole = ParseRoleFromString(demoUser.Role);
                        
                        // Hash password
                        var (passwordHash, passwordSalt) = _passwordService.HashPassword(demoUser.Password);
                        
                        // Extract first and last name from email (before @)
                        var emailParts = demoUser.Username.Split('@');
                        var nameParts = emailParts[0].Split('.');
                        var firstName = nameParts.Length > 0 ? CapitalizeFirstLetter(nameParts[0]) : "Demo";
                        var lastName = nameParts.Length > 1 ? CapitalizeFirstLetter(nameParts[1]) : "User";

                        var user = new UserEntity
                        {
                            Id = Guid.NewGuid(),
                            Email = demoUser.Username,
                            FirstName = firstName,
                            LastName = lastName,
                            PasswordHash = passwordHash,
                            PasswordSalt = passwordSalt,
                            IsActive = true,
                            IsEmailVerified = true,
                            Role = userRole,
                            CreatedAt = DateTime.UtcNow
                        };

                        _context.Users.Add(user);
                        _logger.LogInformation("Created demo user: {Email} with role: {Role}", demoUser.Username, demoUser.Role);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to create demo user: {Username}", demoUser.Username);
                        // Continue with other users even if one fails
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Database seeding completed successfully with {UserCount} users", swaggerOptions.DemoCredentials.Users.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during database seeding");
                throw;
            }
        }

        /// <summary>
        /// Parses role string to UserRole enum
        /// </summary>
        /// <param name="Roletring">Role as string</param>
        /// <returns>UserRole enum value</returns>
        private static UserRole ParseRoleFromString(string Roletring)
        {
            return Roletring?.ToLowerInvariant() switch
            {
                "admin" => UserRole.Admin,
                "user" => UserRole.User,
                _ => UserRole.User // Default to User role if unknown
            };
        }

        /// <summary>
        /// Capitalizes the first letter of a string
        /// </summary>
        /// <param name="input">Input string</param>
        /// <returns>String with first letter capitalized</returns>
        private static string CapitalizeFirstLetter(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;
            
            return char.ToUpperInvariant(input[0]) + input.Substring(1).ToLowerInvariant();
        }
    }
}
