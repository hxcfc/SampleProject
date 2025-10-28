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
    /// Integration tests for users endpoints
    /// </summary>
    public class UsersIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
    {
        private readonly HttpClient _client;
        private readonly ApplicationDbContext _context;
        private readonly WebApplicationFactory<Program> _factory;
        private readonly IServiceScope _scope;

        public UsersIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    Environment.SetEnvironmentVariable("JWT_SECRET_KEY", "TestSecretKeyThatIsAtLeast32CharactersLong!");
                    Environment.SetEnvironmentVariable("JWT_ISSUER", "SampleProject.API.Test");
                    Environment.SetEnvironmentVariable("JWT_AUDIENCE", "SampleProject.Users.Test");
                    ;
                    // Remove the real database
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    // Add in-memory database - use same name as application
                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("SampleProjectDb");
                    });
                });
            });

            _client = _factory.CreateClient();

            // Create a scope to access scoped services
            _scope = _factory.Services.CreateScope();
            _context = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        }

        [Fact]
        public async Task ChangeMyPassword_WithoutToken_ShouldReturnUnauthorized()
        {
            // Arrange
            var changePasswordRequest = new
            {
                CurrentPassword = "password123",
                NewPassword = "newpassword123"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/users/me/change-password", changePasswordRequest);

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task ChangeMyPassword_WithValidToken_ShouldReturnSuccess()
        {
            // Arrange
            await SeedTestUser();
            await LoginAsUser();
            var changePasswordRequest = new
            {
                CurrentPassword = "password123",
                NewPassword = "newpassword123"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/users/me/change-password", changePasswordRequest);

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task ChangeMyPassword_WithWrongCurrentPassword_ShouldReturnBadRequest()
        {
            // Arrange
            await SeedTestUser();
            await LoginAsUser();
            var changePasswordRequest = new
            {
                CurrentPassword = "wrongpassword",
                NewPassword = "newpassword123"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/users/me/change-password", changePasswordRequest);

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateUser_WithDuplicateEmail_ShouldReturnBadRequest()
        {
            // Arrange - create first user
            await SeedTestUser();
            var createUserRequest = new
            {
                Email = "test@example.com", // Same email as seeded user
                Password = "password123",
                FirstName = "New",
                LastName = "User"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/users", createUserRequest);

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateUser_WithInvalidEmail_ShouldReturnBadRequest()
        {
            // Arrange
            var createUserRequest = new
            {
                Email = "not-an-email-at-all",
                Password = "password123",
                FirstName = "New",
                LastName = "User"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/users", createUserRequest);

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeEmpty();
            content.Should().Contain("Email must be a valid email address");
        }

        [Fact]
        public async Task CreateUser_WithShortPassword_ShouldReturnBadRequest()
        {
            // Arrange
            var createUserRequest = new
            {
                Email = "user@example.com",
                Password = "123",
                FirstName = "New",
                LastName = "User"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/users", createUserRequest);

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateUser_WithValidData_ShouldReturnCreatedUser()
        {
            // Arrange
            var createUserRequest = new
            {
                Email = "newuser@example.com",
                Password = "password123",
                FirstName = "New",
                LastName = "User"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/users", createUserRequest);

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("newuser@example.com");
            content.Should().Contain("New");
            content.Should().Contain("User");
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            _scope.Dispose();
            _client.Dispose();
        }

        [Fact]
        public async Task GetUserById_WithAdminToken_ShouldReturnAnyUserInfo()
        {
            // Arrange
            await SeedTestUser();
            var userId = await LoginAndGetUserId();
            await LoginAsAdmin();

            // Act
            var response = await _client.GetAsync($"/api/v1/users/{userId}");

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("test@example.com");
        }

        [Fact]
        public async Task GetUserById_WithoutToken_ShouldReturnUnauthorized()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/api/v1/users/{userId}");

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetUserById_WithValidToken_ShouldReturnUserInfo()
        {
            // Arrange
            await SeedTestUser();
            var userId = await LoginAndGetUserId();

            // Act
            var response = await _client.GetAsync($"/api/v1/users/{userId}");

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("test@example.com");
        }

        [Fact]
        public async Task GetUsersList_WithAdminToken_ShouldReturnUsersList()
        {
            // Arrange
            await SeedTestUser();
            await LoginAsAdmin();

            // Act
            var response = await _client.GetAsync("/api/v1/users");

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("test@example.com");
        }

        [Fact]
        public async Task GetUsersList_WithoutToken_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/users");

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetUsersList_WithUserToken_ShouldReturnForbidden()
        {
            // Arrange
            await SeedTestUser();
            await LoginAsUser();

            // Act
            var response = await _client.GetAsync("/api/v1/users");

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task UpdateMyProfile_WithoutToken_ShouldReturnUnauthorized()
        {
            // Arrange
            var updateRequest = new
            {
                FirstName = "Updated",
                LastName = "Name"
            };

            // Act
            var response = await _client.PutAsJsonAsync("/api/v1/users/me", updateRequest);

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task UpdateMyProfile_WithValidToken_ShouldReturnUpdatedUser()
        {
            // Arrange
            await SeedTestUser();
            await LoginAsUser();
            var updateRequest = new
            {
                FirstName = "Updated",
                LastName = "Name"
            };

            // Act
            var response = await _client.PutAsJsonAsync("/api/v1/users/me", updateRequest);

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Updated");
            content.Should().Contain("Name");
        }

        [Fact]
        public async Task UpdateUser_WithAdminToken_ShouldReturnUpdatedUser()
        {
            // Arrange
            await SeedTestUser();
            var userId = await LoginAndGetUserId();
            await LoginAsAdmin();
            var updateRequest = new
            {
                FirstName = "AdminUpdated",
                LastName = "Name",
                IsActive = true
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/v1/users/{userId}", updateRequest);

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("AdminUpdated");
        }

        [Fact]
        public async Task UpdateUser_WithUserToken_ShouldReturnForbidden()
        {
            // Arrange
            await SeedTestUser();
            var userId = await LoginAndGetUserId();
            await LoginAsUser();
            var updateRequest = new
            {
                FirstName = "Updated",
                LastName = "Name"
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/v1/users/{userId}", updateRequest);

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
        }

        private async Task<Guid> LoginAndGetUserId()
        {
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

            // Get user ID from the seeded user
            var user = _context.Users.FirstOrDefault(u => u.Email == "test@example.com");
            return user?.Id ?? Guid.Empty;
        }

        private async Task LoginAsAdmin()
        {
            var loginRequest = new
            {
                Email = "admin@example.com",
                Password = "admin123"
            };

            var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
            var cookies = loginResponse.Headers.GetValues("Set-Cookie").ToList();
            var accessTokenCookie = cookies.FirstOrDefault(c => c.Contains("auth_session"));

            if (accessTokenCookie != null)
            {
                _client.DefaultRequestHeaders.Add("Cookie", accessTokenCookie);
            }
        }

        private async Task LoginAsUser()
        {
            var loginRequest = new
            {
                Email = "test@example.com",
                Password = "password123"
            };

            var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
            var cookies = loginResponse.Headers.GetValues("Set-Cookie").ToList();
            var accessTokenCookie = cookies.FirstOrDefault(c => c.Contains("auth_session"));

            if (accessTokenCookie != null)
            {
                _client.DefaultRequestHeaders.Add("Cookie", accessTokenCookie);
            }
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