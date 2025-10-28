using FluentValidation.Results;

namespace SampleProject.Application.Exceptions
{
    /// <summary>
    /// Custom validation exception with enhanced error details
    /// </summary>
    public class ValidationException : Exception
    {
        /// <summary>
        /// Validation errors grouped by property name
        /// </summary>
        public Dictionary<string, string[]> Errors { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class
        /// </summary>
        public ValidationException() : base("One or more validation failures have occurred.")
        {
            Errors = new Dictionary<string, string[]>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class with validation failures
        /// </summary>
        /// <param name="failures">Collection of validation failures</param>
        public ValidationException(IEnumerable<FluentValidation.Results.ValidationFailure> failures) : this()
        {
            Errors = failures
                .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class with errors dictionary
        /// </summary>
        /// <param name="errors">Dictionary of property names and their error messages</param>
        public ValidationException(Dictionary<string, string[]> errors) : this()
        {
            Errors = errors;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class with a specified error message
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        public ValidationException(string message) : base(message)
        {
            Errors = new Dictionary<string, string[]>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class with a specified error message and inner exception
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        /// <param name="innerException">The exception that is the cause of the current exception</param>
        public ValidationException(string message, Exception innerException) : base(message, innerException)
        {
            Errors = new Dictionary<string, string[]>();
        }
    }
}
