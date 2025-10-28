using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using SampleProject.Infrastructure.Interfaces;
using SampleProject.Middleware;
using System.Security.Claims;
using Xunit;

namespace SampleProject.Test.Unit.Middleware
{
    /// <summary>
    /// Unit tests for JwtTokenMiddleware
    /// </summary>
    public class JwtTokenMiddlewareTests
    {
        private readonly DefaultHttpContext _context;
        private readonly Mock<IExtendedJwtService> _jwtServiceMock;
        private readonly Mock<ILogger<JwtTokenMiddleware>> _loggerMock;
        private readonly JwtTokenMiddleware _middleware;
        private readonly Mock<RequestDelegate> _nextMock;

        public JwtTokenMiddlewareTests()
        {
            _loggerMock = new Mock<ILogger<JwtTokenMiddleware>>();
            _nextMock = new Mock<RequestDelegate>();
            _jwtServiceMock = new Mock<IExtendedJwtService>();
            _middleware = new JwtTokenMiddleware(_nextMock.Object, _loggerMock.Object);
            _context = new DefaultHttpContext();
        }

        [Fact]
        public async Task InvokeAsync_WithAnonymousEndpoint_ShouldCallNext()
        {
            // Arrange
            _context.Request.Path = "/api/v1/auth/login";

            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(_context, _jwtServiceMock.Object);

            // Assert
            _nextMock.Verify(x => x(_context), Times.Once);
            _jwtServiceMock.Verify(x => x.GetTokenFromRequest(It.IsAny<HttpRequest>()), Times.Never);
        }

        [Fact]
        public async Task InvokeAsync_WithAuthRefreshEndpoint_ShouldCallNext()
        {
            // Arrange
            _context.Request.Path = "/api/v1/auth/refresh";

            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(_context, _jwtServiceMock.Object);

            // Assert
            _nextMock.Verify(x => x(_context), Times.Once);
            _jwtServiceMock.Verify(x => x.GetTokenFromRequest(It.IsAny<HttpRequest>()), Times.Never);
        }

        [Fact]
        public async Task InvokeAsync_WithDifferentPaths_ShouldCallNext()
        {
            // Arrange
            var paths = new[] { "/api/v1/users", "/api/v1/auth/me", "/api/v1/test", "/api/v2/test" };

            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act & Assert
            foreach (var path in paths)
            {
                _context.Request.Path = path;

                await _middleware.InvokeAsync(_context, _jwtServiceMock.Object);
            }

            // Assert
            _nextMock.Verify(x => x(_context), Times.Exactly(paths.Length));
        }

        [Fact]
        public async Task InvokeAsync_WithEmptyToken_ShouldNotSetUser()
        {
            // Arrange
            _context.Request.Path = "/api/v1/test";

            _jwtServiceMock
                .Setup(x => x.GetTokenFromRequest(It.IsAny<HttpRequest>()))
                .Returns(string.Empty);

            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(_context, _jwtServiceMock.Object);

            // Assert
            _nextMock.Verify(x => x(_context), Times.Once);
            _context.User.Identity?.IsAuthenticated.Should().BeFalse();
        }

