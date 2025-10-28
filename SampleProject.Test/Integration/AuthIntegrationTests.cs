using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SampleProject.Application.Interfaces;
using SampleProject.Domain.Entities;
using SampleProject.Domain.Enums;
using SampleProject.Persistence.Data;
using System.Net.Http.Json;
using Xunit;

namespace SampleProject.Test.Integration
{
    /// <summary>
    /// Integration tests for authentication endpoints
    /// </summary>
    public class AuthIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
    {
        private readonly HttpClient _client;
        private readonly ApplicationDbContext _context;
        private readonly WebApplicationFactory<Program> _factory;
        private readonly IServiceScope _scope;

        public AuthIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                Environment.SetEnvironmentVariable("JWT_SECRET_KEY", "TestSecretKeyThatIsAtLeast32CharactersLong!");
                Environment.SetEnvironmentVariable("JWT_ISSUER", "SampleProject.API.Test");
                Environment.SetEnvironmentVariable("JWT_AUDIENCE", "SampleProject.Users.Test");
                ;
                builder.ConfigureServices(services =>
                {
                    // Remove the real database
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    // Add in-memory database
                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestDb");
                    });
                });
            });

            _client = _factory.CreateClient();

            // Create a scope to access scoped services
            _scope = _factory.Services.CreateScope();
            _context = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            _scope.Dispose();
            _client.Dispose();
        }

        [Fact]
        public async Task FullTokenLifecycle_LoginRefreshLogout_ShouldWorkCorrectly()
        {
            // Arrange
            await SeedTestUser();

            // 1. Login
            var loginRequest = new { Email = "test@example.com", Password = "password123" };
            var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
            loginResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            // Extract cookies
            var cookies = loginResponse.Headers.GetValues("Set-Cookie").ToList();
            var accessTokenCookie = cookies.FirstOrDefault(c => c.Contains("auth_session"));
            var refreshTokenCookie = cookies.FirstOrDefault(c => c.Contains("auth_refresh"));

            if (accessTokenCookie != null)
            {
                _client.DefaultRequestHeaders.Add("Cookie", accessTokenCookie);
            }

            // 2. Verify access with token
            var meResponse = await _client.GetAsync("/api/v1/auth/me");
            meResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            // 3. Refresh token
            string refreshToken = "dummy_token";
            if (refreshTokenCookie != null)
            {
                var cookieParts = refreshTokenCookie.Split(';')[0].Split('=');
                if (cookieParts.Length == 2)
                {
                    refreshToken = Uri.UnescapeDataString(cookieParts[1]);
                }
            }

            var refreshClient = _factory.CreateClient();
            if (refreshTokenCookie != null)
            {
                refreshClient.DefaultRequestHeaders.Add("Cookie", refreshTokenCookie);
            }

            var refreshRequest = new { RefreshToken = refreshToken };
            var refreshResponse = await refreshClient.PostAsJsonAsync("/api/v1/auth/refresh", refreshRequest);
            refreshResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            // 4. Logout
            var logoutResponse = await _client.PostAsync("/api/v1/auth/logout", null);
            logoutResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            // 5. Verify refresh token is invalidated (access token remains valid until expiry)
            var refreshAfterLogoutRequest = new { RefreshToken = refreshToken };
            var refreshAfterLogoutResponse = await refreshClient.PostAsJsonAsync("/api/v1/auth/refresh", refreshAfterLogoutRequest);
            refreshAfterLogoutResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetCurrentUser_WithoutToken_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/auth/me");

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetCurrentUser_WithValidToken_ShouldReturnUserInfo()
        {
            // Arrange
            await SeedTestUser();
            var loginRequest = new
            {
                Email = "test@example.com",
                Password = "password123"
            };

            var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
            var loginContent = await loginResponse.Content.ReadAsStringAsync();

            // Extract cookies from response
            var cookies = loginResponse.Headers.GetValues("Set-Cookie").ToList();
            var accessTokenCookie = cookies.FirstOrDefault(c => c.Contains("auth_session"));

            if (accessTokenCookie != null)
            {
                _client.DefaultRequestHeaders.Add("Cookie", accessTokenCookie);
            }

            // Act
            var response = await _client.GetAsync("/api/v1/auth/me");

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("test@example.com");
        }

        [Fact]
        public async Task Login_WithAdminUser_ShouldReturnTokensAndCorrectRole()
        {
            // Arrange - use the demo admin user that's already seeded
            var loginRequest = new
            {
                Email = "admin@example.com",
                Password = "admin123" // Use the correct demo password
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("success");
            content.Should().Contain("Login successful");

            // Check that cookies are set
            var cookies = response.Headers.GetValues("Set-Cookie").ToList();
            cookies.Should().Contain(c => c.Contains("auth_session"));
            cookies.Should().Contain(c => c.Contains("auth_refresh"));

            // Verify user info contains correct role
            var accessTokenCookie = cookies.FirstOrDefault(c => c.Contains("auth_session"));
            if (accessTokenCookie != null)
            {
                _client.DefaultRequestHeaders.Add("Cookie", accessTokenCookie);
                var userResponse = await _client.GetAsync("/api/v1/auth/me");
                var userContent = await userResponse.Content.ReadAsStringAsync();
                userContent.Should().Contain("admin@example.com");
                userContent.Should().Contain("Admin");
            }
        }

        [Fact]
        public async Task Login_WithInactiveUser_ShouldReturnBadRequest()
        {
            // Arrange - create inactive user
            var passwordService = _scope.ServiceProvider.GetRequiredService<IPasswordService>();
            var (passwordHash, passwordSalt) = passwordService.HashPassword("password123");

            var user = new UserEntity
            {
                Id = Guid.NewGuid(),
                Email = "inactive@example.com",
                FirstName = "Inactive",
                LastName = "User",
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                IsActive = false, // Inactive user
                Role = UserRole.User,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var loginRequest = new
            {
                Email = "inactive@example.com",
                Password = "password123"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ShouldReturnBadRequest()
        {
            // Arrange
            await SeedTestUser();
            var loginRequest = new
            {
                Email = "test@example.com",
                Password = "wrong_password"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Login_WithNonExistentUser_ShouldReturnBadRequest()
        {
            // Arrange
            var loginRequest = new
            {
                Email = "nonexistent@example.com",
                Password = "password123"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ShouldReturnTokens()
        {
            // Arrange
            await SeedTestUser();
            var loginRequest = new
            {
                Email = "test@example.com",
                Password = "password123"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("success");
            content.Should().Contain("Login successful");

            // Check that cookies are set
            var cookies = response.Headers.GetValues("Set-Cookie").ToList();
            cookies.Should().Contain(c => c.Contains("auth_session"));
            cookies.Should().Contain(c => c.Contains("auth_refresh"));
        }

        [Fact]
        public async Task Logout_WithoutToken_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.PostAsync("/api/v1/auth/logout", null);

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Logout_WithValidToken_ShouldReturnSuccess()
        {
            // Arrange - login first
            await SeedTestUser();
            var loginRequest = new { Email = "test@example.com", Password = "password123" };
            var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

            // Extract cookies
            var cookies = loginResponse.Headers.GetValues("Set-Cookie").ToList();
            var accessTokenCookie = cookies.FirstOrDefault(c => c.Contains("auth_session"));

            if (accessTokenCookie != null)
            {
                _client.DefaultRequestHeaders.Add("Cookie", accessTokenCookie);
            }

            // Act
            var response = await _client.PostAsync("/api/v1/auth/logout", null);

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("success");
        }

        [Fact]
        public async Task RefreshToken_WithEmptyToken_ShouldReturnBadRequest()
        {
            // Arrange
            var refreshRequest = new { RefreshToken = "" };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/auth/refresh", refreshRequest);

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task RefreshToken_WithInvalidToken_ShouldReturnBadRequest()
        {
            // Arrange
            var refreshRequest = new { RefreshToken = "invalid_token" };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/auth/refresh", refreshRequest);

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task RefreshToken_WithValidToken_ShouldReturnNewTokens()
        {
            // Arrange
            await SeedTestUser();
            var loginRequest = new
            {
                Email = "test@example.com",
                Password = "password123"
            };

            var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
            var loginContent = await loginResponse.Content.ReadAsStringAsync();

            // Extract refresh token from cookies
            var cookies = loginResponse.Headers.GetValues("Set-Cookie").ToList();
            var refreshTokenCookie = cookies.FirstOrDefault(c => c.Contains("auth_refresh"));

            // Extract the actual refresh token value from the cookie
            string refreshToken = "dummy_token"; // Default fallback
            if (refreshTokenCookie != null)
            {
                var cookieParts = refreshTokenCookie.Split(';')[0].Split('=');
                if (cookieParts.Length == 2)
                {
                    refreshToken = Uri.UnescapeDataString(cookieParts[1]);
                }
            }

            // Create a new client for the refresh request to avoid cookie conflicts
            var refreshClient = _factory.CreateClient();
            if (refreshTokenCookie != null)
            {
                refreshClient.DefaultRequestHeaders.Add("Cookie", refreshTokenCookie);
            }

            var refreshRequest = new
            {
                RefreshToken = refreshToken
            };

            // Act
            var response = await refreshClient.PostAsJsonAsync("/api/v1/auth/refresh", refreshRequest);

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("success");
        }

        [Fact]
        public async Task ValidateToken_WithoutToken_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/auth/validate");

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task ValidateToken_WithValidToken_ShouldReturnUserInfo()
        {
            // Arrange - login first
            await SeedTestUser();
            var loginRequest = new { Email = "test@example.com", Password = "password123" };
            var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

            // Extract cookies
            var cookies = loginResponse.Headers.GetValues("Set-Cookie").ToList();
            var accessTokenCookie = cookies.FirstOrDefault(c => c.Contains("auth_session"));

            if (accessTokenCookie != null)
            {
                _client.DefaultRequestHeaders.Add("Cookie", accessTokenCookie);
            }

            // Act
            var response = await _client.GetAsync("/api/v1/auth/validate");

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("success");
            content.Should().Contain("test@example.com");
        }

        private async Task SeedTestUser()
        {
            // Get password service from the existing scope
            var passwordService = _scope.ServiceProvider.GetRequiredService<IPasswordService>();

            // Hash the password properly
            var (passwordHash, passwordSalt) = passwordService.HashPassword("password123");

            var user = new UserEntity
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                IsActive = true,
                Role = UserRole.User,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }
    }
}