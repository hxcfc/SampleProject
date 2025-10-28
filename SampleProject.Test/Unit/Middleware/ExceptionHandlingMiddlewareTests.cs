using SampleProject.Middleware;
using Common.Shared.Exceptions;
using SampleProject.Domain.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using System.Text;
using Xunit;
using FluentValidation;

namespace SampleProject.Test.Unit.Middleware
{
    /// <summary>
    /// Unit tests for ExceptionHandlingMiddleware
    /// </summary>
    public class ExceptionHandlingMiddlewareTests
    {
        private readonly Mock<ILogger<ExceptionHandlingMiddleware>> _loggerMock;
        private readonly Mock<RequestDelegate> _nextMock;
        private readonly ExceptionHandlingMiddleware _middleware;
        private readonly DefaultHttpContext _context;

        public ExceptionHandlingMiddlewareTests()
        {
            _loggerMock = new Mock<ILogger<ExceptionHandlingMiddleware>>();
            _nextMock = new Mock<RequestDelegate>();
            _middleware = new ExceptionHandlingMiddleware(_nextMock.Object, _loggerMock.Object);
            _context = new DefaultHttpContext();
        }

        [Fact]
        public async Task InvokeAsync_WithNoException_ShouldCallNext()
        {
            // Arrange
            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(_context);

            // Assert
            _nextMock.Verify(x => x(_context), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_WithBadRequestException_ShouldReturn400()
        {
            // Arrange
            var exception = new BadRequestException("Bad request message");
            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .ThrowsAsync(exception);

            _context.Response.Body = new MemoryStream();

            // Act
            await _middleware.InvokeAsync(_context);

            // Assert
            _context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            _context.Response.ContentType.Should().Be("application/json");

            var responseBody = await GetResponseBody(_context);
            responseBody.Should().Contain("Bad request message");
        }

        [Fact]
        public async Task InvokeAsync_WithUnauthorizedException_ShouldReturn401()
        {
            // Arrange
            var exception = new UnauthorizedException("Unauthorized message");
            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .ThrowsAsync(exception);

            _context.Response.Body = new MemoryStream();

            // Act
            await _middleware.InvokeAsync(_context);

            // Assert
            _context.Response.StatusCode.Should().Be((int)HttpStatusCode.Unauthorized);
            _context.Response.ContentType.Should().Be("application/json");

            var responseBody = await GetResponseBody(_context);
            responseBody.Should().Contain("Unauthorized message");
        }

        [Fact]
        public async Task InvokeAsync_WithForbiddenException_ShouldReturn403()
        {
            // Arrange
            var exception = new ForbiddenException("Forbidden message");
            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .ThrowsAsync(exception);

            _context.Response.Body = new MemoryStream();

            // Act
            await _middleware.InvokeAsync(_context);

            // Assert
            _context.Response.StatusCode.Should().Be((int)HttpStatusCode.Forbidden);
            _context.Response.ContentType.Should().Be("application/json");

            var responseBody = await GetResponseBody(_context);
            responseBody.Should().Contain("Forbidden message");
        }

        [Fact]
        public async Task InvokeAsync_WithNotFoundException_ShouldReturn404()
        {
            // Arrange
            var exception = new NotFoundException("Not found message");
            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .ThrowsAsync(exception);

            _context.Response.Body = new MemoryStream();

            // Act
            await _middleware.InvokeAsync(_context);

            // Assert
            _context.Response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
            _context.Response.ContentType.Should().Be("application/json");

            var responseBody = await GetResponseBody(_context);
            responseBody.Should().Contain("Not found message");
        }

        [Fact]
        public async Task InvokeAsync_WithValidationException_ShouldReturn400()
        {
            // Arrange
            var validationErrors = new List<FluentValidation.Results.ValidationFailure>
            {
                new FluentValidation.Results.ValidationFailure("Email", "Email is required"),
                new FluentValidation.Results.ValidationFailure("Password", "Password is required")
            };

            var exception = new ValidationException(validationErrors);
            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .ThrowsAsync(exception);

            _context.Response.Body = new MemoryStream();

            // Act
            await _middleware.InvokeAsync(_context);

            // Assert
            _context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            _context.Response.ContentType.Should().Be("application/json");

            var responseBody = await GetResponseBody(_context);
            responseBody.Should().Contain("Validation error");
        }

        [Fact]
        public async Task InvokeAsync_WithGenericException_ShouldReturn500()
        {
            // Arrange
            var exception = new Exception("Generic error message");
            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .ThrowsAsync(exception);

            _context.Response.Body = new MemoryStream();

            // Act
            await _middleware.InvokeAsync(_context);

            // Assert
            _context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
            _context.Response.ContentType.Should().Be("application/json");

            var responseBody = await GetResponseBody(_context);
            responseBody.Should().Contain("Please contact the application developer");
        }

        [Fact]
        public async Task InvokeAsync_WithOperationCanceledException_ShouldRethrow()
        {
            // Arrange
            var exception = new OperationCanceledException("Operation was canceled");
            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .ThrowsAsync(exception);

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() => _middleware.InvokeAsync(_context));
        }

        [Fact]
        public async Task InvokeAsync_WithFaviconRequest_ShouldReturn404()
        {
            // Arrange
            var exception = new Exception("File not found");
            _context.Request.Path = "/favicon.ico";
            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .ThrowsAsync(exception);

            // Act
            await _middleware.InvokeAsync(_context);

            // Assert
            _context.Response.StatusCode.Should().Be(404);
        }


        [Fact]
        public async Task InvokeAsync_WithResponseAlreadyStarted_ShouldNotWriteResponse()
        {
            // Arrange
            var exception = new BadRequestException("Test error");
            _nextMock
                .Setup(x => x(It.IsAny<HttpContext>()))
                .ThrowsAsync(exception);

            _context.Response.Body = new MemoryStream();
            
            // Simulate response already started
            _context.Response.StatusCode = 200;
            await _context.Response.WriteAsync("partial response");
            _context.Response.Body.Position = 0;

            // Act
            await _middleware.InvokeAsync(_context);

            // Assert - should not modify the existing response
            // The middleware should detect that response has started and not modify it
            _context.Response.StatusCode.Should().Be(200);
        }

        [Fact]
        public void Constructor_WithNullNext_ShouldThrow()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new ExceptionHandlingMiddleware(null!, _loggerMock.Object));
        }

        [Fact]
        public void Constructor_WithNullLogger_ShouldThrow()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new ExceptionHandlingMiddleware(_nextMock.Object, null!));
        }

        private static async Task<string> GetResponseBody(HttpContext context)
        {
            context.Response.Body.Position = 0;
            using var reader = new StreamReader(context.Response.Body);
            return await reader.ReadToEndAsync();
        }
    }
}
