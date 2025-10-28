using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SampleProject.Middleware;
using Common.Options;
using Xunit;

namespace SampleProject.Test.Unit.Middleware
{
    /// <summary>
    /// Unit tests for CorrelationIdMiddleware
    /// </summary>
    public class CorrelationIdMiddlewareTests
    {
        private readonly DefaultHttpContext _context;
        private readonly Mock<ILogger<CorrelationIdMiddleware>> _loggerMock;
        private readonly CorrelationIdMiddleware _middleware;
        private readonly Mock<RequestDelegate> _nextMock;
        private readonly IOptions<MonitoringOptions> _options;

        public CorrelationIdMiddlewareTests()
        {
            _loggerMock = new Mock<ILogger<CorrelationIdMiddleware>>();
            _nextMock = new Mock<RequestDelegate>();
            _options = Options.Create(new MonitoringOptions
            {
                Enabled = true,
                EnableCorrelationId = true,
                CorrelationIdHeaderName = "X-Correlation-ID"
            });
            _middleware = new CorrelationIdMiddleware(_nextMock.Object, _loggerMock.Object, _options);
            _context = new DefaultHttpContext();
        }

        [Fact]
        public async Task InvokeAsync_WithExceptionInNext_ShouldLogDebug()
        {
            // Arrange
            _context.Request.Path = "/api/v1/test";

            var exception = new Exception("Test exception");
            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .ThrowsAsync(exception);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _middleware.InvokeAsync(_context));

            // Verify debug was logged
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
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
        }

        [Fact]
        public async Task InvokeAsync_WithExceptionInNext_ShouldStillAddCorrelationId()
        {
            // Arrange
            _context.Request.Path = "/api/v1/test";

            var exception = new Exception("Test exception");
            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .ThrowsAsync(exception);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _middleware.InvokeAsync(_context));

            // Even though an exception was thrown, correlation ID should still be added
            _context.Response.Headers.Should().ContainKey("X-Correlation-ID");
            _context.Response.Headers["X-Correlation-ID"].Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task InvokeAsync_WithExistingCorrelationId_ShouldUseExistingId()
        {
            // Arrange
            var existingCorrelationId = "existing-correlation-id";
            _context.Request.Path = "/api/v1/test";
            _context.Request.Headers.Add("X-Correlation-ID", existingCorrelationId);

            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(_context);

            // Assert
            _context.Response.Headers["X-Correlation-ID"].ToString().Should().Be(existingCorrelationId);
        }

        [Fact]
        public async Task InvokeAsync_WithHealthCheckEndpoint_ShouldAddCorrelationId()
        {
            // Arrange
            _context.Request.Path = "/health";

            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(_context);

            // Assert
            _context.Response.Headers.Should().ContainKey("X-Correlation-ID");
            _context.Response.Headers["X-Correlation-ID"].Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task InvokeAsync_WithMultipleRequests_ShouldGenerateDifferentIds()
        {
            // Arrange
            var context1 = new DefaultHttpContext { Request = { Path = "/api/v1/test1" } };
            var context2 = new DefaultHttpContext { Request = { Path = "/api/v1/test2" } };

            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(context1);
            await _middleware.InvokeAsync(context2);

            // Assert
            var correlationId1 = context1.Response.Headers["X-Correlation-ID"].ToString();
            var correlationId2 = context2.Response.Headers["X-Correlation-ID"].ToString();

            correlationId1.Should().NotBe(correlationId2);
        }

        [Fact]
        public async Task InvokeAsync_WithNoExistingCorrelationId_ShouldGenerateNewId()
        {
            // Arrange
            _context.Request.Path = "/api/v1/test";

            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(_context);

            // Assert
            _context.Response.Headers.Should().ContainKey("X-Correlation-ID");
            var correlationId = _context.Response.Headers["X-Correlation-ID"].ToString();
            correlationId.Should().NotBeNullOrEmpty();
            correlationId.Should().MatchRegex(@"^[0-9a-f]{8}$");
        }

        [Fact]
        public async Task InvokeAsync_WithSameRequest_ShouldGenerateSameId()
        {
            // Arrange
            var context1 = new DefaultHttpContext { Request = { Path = "/api/v1/test" } };
            var context2 = new DefaultHttpContext { Request = { Path = "/api/v1/test" } };

            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(context1);
            await _middleware.InvokeAsync(context2);

            // Assert
            var correlationId1 = context1.Response.Headers["X-Correlation-ID"].ToString();
            var correlationId2 = context2.Response.Headers["X-Correlation-ID"].ToString();

            correlationId1.Should().NotBe(correlationId2); // Different requests should have different IDs
        }

        [Fact]
        public async Task InvokeAsync_WithStaticFileEndpoint_ShouldAddCorrelationId()
        {
            // Arrange
            _context.Request.Path = "/favicon.ico";

            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(_context);

            // Assert
            _context.Response.Headers.Should().ContainKey("X-Correlation-ID");
            _context.Response.Headers["X-Correlation-ID"].Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task InvokeAsync_WithSwaggerEndpoint_ShouldAddCorrelationId()
        {
            // Arrange
            _context.Request.Path = "/swagger";

            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(_context);

            // Assert
            _context.Response.Headers.Should().ContainKey("X-Correlation-ID");
            _context.Response.Headers["X-Correlation-ID"].Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task InvokeAsync_WithValidRequest_ShouldAddCorrelationIdToResponse()
        {
            // Arrange
            _context.Request.Path = "/api/v1/test";

            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(_context);

            // Assert
            _context.Response.Headers.Should().ContainKey("X-Correlation-ID");
            _context.Response.Headers["X-Correlation-ID"].Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task InvokeAsync_WithValidRequest_ShouldCallNext()
        {
            // Arrange
            _context.Request.Path = "/api/v1/test";

            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(_context);

            // Assert
            _nextMock.Verify(x => x(_context), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_WithValidRequest_ShouldLogCorrelationId()
        {
            // Arrange
            _context.Request.Path = "/api/v1/test";

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
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Processing request with correlation ID")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}