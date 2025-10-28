namespace Common.Shared.Exceptions
{
    /// <summary>
    /// Exception thrown when access is unauthorized
    /// </summary>
    public class UnauthorizedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnauthorizedException"/> class
        /// </summary>
        public UnauthorizedException() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnauthorizedException"/> class with a specified error message
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        public UnauthorizedException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnauthorizedException"/> class with a specified error message and inner exception
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        /// <param name="innerException">The exception that is the cause of the current exception</param>
        public UnauthorizedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}