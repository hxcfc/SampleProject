using SampleProject.Infrastructure.Implementations;
using SampleProject.Domain.Enums;
using SampleProject.Domain.Responses;
using Common.Options;
using Microsoft.Extensions.Options;
using FluentAssertions;
using Xunit;
using System.IdentityModel.Tokens.Jwt;

namespace SampleProject.Test.Unit.Infrastructure
{
    /// <summary>
    /// Unit tests for JwtService
    /// </summary>
    public class JwtServiceTests
    {
        private readonly JwtService _jwtService;
        private readonly JwtOptions _jwtOptions;

        public JwtServiceTests()
        {
            _jwtOptions = new JwtOptions
            {
                SecretKey = "YourSuperSecretKeyThatIsAtLeast32CharactersLongForProduction!",
                Issuer = "SampleProject.API",
                Audience = "SampleProject.Users",
                ExpirationMinutes = 60,
                RefreshTokenExpirationDays = 7
            };

            var options = Options.Create(_jwtOptions);
            _jwtService = new JwtService(options);
        }

        [Fact]
        public async Task GenerateTokenAsync_WithValidUser_ShouldReturnValidTokenResponse()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var username = "testuser";
            var email = "test@example.com";
            var firstName = "John";
            var lastName = "Doe";
            var role = UserRole.User;

            // Act
            var result = await _jwtService.GenerateTokenAsync(userId, username, email, firstName, lastName, role);

