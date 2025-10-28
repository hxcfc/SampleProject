using SampleProject.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Common.Options;
using FluentAssertions;
using Moq;
using Xunit;
using System.Text.Json;

namespace SampleProject.Test.Unit.Middleware
{
    /// <summary>
    /// Unit tests for RateLimitingMiddleware
    /// </summary>
    public class RateLimitingMiddlewareTests
    {
        private readonly Mock<ILogger<RateLimitingMiddleware>> _loggerMock;
        private readonly Mock<RequestDelegate> _nextMock;
        private readonly RateLimitingMiddleware _middleware;
        private readonly DefaultHttpContext _context;

        public RateLimitingMiddlewareTests()
        {
            _loggerMock = new Mock<ILogger<RateLimitingMiddleware>>();
            _nextMock = new Mock<RequestDelegate>();
            
            var rateLimitingOptions = new RateLimitingOptions
            {
                Enabled = true,
                GlobalRateLimit = 1000,
                PerIpRateLimit = 100,
                AuthRateLimit = 10,
                RefreshTokenRateLimit = 5,
                WindowInMinutes = 1,
                EnableEndpointRateLimiting = true,
                EnableRateLimitHeaders = true
            };
            
            var options = Options.Create(rateLimitingOptions);
            _middleware = new RateLimitingMiddleware(_nextMock.Object, _loggerMock.Object, options);
            _context = new DefaultHttpContext();
        }