        [Fact]
        public async Task InvokeAsync_WithException_ShouldLogError()
        {
            // Arrange
            _context.Request.Path = "/api/v1/test";

            _jwtServiceMock
                .Setup(x => x.GetTokenFromRequest(It.IsAny<HttpRequest>()))
                .Throws(new Exception("JWT service error"));

            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(_context, _jwtServiceMock.Object);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred in JwtTokenMiddleware")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_WithExceptionInJwtService_ShouldCallNext()
        {
            // Arrange
            _context.Request.Path = "/api/v1/test";

            _jwtServiceMock
                .Setup(x => x.GetTokenFromRequest(It.IsAny<HttpRequest>()))
                .Throws(new Exception("JWT service error"));

            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(_context, _jwtServiceMock.Object);

            // Assert
            _nextMock.Verify(x => x(_context), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_WithExceptionInNext_ShouldRethrow()
        {
            // Arrange
            _context.Request.Path = "/api/v1/test";

            var exception = new Exception("Test exception");
            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .ThrowsAsync(exception);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _middleware.InvokeAsync(_context, _jwtServiceMock.Object));
        }

        [Fact]
        public async Task InvokeAsync_WithHealthCheckEndpoint_ShouldCallNext()
        {
            // Arrange
            _context.Request.Path = "/health";

            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(_context, _jwtServiceMock.Object);

            // Assert
            _nextMock.Verify(x => x(_context), Times.Once);
            _jwtServiceMock.Verify(x => x.GetTokenFromRequest(It.IsAny<HttpRequest>()), Times.Never);
        }

        [Fact]
        public async Task InvokeAsync_WithInvalidToken_ShouldLogWarning()
        {
            // Arrange
            _context.Request.Path = "/api/v1/test";
            var token = "invalid_token";

            _jwtServiceMock
                .Setup(x => x.GetTokenFromRequest(It.IsAny<HttpRequest>()))
                .Returns(token);

            _jwtServiceMock
                .Setup(x => x.ValidateToken(token))
                .Returns(false);

            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(_context, _jwtServiceMock.Object);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalid or expired JWT token")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_WithInvalidToken_ShouldNotSetUser()
        {
            // Arrange
            _context.Request.Path = "/api/v1/test";
            var token = "invalid_token";

            _jwtServiceMock
                .Setup(x => x.GetTokenFromRequest(It.IsAny<HttpRequest>()))
                .Returns(token);

            _jwtServiceMock
                .Setup(x => x.ValidateToken(token))
                .Returns(false);

            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(_context, _jwtServiceMock.Object);

            // Assert
            _nextMock.Verify(x => x(_context), Times.Once);
            _context.User.Identity?.IsAuthenticated.Should().BeFalse();
        }

        [Fact]
        public async Task InvokeAsync_WithNoToken_ShouldLogDebug()
        {
            // Arrange
            _context.Request.Path = "/api/v1/test";

            _jwtServiceMock
                .Setup(x => x.GetTokenFromRequest(It.IsAny<HttpRequest>()))
                .Returns((string?)null);

            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(_context, _jwtServiceMock.Object);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No JWT token found in request")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_WithNoToken_ShouldNotSetUser()
        {
            // Arrange
            _context.Request.Path = "/api/v1/test";

            _jwtServiceMock
                .Setup(x => x.GetTokenFromRequest(It.IsAny<HttpRequest>()))
                .Returns((string?)null);

            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(_context, _jwtServiceMock.Object);

            // Assert
            _nextMock.Verify(x => x(_context), Times.Once);
            _context.User.Identity?.IsAuthenticated.Should().BeFalse();
        }

        [Fact]
        public async Task InvokeAsync_WithSwaggerEndpoint_ShouldCallNext()
        {
            // Arrange
            _context.Request.Path = "/swagger";

            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(_context, _jwtServiceMock.Object);

            // Assert
            _nextMock.Verify(x => x(_context), Times.Once);
            _jwtServiceMock.Verify(x => x.GetTokenFromRequest(It.IsAny<HttpRequest>()), Times.Never);
        }

        [Fact]
        public async Task InvokeAsync_WithSwaggerSubPath_ShouldCallNext()
        {
            // Arrange
            _context.Request.Path = "/swagger/index.html";

            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(_context, _jwtServiceMock.Object);

            // Assert
            _nextMock.Verify(x => x(_context), Times.Once);
            _jwtServiceMock.Verify(x => x.GetTokenFromRequest(It.IsAny<HttpRequest>()), Times.Never);
        }

        [Fact]
        public async Task InvokeAsync_WithValidToken_ShouldLogDebug()
        {
            // Arrange
            _context.Request.Path = "/api/v1/test";
            var token = "valid_token";
            var userId = "user123";

            _jwtServiceMock
                .Setup(x => x.GetTokenFromRequest(It.IsAny<HttpRequest>()))
                .Returns(token);

            _jwtServiceMock
                .Setup(x => x.ValidateToken(token))
                .Returns(true);

            _jwtServiceMock
                .Setup(x => x.GetUserIdFromToken(token))
                .Returns(userId);

            _jwtServiceMock
                .Setup(x => x.GetUsernameFromToken(token))
                .Returns("testuser");

            _jwtServiceMock
                .Setup(x => x.GetEmailFromToken(token))
                .Returns("test@example.com");

            _jwtServiceMock
                .Setup(x => x.GetFirstNameFromToken(token))
                .Returns("John");

            _jwtServiceMock
                .Setup(x => x.GetLastNameFromToken(token))
                .Returns("Doe");

            _jwtServiceMock
                .Setup(x => x.GetRoleFromToken(token))
                .Returns("User");

            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(_context, _jwtServiceMock.Object);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("JWT token processed successfully")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_WithValidToken_ShouldSetUserClaims()
        {
            // Arrange
            _context.Request.Path = "/api/v1/test";
            var token = "valid_token";
            var userId = "user123";
            var username = "testuser";
            var email = "test@example.com";
            var firstName = "John";
            var lastName = "Doe";
            var role = "User";

            _jwtServiceMock
                .Setup(x => x.GetTokenFromRequest(It.IsAny<HttpRequest>()))
                .Returns(token);

            _jwtServiceMock
                .Setup(x => x.ValidateToken(token))
                .Returns(true);

            _jwtServiceMock
                .Setup(x => x.GetUserIdFromToken(token))
                .Returns(userId);

            _jwtServiceMock
                .Setup(x => x.GetUsernameFromToken(token))
                .Returns(username);

            _jwtServiceMock
                .Setup(x => x.GetEmailFromToken(token))
                .Returns(email);

            _jwtServiceMock
                .Setup(x => x.GetFirstNameFromToken(token))
                .Returns(firstName);

            _jwtServiceMock
                .Setup(x => x.GetLastNameFromToken(token))
                .Returns(lastName);

            _jwtServiceMock
                .Setup(x => x.GetRoleFromToken(token))
                .Returns(role);

            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(_context, _jwtServiceMock.Object);

            // Assert
            _nextMock.Verify(x => x(_context), Times.Once);
            _context.User.Should().NotBeNull();
            _context.User.Identity.Should().NotBeNull();
            _context.User.Identity!.IsAuthenticated.Should().BeTrue();
            _context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value.Should().Be(userId);
            _context.User.FindFirst(ClaimTypes.Name)?.Value.Should().Be(username);
            _context.User.FindFirst(ClaimTypes.Email)?.Value.Should().Be(email);
            _context.User.FindFirst(ClaimTypes.GivenName)?.Value.Should().Be(firstName);
            _context.User.FindFirst(ClaimTypes.Surname)?.Value.Should().Be(lastName);
            _context.User.FindFirst(ClaimTypes.Role)?.Value.Should().Be("User");
        }

        [Fact]
        public void Constructor_WithNullNext_ShouldThrow()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new JwtTokenMiddleware(null!, _loggerMock.Object));
        }

        [Fact]
        public void Constructor_WithNullLogger_ShouldThrow()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new JwtTokenMiddleware(_nextMock.Object, null!));
        }
    }
}