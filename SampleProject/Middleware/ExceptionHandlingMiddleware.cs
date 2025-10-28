using Common.Shared.Exceptions;

namespace SampleProject.Middleware
{
    /// <summary>
    /// Middleware for handling exceptions globally
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly RequestDelegate _next;

        /// <summary>
        /// Initializes a new instance of the ExceptionHandlingMiddleware class
        /// </summary>
        /// <param name="next">Next middleware in the pipeline</param>
        /// <param name="logger">Logger instance</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null</exception>
        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Invokes the middleware
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <returns>Task</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (OperationCanceledException)
            {
                // Client disconnected or request was aborted (includes TaskCanceledException)
                // This is normal behavior, don't log as error
                _logger.LogDebug("Request was cancelled: {Path}", context.Request.Path);

                // Don't try to write response if client disconnected
                // Just rethrow and let framework handle cleanup
                throw;
            }
            catch (Exception ex)
            {
                // Don't log errors for static files (favicon, etc.)
                var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
                if (path.Contains("favicon") || path.Contains(".ico") || path.Contains(".png"))
                {
                    _logger.LogDebug("Static file not found: {Path}", context.Request.Path);
                    context.Response.StatusCode = 404;
                    return;
                }

                _logger.LogError(ex, StringMessages.UnhandledExceptionOccurred);
                await HandleExceptionAsync(context, ex);
            }
        }

        /// <summary>
        /// Handles the exception and returns appropriate response using RFC 7807 ProblemDetails
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <param name="exception">Exception that occurred</param>
        /// <returns>Task</returns>
        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/problem+json";

            var traceId = context.TraceIdentifier;
            SampleProject.Domain.Responses.ProblemDetails problemDetails;

            switch (exception)
            {
                case BadRequestException ex:
                    problemDetails = ProblemDetailsFactory.CreateBadRequestProblem(ex.Message, traceId);
                    break;

                case UnauthorizedException ex:
                    problemDetails = ProblemDetailsFactory.CreateUnauthorizedProblem(ex.Message, traceId);
                    break;

                case ForbiddenException ex:
                    problemDetails = ProblemDetailsFactory.CreateForbiddenProblem(ex.Message, traceId);
                    break;

                case NotFoundException ex:
                    problemDetails = ProblemDetailsFactory.CreateNotFoundProblem(ex.Message, traceId);
                    break;

                case DbBadRequestException ex:
                    problemDetails = ProblemDetailsFactory.CreateDatabaseErrorProblem(ex.Message, traceId);
                    break;

                case ValidationException ex:
                    var validationErrors = ex.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray()
                        );
                    problemDetails = ProblemDetailsFactory.CreateValidationProblem(validationErrors, traceId);
                    break;

                default:
                    problemDetails = ProblemDetailsFactory.CreateInternalServerErrorProblem(
                        StringMessages.UnexpectedErrorOccurred, traceId);
                    break;
            }

            // Only modify response if it hasn't started yet and response body is empty
            var canModifyResponse = !context.Response.HasStarted;
            var isBodyEmpty = true;

            try
            {
                isBodyEmpty = context.Response.Body.CanRead && context.Response.Body.Length == 0;
            }
            catch (ObjectDisposedException)
            {
                // If stream is disposed, consider it as not empty to avoid modification
                isBodyEmpty = false;
            }

            if (canModifyResponse && isBodyEmpty)
            {
                context.Response.StatusCode = problemDetails.Status ?? 500;

                var jsonResponse = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                });

                // Check if response stream is still available before writing
                if (context.Response.Body.CanWrite)
                {
                    await context.Response.WriteAsync(jsonResponse);
                }
            }
        }
    }
}