        [Fact]
        public async Task InvokeAsync_WithValidRequest_ShouldCallNext()
        {
            // Arrange
            _context.Request.Path = "/api/v1/test";
            _context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
            
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
            _context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
            
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
            _context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
            
            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(_context);

            // Assert
            _nextMock.Verify(x => x(_context), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_WithExceededPerIpRateLimit_ShouldReturn429()
        {
            // Arrange
            _context.Request.Path = "/api/v1/test";
            _context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
            _context.Response.Body = new MemoryStream();

            // Act - Make multiple requests to exceed per-IP rate limit (100 requests)
            for (int i = 0; i < 101; i++)
            {
                await _middleware.InvokeAsync(_context);
            }

            // Assert
            _context.Response.StatusCode.Should().Be(429);
        }

        [Fact]
        public async Task InvokeAsync_WithExceededAuthRateLimit_ShouldReturn429()
        {
            // Arrange
            _context.Request.Path = "/api/v1/auth/login";
            _context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
            _context.Response.Body = new MemoryStream();

            // Act - Make multiple requests to exceed auth rate limit (10 requests)
            for (int i = 0; i < 11; i++)
            {
                await _middleware.InvokeAsync(_context);
            }

            // Assert
            _context.Response.StatusCode.Should().Be(429);
        }

        [Fact]
        public async Task InvokeAsync_WithExceededRefreshTokenRateLimit_ShouldReturn429()
        {
            // Arrange
            _context.Request.Path = "/api/v1/auth/refresh";
            _context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
            _context.Response.Body = new MemoryStream();

            // Act - Make multiple requests to exceed refresh token rate limit (5 requests)
            for (int i = 0; i < 6; i++)
            {
                await _middleware.InvokeAsync(_context);
            }

            // Assert
            _context.Response.StatusCode.Should().Be(429);
        }

        [Fact]
        public async Task InvokeAsync_WithDifferentIPs_ShouldNotAffectEachOther()
        {
            // Arrange
            var context1 = new DefaultHttpContext
            {
                Request = { Path = "/api/v1/test" },
                Connection = { RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1") }
            };
            context1.Response.Body = new MemoryStream();

            var context2 = new DefaultHttpContext
            {
                Request = { Path = "/api/v1/test" },
                Connection = { RemoteIpAddress = System.Net.IPAddress.Parse("192.168.1.1") }
            };
            context2.Response.Body = new MemoryStream();

            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act - Make requests from different IPs
            for (int i = 0; i < 50; i++)
            {
                await _middleware.InvokeAsync(context1);
                await _middleware.InvokeAsync(context2);
            }

            // Assert
            context1.Response.StatusCode.Should().Be(200); // Should not be rate limited
            context2.Response.StatusCode.Should().Be(200); // Should not be rate limited
        }

        [Fact]
        public async Task InvokeAsync_WithNullIPAddress_ShouldUseDefaultIP()
        {
            // Arrange
            _context.Request.Path = "/api/v1/test";
            _context.Connection.RemoteIpAddress = null; // Null IP address
            
            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(_context);

            // Assert
            _nextMock.Verify(x => x(_context), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_WithXForwardedForHeader_ShouldUseForwardedIP()
        {
            // Arrange
            _context.Request.Path = "/api/v1/test";
            _context.Request.Headers.Add("X-Forwarded-For", "192.168.1.100, 10.0.0.1");
            _context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
            
            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(_context);

            // Assert
            _nextMock.Verify(x => x(_context), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_WithXRealIpHeader_ShouldUseRealIP()
        {
            // Arrange
            _context.Request.Path = "/api/v1/test";
            _context.Request.Headers.Add("X-Real-IP", "192.168.1.200");
            _context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
            
            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(_context);

            // Assert
            _nextMock.Verify(x => x(_context), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_WithRateLimitExceeded_ShouldLogWarning()
        {
            // Arrange
            _context.Request.Path = "/api/v1/test";
            _context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
            _context.Response.Body = new MemoryStream();

            // Act - Make multiple requests to exceed rate limit
            for (int i = 0; i < 101; i++)
            {
                await _middleware.InvokeAsync(_context);
            }

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task InvokeAsync_WithRateLimitExceeded_ShouldReturnRateLimitResponse()
        {
            // Arrange
            _context.Request.Path = "/api/v1/test";
            _context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
            _context.Response.Body = new MemoryStream();

            // Act - Make multiple requests to exceed rate limit
            for (int i = 0; i < 101; i++)
            {
                await _middleware.InvokeAsync(_context);
            }

            // Assert
            _context.Response.StatusCode.Should().Be(429);
            _context.Response.ContentType.Should().Be("application/json");
            
            var responseBody = await GetResponseBody(_context);
            responseBody.Should().Contain("success");
            responseBody.Should().Contain("message");
            responseBody.Should().Contain("retryAfter");
        }

        [Fact]
        public async Task InvokeAsync_WithDisabledRateLimiting_ShouldCallNext()
        {
            // Arrange
            var rateLimitingOptions = new RateLimitingOptions
            {
                Enabled = false,
                GlobalRateLimit = 1000,
                PerIpRateLimit = 100,
                AuthRateLimit = 10,
                RefreshTokenRateLimit = 5,
                WindowInMinutes = 1,
                EnableEndpointRateLimiting = true,
                EnableRateLimitHeaders = true
            };
            
            var options = Options.Create(rateLimitingOptions);
            var middleware = new RateLimitingMiddleware(_nextMock.Object, _loggerMock.Object, options);
            
            _context.Request.Path = "/api/v1/test";
            _context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");

            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await middleware.InvokeAsync(_context);

            // Assert
            _nextMock.Verify(x => x(_context), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_WithRateLimitHeadersEnabled_ShouldAddHeaders()
        {
            // Arrange
            _context.Request.Path = "/api/v1/test";
            _context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
            
            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(_context);

            // Assert
            _nextMock.Verify(x => x(_context), Times.Once);
            _context.Response.Headers.Should().ContainKey("X-RateLimit-Limit-Global");
            _context.Response.Headers.Should().ContainKey("X-RateLimit-Remaining-Global");
            _context.Response.Headers.Should().ContainKey("X-RateLimit-Limit-IP");
            _context.Response.Headers.Should().ContainKey("X-RateLimit-Remaining-IP");
            _context.Response.Headers.Should().ContainKey("X-RateLimit-Reset");
        }

        [Fact]
        public async Task InvokeAsync_WithEndpointRateLimitingDisabled_ShouldNotCheckEndpointLimits()
        {
            // Arrange
            var rateLimitingOptions = new RateLimitingOptions
            {
                Enabled = true,
                GlobalRateLimit = 1000,
                PerIpRateLimit = 100,
                AuthRateLimit = 10,
                RefreshTokenRateLimit = 5,
                WindowInMinutes = 1,
                EnableEndpointRateLimiting = false,
                EnableRateLimitHeaders = true
            };
            
            var options = Options.Create(rateLimitingOptions);
            var middleware = new RateLimitingMiddleware(_nextMock.Object, _loggerMock.Object, options);
            
            _context.Request.Path = "/api/v1/auth/login";
            _context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
            _context.Response.Body = new MemoryStream();

            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act - Make more requests than auth limit but less than per-IP limit
            for (int i = 0; i < 15; i++)
            {
                await middleware.InvokeAsync(_context);
            }

            // Assert - Should not be rate limited because endpoint rate limiting is disabled
            _context.Response.StatusCode.Should().Be(200);
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
                _context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
                
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
                _context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
                
                await _middleware.InvokeAsync(_context);
            }

            // Assert
            _nextMock.Verify(x => x(_context), Times.Exactly(paths.Length));
        }

        private static async Task<string> GetResponseBody(HttpContext context)
        {
            context.Response.Body.Position = 0;
            using var reader = new StreamReader(context.Response.Body);
            return await reader.ReadToEndAsync();
        }
    }
}