namespace SampleProject.Domain.Responses
{
    /// <summary>
    /// Factory for creating standardized ProblemDetails instances
    /// </summary>
    public static class ProblemDetailsFactory
    {
        /// <summary>
        /// Creates a ProblemDetails for bad request errors
        /// </summary>
        /// <param name="detail">Error detail message</param>
        /// <param name="traceId">Optional trace identifier</param>
        /// <returns>ProblemDetails instance</returns>
        public static ProblemDetails CreateBadRequestProblem(
            string detail,
            string? traceId = null)
        {
            return new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Bad Request",
                Status = 400,
                Detail = detail,
                TraceId = traceId,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Creates a ProblemDetails for database errors
        /// </summary>
        /// <param name="detail">Error detail message</param>
        /// <param name="traceId">Optional trace identifier</param>
        /// <returns>ProblemDetails instance</returns>
        public static ProblemDetails CreateDatabaseErrorProblem(
            string detail,
            string? traceId = null)
        {
            return new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Database Error",
                Status = 400,
                Detail = detail,
                TraceId = traceId,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Creates a ProblemDetails for forbidden errors
        /// </summary>
        /// <param name="detail">Error detail message</param>
        /// <param name="traceId">Optional trace identifier</param>
        /// <returns>ProblemDetails instance</returns>
        public static ProblemDetails CreateForbiddenProblem(
            string detail,
            string? traceId = null)
        {
            return new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                Title = "Forbidden",
                Status = 403,
                Detail = detail,
                TraceId = traceId,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Creates a ProblemDetails for internal server errors
        /// </summary>
        /// <param name="detail">Error detail message</param>
        /// <param name="traceId">Optional trace identifier</param>
        /// <returns>ProblemDetails instance</returns>
        public static ProblemDetails CreateInternalServerErrorProblem(
            string detail,
            string? traceId = null)
        {
            return new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                Title = "Internal Server Error",
                Status = 500,
                Detail = detail,
                TraceId = traceId,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Creates a ProblemDetails for not found errors
        /// </summary>
        /// <param name="detail">Error detail message</param>
        /// <param name="traceId">Optional trace identifier</param>
        /// <returns>ProblemDetails instance</returns>
        public static ProblemDetails CreateNotFoundProblem(
            string detail,
            string? traceId = null)
        {
            return new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                Title = "Not Found",
                Status = 404,
                Detail = detail,
                TraceId = traceId,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Creates a ProblemDetails for unauthorized errors
        /// </summary>
        /// <param name="detail">Error detail message</param>
        /// <param name="traceId">Optional trace identifier</param>
        /// <returns>ProblemDetails instance</returns>
        public static ProblemDetails CreateUnauthorizedProblem(
            string detail,
            string? traceId = null)
        {
            return new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                Title = "Unauthorized",
                Status = 401,
                Detail = detail,
                TraceId = traceId,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Creates a ProblemDetails for validation errors
        /// </summary>
        /// <param name="errors">Dictionary of field names and their validation errors</param>
        /// <param name="traceId">Optional trace identifier</param>
        /// <returns>ProblemDetails instance</returns>
        public static ProblemDetails CreateValidationProblem(
            Dictionary<string, string[]> errors,
            string? traceId = null)
        {
            return new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "One or more validation errors occurred",
                Status = 400,
                Detail = "Please refer to the errors property for additional details",
                Errors = errors,
                TraceId = traceId,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// RFC 7807 Problem Details for HTTP APIs implementation
    /// Provides a standardized way to represent error information in HTTP responses
    /// </summary>
    public class ProblemDetails
    {
        /// <summary>
        /// A human-readable explanation specific to this occurrence of the problem
        /// </summary>
        [JsonPropertyName("detail")]
        public string? Detail { get; set; }

        /// <summary>
        /// Validation errors for validation failures
        /// </summary>
        [JsonPropertyName("errors")]
        public Dictionary<string, string[]>? Errors { get; set; }

        /// <summary>
        /// Additional properties that provide more information about the problem
        /// </summary>
        [JsonPropertyName("extensions")]
        public Dictionary<string, object>? Extensions { get; set; }

        /// <summary>
        /// A URI reference that identifies the specific occurrence of the problem
        /// </summary>
        [JsonPropertyName("instance")]
        public string? Instance { get; set; }

        /// <summary>
        /// The HTTP status code
        /// </summary>
        [JsonPropertyName("status")]
        public int? Status { get; set; }

        /// <summary>
        /// Timestamp when the problem occurred
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTime? Timestamp { get; set; }

        /// <summary>
        /// A short, human-readable summary of the problem type
        /// </summary>
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        /// <summary>
        /// Trace identifier for correlation
        /// </summary>
        [JsonPropertyName("traceId")]
        public string? TraceId { get; set; }

        /// <summary>
        /// A URI reference that identifies the problem type
        /// </summary>
        [JsonPropertyName("type")]
        public string? Type { get; set; }
    }
}