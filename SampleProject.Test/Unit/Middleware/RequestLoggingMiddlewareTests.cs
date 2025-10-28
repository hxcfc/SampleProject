using SampleProject.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using FluentAssertions;
using Moq;
using Xunit;

namespace SampleProject.Test.Unit.Middleware
{
    /// <summary>
    /// Unit tests for RequestLoggingMiddleware
    /// </summary>
    public class RequestLoggingMiddlewareTests
    {
        private readonly Mock<ILogger<RequestLoggingMiddleware>> _loggerMock;
        private readonly Mock<RequestDelegate> _nextMock;
        private readonly RequestLoggingMiddleware _middleware;
        private readonly DefaultHttpContext _context;

        public RequestLoggingMiddlewareTests()
        {
            _loggerMock = new Mock<ILogger<RequestLoggingMiddleware>>();
            _nextMock = new Mock<RequestDelegate>();
            _middleware = new RequestLoggingMiddleware(_nextMock.Object, _loggerMock.Object);
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
        public async Task InvokeAsync_WithValidRequest_ShouldLogRequest()
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
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Request")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task InvokeAsync_WithValidRequest_ShouldLogResponse()
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
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Response")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task InvokeAsync_WithPostRequest_ShouldLogRequestBody()
        {
            // Arrange
            _context.Request.Path = "/api/v1/test";
            _context.Request.Method = "POST";
            _context.Request.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("{\"test\": \"data\"}"));
            
            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(_context);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Request") && v.ToString()!.Contains("Body")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task InvokeAsync_WithPutRequest_ShouldLogRequestBody()
        {
            // Arrange
            _context.Request.Path = "/api/v1/test";
            _context.Request.Method = "PUT";
            _context.Request.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("{\"test\": \"data\"}"));
            
            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(_context);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Request") && v.ToString()!.Contains("Body")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task InvokeAsync_WithPatchRequest_ShouldLogRequestBody()
        {
            // Arrange
            _context.Request.Path = "/api/v1/test";
            _context.Request.Method = "PATCH";
            _context.Request.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("{\"test\": \"data\"}"));
            
            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(_context);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Request") && v.ToString()!.Contains("Body")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task InvokeAsync_WithGetRequest_ShouldNotLogRequestBody()
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
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Request") && v.ToString()!.Contains("Body")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }

        [Fact]
        public async Task InvokeAsync_WithDeleteRequest_ShouldNotLogRequestBody()
        {
            // Arrange
            _context.Request.Path = "/api/v1/test";
            _context.Request.Method = "DELETE";
            
            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(_context);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Request") && v.ToString()!.Contains("Body")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }

        [Fact]
        public async Task InvokeAsync_WithErrorResponse_ShouldLogErrorResponse()
        {
            // Arrange
            _context.Request.Path = "/api/v1/test";
            _context.Request.Method = "GET";
            _context.Response.StatusCode = 500;
            _context.Response.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("{\"error\": \"Internal server error\"}"));
            
            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(_context);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Response") && v.ToString()!.Contains("Error Body")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
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
        public async Task InvokeAsync_WithHealthCheckEndpoint_ShouldLogRequest()
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
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Request")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task InvokeAsync_WithSwaggerEndpoint_ShouldLogRequest()
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
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Request")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task InvokeAsync_WithStaticFileEndpoint_ShouldLogRequest()
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
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Request")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task InvokeAsync_WithDifferentHttpMethods_ShouldLogAll()
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
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Request")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Exactly(methods.Length));
        }

        [Fact]
        public async Task InvokeAsync_WithDifferentPaths_ShouldLogAll()
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
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Request")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Exactly(paths.Length));
        }

        [Fact]
        public void Constructor_WithNullNext_ShouldThrow()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new RequestLoggingMiddleware(null!, _loggerMock.Object));
        }

        [Fact]
        public void Constructor_WithNullLogger_ShouldThrow()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new RequestLoggingMiddleware(_nextMock.Object, null!));
        }
    }
}