            // Assert
            result.Should().NotBeNull();
            result.AccessToken.Should().NotBeNullOrEmpty();
            result.RefreshToken.Should().NotBeNullOrEmpty();
            result.TokenType.Should().Be("Bearer");
            result.ExpiresIn.Should().Be(3600); // 60 minutes * 60 seconds
            result.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(60), TimeSpan.FromMinutes(1));
        }

        [Fact]
        public async Task GenerateTokenAsync_WithAdminUser_ShouldReturnTokenWithAdminRole()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var username = "adminuser";
            var email = "admin@example.com";
            var firstName = "Admin";
            var lastName = "User";
            var role = UserRole.Admin;

            // Act
            var result = await _jwtService.GenerateTokenAsync(userId, username, email, firstName, lastName, role);

            // Assert
            result.Should().NotBeNull();
            result.AccessToken.Should().NotBeNullOrEmpty();
            
            // Verify token contains admin role
            var roles = _jwtService.GetRoleFromToken(result.AccessToken);
            roles.Should().Contain("Admin");
        }

            [Fact]
            public async Task GenerateTokenAsync_WithUserRole_ShouldReturnTokenWithUserRole()
            {
                // Arrange
                var userId = Guid.NewGuid().ToString();
                var username = "regularuser";
                var email = "user@example.com";
                var firstName = "Regular";
                var lastName = "User";
                var role = UserRole.User;

                // Act
                var result = await _jwtService.GenerateTokenAsync(userId, username, email, firstName, lastName, role);

                // Assert
                result.Should().NotBeNull();
                result.AccessToken.Should().NotBeNullOrEmpty();
                
                // Verify token is valid
                var isValid = _jwtService.ValidateToken(result.AccessToken);
                isValid.Should().BeTrue("Token should be valid");
                
                // Verify token contains user role
                var roles = _jwtService.GetRoleFromToken(result.AccessToken);
                roles.Should().NotBeEmpty("Token should contain roles");
                roles.Should().Contain("User");
            }

        [Fact]
        public async Task GenerateTokenAsync_WithNullUserId_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => 
                _jwtService.GenerateTokenAsync(null!, "username", "email", "firstName", "lastName", UserRole.User));
        }

        [Fact]
        public void GenerateRefreshToken_ShouldReturnValidToken()
        {
            // Act
            var refreshToken = _jwtService.GenerateRefreshToken();

            // Assert
            refreshToken.Should().NotBeNull();
            refreshToken.Should().NotBeEmpty();
            refreshToken.Length.Should().BeGreaterThan(20); // Should be a reasonable length
        }

        [Fact]
        public void GenerateRefreshToken_ShouldReturnDifferentTokens()
        {
            // Act
            var token1 = _jwtService.GenerateRefreshToken();
            var token2 = _jwtService.GenerateRefreshToken();

            // Assert
            token1.Should().NotBe(token2);
        }

        [Fact]
        public async Task ValidateToken_WithValidToken_ShouldReturnTrue()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var username = "testuser";
            var email = "test@example.com";
            var firstName = "John";
            var lastName = "Doe";
            var role = UserRole.User;

            var result = await _jwtService.GenerateTokenAsync(userId, username, email, firstName, lastName, role);

            // Act
            var isValid = _jwtService.ValidateToken(result.AccessToken);

            // Assert
            isValid.Should().BeTrue();
        }

        [Fact]
        public void ValidateToken_WithInvalidToken_ShouldReturnFalse()
        {
            // Arrange
            var invalidToken = "invalid.token.here";

            // Act
            var isValid = _jwtService.ValidateToken(invalidToken);

            // Assert
            isValid.Should().BeFalse();
        }

        [Fact]
        public void ValidateToken_WithNullToken_ShouldReturnFalse()
        {
            // Act
            var isValid = _jwtService.ValidateToken(null!);

            // Assert
            isValid.Should().BeFalse();
        }

        [Fact]
        public void ValidateToken_WithEmptyToken_ShouldReturnFalse()
        {
            // Act
            var isValid = _jwtService.ValidateToken("");

            // Assert
            isValid.Should().BeFalse();
        }

        [Fact]
        public async Task GetUserIdFromToken_WithValidToken_ShouldReturnUserId()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var username = "testuser";
            var email = "test@example.com";
            var firstName = "John";
            var lastName = "Doe";
            var role = UserRole.User;

            var result = await _jwtService.GenerateTokenAsync(userId, username, email, firstName, lastName, role);

            // Debug: Print all claims in the token
            var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(result.AccessToken);
            Console.WriteLine("All claims in token:");
            foreach (var claim in jwtToken.Claims)
            {
                Console.WriteLine($"Type: {claim.Type}, Value: {claim.Value}");
            }

            // Act
            var resultUserId = _jwtService.GetUserIdFromToken(result.AccessToken);

            // Assert
            resultUserId.Should().Be(userId);
        }

        [Fact]
        public void GetUserIdFromToken_WithInvalidToken_ShouldReturnNull()
        {
            // Arrange
            var invalidToken = "invalid.token.here";

            // Act
            var result = _jwtService.GetUserIdFromToken(invalidToken);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetUserIdFromToken_WithNullToken_ShouldReturnNull()
        {
            // Act
            var result = _jwtService.GetUserIdFromToken(null!);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetUsernameFromToken_WithValidToken_ShouldReturnUsername()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var username = "testuser";
            var email = "test@example.com";
            var firstName = "John";
            var lastName = "Doe";
            var role = UserRole.User;

            var result = await _jwtService.GenerateTokenAsync(userId, username, email, firstName, lastName, role);

            // Act
            var resultUsername = _jwtService.GetUsernameFromToken(result.AccessToken);

            // Assert
            resultUsername.Should().Be(username);
        }

        [Fact]
        public void GetUsernameFromToken_WithInvalidToken_ShouldReturnNull()
        {
            // Arrange
            var invalidToken = "invalid.token.here";

            // Act
            var result = _jwtService.GetUsernameFromToken(invalidToken);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetRoleFromToken_WithValidToken_ShouldReturnRoles()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var username = "testuser";
            var email = "test@example.com";
            var firstName = "John";
            var lastName = "Doe";
            var role = UserRole.User;

            var result = await _jwtService.GenerateTokenAsync(userId, username, email, firstName, lastName, role);

            // Act
            var roleFromToken = _jwtService.GetRoleFromToken(result.AccessToken);

            // Assert
            roleFromToken.Should().NotBeNull();
            roleFromToken.Should().Be("User");
        }

        [Fact]
        public void GetRoleFromToken_WithInvalidToken_ShouldReturnNull()
        {
            // Arrange
            var invalidToken = "invalid.token.here";

            // Act
            var roleFromToken = _jwtService.GetRoleFromToken(invalidToken);

            // Assert
            roleFromToken.Should().BeNull();
        }

        [Fact]
        public async Task GetEmailFromToken_WithValidToken_ShouldReturnEmail()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var username = "testuser";
            var email = "test@example.com";
            var firstName = "John";
            var lastName = "Doe";
            var role = UserRole.User;

            var result = await _jwtService.GenerateTokenAsync(userId, username, email, firstName, lastName, role);

            // Act
            var resultEmail = _jwtService.GetEmailFromToken(result.AccessToken);

            // Assert
            resultEmail.Should().Be(email);
        }

        [Fact]
        public void GetEmailFromToken_WithInvalidToken_ShouldReturnNull()
        {
            // Arrange
            var invalidToken = "invalid.token.here";

            // Act
            var result = _jwtService.GetEmailFromToken(invalidToken);

            // Assert
            result.Should().BeNull();
        }
    }
}