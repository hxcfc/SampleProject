using SampleProject.Middleware;
using Common.Shared.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using FluentAssertions;
using Moq;
using Xunit;

namespace SampleProject.Test.Unit.Middleware
{
    /// <summary>
    /// Unit tests for SecurityHeadersMiddleware
    /// </summary>
    public class SecurityHeadersMiddlewareTests
    {
        private readonly Mock<ILogger<SecurityHeadersMiddleware>> _loggerMock;
        private readonly Mock<RequestDelegate> _nextMock;
        private readonly Mock<ISecurityHeadersService> _securityHeadersServiceMock;
        private readonly SecurityHeadersMiddleware _middleware;
        private readonly DefaultHttpContext _context;

        public SecurityHeadersMiddlewareTests()
        {
            _loggerMock = new Mock<ILogger<SecurityHeadersMiddleware>>();
            _nextMock = new Mock<RequestDelegate>();
            _securityHeadersServiceMock = new Mock<ISecurityHeadersService>();
            _middleware = new SecurityHeadersMiddleware(_nextMock.Object, _securityHeadersServiceMock.Object, _loggerMock.Object);
            _context = new DefaultHttpContext();
        }

        [Fact]
        public async Task InvokeAsync_WithValidRequest_ShouldCallSecurityHeadersService()
        {
            // Arrange
            _context.Request.Path = "/api/v1/test";
            
            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(_context);

            // Assert
            _securityHeadersServiceMock.Verify(x => x.ApplySecurityHeaders(_context), Times.Once);
            _nextMock.Verify(x => x(_context), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_WithHealthCheckEndpoint_ShouldCallSecurityHeadersService()
        {
            // Arrange
            _context.Request.Path = "/health";
            
            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(_context);

            // Assert
            _securityHeadersServiceMock.Verify(x => x.ApplySecurityHeaders(_context), Times.Once);
            _nextMock.Verify(x => x(_context), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_WithSwaggerEndpoint_ShouldCallSecurityHeadersService()
        {
            // Arrange
            _context.Request.Path = "/swagger";
            
            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(_context);

            // Assert
            _securityHeadersServiceMock.Verify(x => x.ApplySecurityHeaders(_context), Times.Once);
            _nextMock.Verify(x => x(_context), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_WithStaticFileEndpoint_ShouldCallSecurityHeadersService()
        {
            // Arrange
            _context.Request.Path = "/favicon.ico";
            
            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(_context);

            // Assert
            _securityHeadersServiceMock.Verify(x => x.ApplySecurityHeaders(_context), Times.Once);
            _nextMock.Verify(x => x(_context), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_WithDifferentPaths_ShouldCallSecurityHeadersService()
        {
            // Arrange
            var paths = new[] { "/api/v1/users", "/api/v1/auth/login", "/api/v1/test", "/api/v2/test" };
            
            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act & Assert
            foreach (var path in paths)
            {
                _context.Request.Path = path;
                
                await _middleware.InvokeAsync(_context);
            }

            // Assert
            _securityHeadersServiceMock.Verify(x => x.ApplySecurityHeaders(_context), Times.Exactly(paths.Length));
            _nextMock.Verify(x => x(_context), Times.Exactly(paths.Length));
        }

        [Fact]
        public async Task InvokeAsync_WithSecurityHeadersServiceException_ShouldLogErrorAndCallNext()
        {
            // Arrange
            _context.Request.Path = "/api/v1/test";
            var exception = new Exception("Security headers service error");
            
            _securityHeadersServiceMock
                .Setup(x => x.ApplySecurityHeaders(It.IsAny<object>()))
                .Throws(exception);
            
            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(_context);

            // Assert
            _securityHeadersServiceMock.Verify(x => x.ApplySecurityHeaders(_context), Times.Once);
            _nextMock.Verify(x => x(_context), Times.Once);
            
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error applying security headers")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
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
            await Assert.ThrowsAsync<Exception>(() => _middleware.InvokeAsync(_context));
            
            // Verify security headers service was still called
            _securityHeadersServiceMock.Verify(x => x.ApplySecurityHeaders(_context), Times.Once);
        }

        [Fact]
        public void Constructor_WithNullSecurityHeadersService_ShouldThrow()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new SecurityHeadersMiddleware(_nextMock.Object, null!, _loggerMock.Object));
        }

        [Fact]
        public void Constructor_WithNullNext_ShouldThrow()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new SecurityHeadersMiddleware(null!, _securityHeadersServiceMock.Object, _loggerMock.Object));
        }

        [Fact]
        public void Constructor_WithNullLogger_ShouldThrow()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new SecurityHeadersMiddleware(_nextMock.Object, _securityHeadersServiceMock.Object, null!));
        }

        [Fact]
        public async Task InvokeAsync_WithValidRequest_ShouldCallSecurityHeadersServiceBeforeNext()
        {
            // Arrange
            _context.Request.Path = "/api/v1/test";
            var callOrder = new List<string>();
            
            _securityHeadersServiceMock
                .Setup(x => x.ApplySecurityHeaders(It.IsAny<object>()))
                .Callback(() => callOrder.Add("SecurityHeadersService"));
            
            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .Callback(() => callOrder.Add("Next"))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(_context);

            // Assert
            callOrder.Should().HaveCount(2);
            callOrder[0].Should().Be("SecurityHeadersService");
            callOrder[1].Should().Be("Next");
        }

        [Fact]
        public async Task InvokeAsync_WithMultipleRequests_ShouldCallSecurityHeadersServiceForEach()
        {
            // Arrange
            var contexts = new[]
            {
                new DefaultHttpContext { Request = { Path = "/api/v1/test1" } },
                new DefaultHttpContext { Request = { Path = "/api/v1/test2" } },
                new DefaultHttpContext { Request = { Path = "/api/v1/test3" } }
            };
            
            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            foreach (var context in contexts)
            {
                await _middleware.InvokeAsync(context);
            }

            // Assert
            _securityHeadersServiceMock.Verify(x => x.ApplySecurityHeaders(It.IsAny<object>()), Times.Exactly(3));
            _nextMock.Verify(x => x(It.IsAny<HttpContext>()), Times.Exactly(3));
        }
    }
}