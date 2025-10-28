using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SampleProject.Controllers;
using SampleProject.Domain.Common;
using SampleProject.Domain.Responses;
using System.Text;
using Xunit;

namespace SampleProject.Test.Unit.Controllers
{
    /// <summary>
    /// Unit tests for BaseController
    /// </summary>
    public class BaseControllerTests
    {
        private readonly TestController _controller;
        private readonly Mock<IMediator> _mediatorMock;

        public BaseControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _controller = new TestController(_mediatorMock.Object);
        }

        [Fact]
        public void Constructor_WithNullMediator_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new TestController(null!));
        }

        [Fact]
        public async Task GetXmlBodyAsync_WithNullStream_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _controller.GetXmlBodyAsync(null!));
        }

        [Fact]
        public async Task GetXmlBodyAsync_WithValidStream_ShouldReturnXmlContent()
        {
            // Arrange
            var xmlContent = "<root><item>test</item></root>";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(xmlContent));

            // Act
            var result = await _controller.GetXmlBodyAsync(stream);

            // Assert
            result.Should().Be(xmlContent);
        }

        [Fact]
        public void HandleResult_WithFailureResult_ShouldReturnBadRequest()
        {
            // Arrange
            var errorMessage = "Test error";
            var result = Result<string>.Failure(errorMessage);

            // Act
            var response = _controller.HandleResult(result);

            // Assert
            response.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = response as BadRequestObjectResult;
            badRequestResult!.Value.Should().BeOfType<ErrorResponseModel>();

            var errorModel = badRequestResult.Value as ErrorResponseModel;
            errorModel!.Error.Should().Be("Operation Failed");
            errorModel.ErrorDescription.Should().Be(errorMessage);
        }

        [Fact]
        public void HandleResult_WithFailureResultAndNullError_ShouldReturnBadRequestWithDefaultError()
        {
            // Arrange
            var result = Result<string>.Failure(null);

            // Act
            var response = _controller.HandleResult(result);

            // Assert
            response.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = response as BadRequestObjectResult;
            badRequestResult!.Value.Should().BeOfType<ErrorResponseModel>();

            var errorModel = badRequestResult.Value as ErrorResponseModel;
            errorModel!.Error.Should().Be("Operation Failed");
            errorModel.ErrorDescription.Should().Be("Unknown error occurred");
        }

        [Fact]
        public void HandleResult_WithFailureResultNoData_ShouldReturnBadRequest()
        {
            // Arrange
            var errorMessage = "Test error";
            var result = Result.Failure(errorMessage);

            // Act
            var response = _controller.HandleResult(result);

            // Assert
            response.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = response as BadRequestObjectResult;
            badRequestResult!.Value.Should().BeOfType<ErrorResponseModel>();

            var errorModel = badRequestResult.Value as ErrorResponseModel;
            errorModel!.Error.Should().Be("Operation Failed");
            errorModel.ErrorDescription.Should().Be(errorMessage);
        }

        [Fact]
        public void HandleResult_WithSuccessResult_ShouldReturnOk()
        {
            // Arrange
            var data = "test_data";
            var result = Result<string>.Success(data);

            // Act
            var response = _controller.HandleResult(result);

            // Assert
            response.Should().BeOfType<ObjectResult>();
            var objectResult = response as ObjectResult;
            objectResult!.StatusCode.Should().Be(200);
            objectResult.Value.Should().Be(data);
        }

        [Fact]
        public void HandleResult_WithSuccessResultAndCustomStatusCode_ShouldReturnCustomStatusCode()
        {
            // Arrange
            var data = "test_data";
            var result = Result<string>.Success(data);
            var customStatusCode = 201;

            // Act
            var response = _controller.HandleResult(result, customStatusCode);

            // Assert
            response.Should().BeOfType<ObjectResult>();
            var objectResult = response as ObjectResult;
            objectResult!.StatusCode.Should().Be(customStatusCode);
            objectResult.Value.Should().Be(data);
        }

        [Fact]
        public void HandleResult_WithSuccessResultNoData_ShouldReturnOk()
        {
            // Arrange
            var result = Result.Success();

            // Act
            var response = _controller.HandleResult(result);

            // Assert
            response.Should().BeOfType<StatusCodeResult>();
            var statusCodeResult = response as StatusCodeResult;
            statusCodeResult!.StatusCode.Should().Be(200);
        }

        [Fact]
        public void HandleResult_WithSuccessResultNoDataAndCustomStatusCode_ShouldReturnCustomStatusCode()
        {
            // Arrange
            var result = Result.Success();
            var customStatusCode = 204;

            // Act
            var response = _controller.HandleResult(result, customStatusCode);

            // Assert
            response.Should().BeOfType<StatusCodeResult>();
            var statusCodeResult = response as StatusCodeResult;
            statusCodeResult!.StatusCode.Should().Be(customStatusCode);
        }
    }

    /// <summary>
    /// Test controller for testing BaseController functionality
    /// </summary>
    public class TestController : BaseController
    {
        public TestController(IMediator mediator) : base(mediator)
        {
        }

        public new async Task<string> GetXmlBodyAsync(Stream bodyContent)
        {
            return await base.GetXmlBodyAsync(bodyContent);
        }

        public new IActionResult HandleResult<T>(Result<T> result, int successStatusCode = 200)
        {
            return base.HandleResult(result, successStatusCode);
        }

        public new IActionResult HandleResult(Result result, int successStatusCode = 200)
        {
            return base.HandleResult(result, successStatusCode);
        }
    }
}