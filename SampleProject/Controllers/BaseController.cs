using SampleProject.Domain.Common;
using SampleProject.Domain.Responses;
using Microsoft.AspNetCore.Mvc;
using MediatR;

namespace SampleProject.Controllers
{
    /// <summary>
    /// Base controller providing common functionality for all API controllers
    /// </summary>
    public abstract class BaseController : ControllerBase
    {
        /// <summary>
        /// MediatR mediator instance for handling commands and queries
        /// </summary>
        protected readonly IMediator Mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseController"/> class
        /// </summary>
        /// <param name="mediator">MediatR mediator instance</param>
        protected BaseController(IMediator mediator)
        {
            Mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        /// <summary>
        /// Handles the result of a MediatR operation and returns appropriate HTTP response
        /// </summary>
        /// <typeparam name="T">The type of data returned by the operation</typeparam>
        /// <param name="result">The result from MediatR operation</param>
        /// <param name="successStatusCode">HTTP status code for success (default: 200)</param>
        /// <returns>Appropriate HTTP response</returns>
        protected IActionResult HandleResult<T>(Result<T> result, int successStatusCode = 200)
        {
            if (result.IsSuccess)
            {
                return StatusCode(successStatusCode, result.Value);
            }

            return BadRequest(new ErrorResponseModel
            {
                Error = StringMessages.OperationFailed,
                ErrorDescription = result.Error ?? StringMessages.UnknownErrorOccurred
            });
        }

        /// <summary>
        /// Handles the result of a MediatR operation that doesn't return data
        /// </summary>
        /// <param name="result">The result from MediatR operation</param>
        /// <param name="successStatusCode">HTTP status code for success (default: 200)</param>
        /// <returns>Appropriate HTTP response</returns>
        protected IActionResult HandleResult(Result result, int successStatusCode = 200)
        {
            if (result.IsSuccess)
            {
                return StatusCode(successStatusCode);
            }

            return BadRequest(new ErrorResponseModel
            {
                Error = StringMessages.OperationFailed,
                ErrorDescription = result.Error ?? StringMessages.UnknownErrorOccurred
            });
        }

        /// <summary>
        /// Reads XML content from a stream
        /// </summary>
        /// <param name="bodyContent">The stream containing XML content</param>
        /// <returns>XML content as string</returns>
        protected async Task<string> GetXmlBodyAsync(Stream bodyContent)
        {
            ArgumentNullException.ThrowIfNull(bodyContent);

            using var reader = new StreamReader(bodyContent, Encoding.UTF8);
            return await reader.ReadToEndAsync();
        }
    }
}
