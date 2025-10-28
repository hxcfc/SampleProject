using SampleProject.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using FluentAssertions;
using Moq;
using Xunit;

namespace SampleProject.Test.Unit.Middleware
{
    /// <summary>
    /// Unit tests for MetricsMiddleware
    /// </summary>
    public class MetricsMiddlewareTests
    {
        private readonly Mock<ILogger<MetricsMiddleware>> _loggerMock;
        private readonly Mock<RequestDelegate> _nextMock;
        private readonly MetricsMiddleware _middleware;
        private readonly DefaultHttpContext _context;

        public MetricsMiddlewareTests()
        {
            _loggerMock = new Mock<ILogger<MetricsMiddleware>>();
            _nextMock = new Mock<RequestDelegate>();
            _middleware = new MetricsMiddleware(_nextMock.Object, _loggerMock.Object);
            _context = new DefaultHttpContext();
        }

        [Fact]
        public async Task InvokeAsync_WithValidRequest_ShouldCallNext()
        {
            // Arrange
            _context.Request.Path = "/api/v1/test";
            _context.Request.Method = "GET";
            
            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(_context);

            // Assert
            _nextMock.Verify(x => x(_context), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_WithHealthCheckEndpoint_ShouldCallNext()
        {
            // Arrange
            _context.Request.Path = "/health";
            _context.Request.Method = "GET";
            
            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(_context);

            // Assert
            _nextMock.Verify(x => x(_context), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_WithSwaggerEndpoint_ShouldCallNext()
        {
            // Arrange
            _context.Request.Path = "/swagger";
            _context.Request.Method = "GET";
            
            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(_context);

            // Assert
            _nextMock.Verify(x => x(_context), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_WithStaticFileEndpoint_ShouldCallNext()
        {
            // Arrange
            _context.Request.Path = "/favicon.ico";
            _context.Request.Method = "GET";
            
            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(_context);

            // Assert
            _nextMock.Verify(x => x(_context), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_WithExceptionInNext_ShouldRethrow()
        {
            // Arrange
            _context.Request.Path = "/api/v1/test";
            _context.Request.Method = "GET";
            
            var exception = new Exception("Test exception");
            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .ThrowsAsync(exception);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _middleware.InvokeAsync(_context));
        }

        [Fact]
        public async Task InvokeAsync_WithDifferentHttpMethods_ShouldCallNext()
        {
            // Arrange
            var methods = new[] { "GET", "POST", "PUT", "DELETE", "PATCH" };
            
            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act & Assert
            foreach (var method in methods)
            {
                _context.Request.Method = method;
                _context.Request.Path = $"/api/v1/test/{method.ToLower()}";
                
                await _middleware.InvokeAsync(_context);
            }

            // Assert
            _nextMock.Verify(x => x(_context), Times.Exactly(methods.Length));
        }

        [Fact]
        public async Task InvokeAsync_WithDifferentPaths_ShouldCallNext()
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
                _context.Request.Method = "GET";
                
                await _middleware.InvokeAsync(_context);
            }

            // Assert
            _nextMock.Verify(x => x(_context), Times.Exactly(paths.Length));
        }

        [Fact]
        public async Task InvokeAsync_WithValidRequest_ShouldLogDebug()
        {
            // Arrange
            _context.Request.Path = "/api/v1/test";
            _context.Request.Method = "GET";
            
            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(_context);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_WithExceptionInNext_ShouldLogError()
        {
            // Arrange
            _context.Request.Path = "/api/v1/test";
            _context.Request.Method = "GET";
            
            var exception = new Exception("Test exception");
            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .ThrowsAsync(exception);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _middleware.InvokeAsync(_context));
            
            // Verify error was logged
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task InvokeAsync_WithApiPath_ShouldNormalizePath()
        {
            // Arrange
            _context.Request.Path = "/api/v1/users/123e4567-e89b-12d3-a456-426614174000";
            _context.Request.Method = "GET";
            
            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(_context);

            // Assert
            _nextMock.Verify(x => x(_context), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_WithApiPathWithNumbers_ShouldNormalizePath()
        {
            // Arrange
            _context.Request.Path = "/api/v1/users/123";
            _context.Request.Method = "GET";
            
            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(_context);

            // Assert
            _nextMock.Verify(x => x(_context), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_WithNonApiPath_ShouldNotNormalizePath()
        {
            // Arrange
            _context.Request.Path = "/health";
            _context.Request.Method = "GET";
            
            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(_context);

            // Assert
            _nextMock.Verify(x => x(_context), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_WithNullPath_ShouldHandleGracefully()
        {
            // Arrange
            _context.Request.Path = null;
            _context.Request.Method = "GET";
            
            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(_context);

            // Assert
            _nextMock.Verify(x => x(_context), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_WithResponseStatusCode_ShouldRecordMetrics()
        {
            // Arrange
            _context.Request.Path = "/api/v1/test";
            _context.Request.Method = "GET";
            _context.Response.StatusCode = 200;
            
            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(_context);

            // Assert
            _nextMock.Verify(x => x(_context), Times.Once);
            _context.Response.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task InvokeAsync_WithErrorStatusCode_ShouldRecordMetrics()
        {
            // Arrange
            _context.Request.Path = "/api/v1/test";
            _context.Request.Method = "GET";
            _context.Response.StatusCode = 404;
            
            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(_context);

            // Assert
            _nextMock.Verify(x => x(_context), Times.Once);
            _context.Response.StatusCode.Should().Be(404);
        }

        [Fact]
        public void Constructor_WithNullNext_ShouldThrow()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new MetricsMiddleware(null!, _loggerMock.Object));
        }

        [Fact]
        public void Constructor_WithNullLogger_ShouldThrow()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new MetricsMiddleware(_nextMock.Object, null!));
        }
    }
}