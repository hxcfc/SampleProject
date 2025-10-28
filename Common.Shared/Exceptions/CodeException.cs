namespace Common.Shared.Exceptions
{
    /// <summary>
    /// Base exception for code-related errors
    /// </summary>
    public class CodeException : Exception
    {
        /// <summary>
        /// Error code associated with this exception
        /// </summary>
        public int ErrorCode { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeException"/> class
        /// </summary>
        public CodeException() : base()
        {
            ErrorCode = 500;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeException"/> class with a specified error message
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        public CodeException(string message) : base(message)
        {
            ErrorCode = 500;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeException"/> class with a specified error message and error code
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        /// <param name="errorCode">The error code associated with this exception</param>
        public CodeException(string message, int errorCode) : base(message)
        {
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeException"/> class with a specified error message, error code, and inner exception
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        /// <param name="errorCode">The error code associated with this exception</param>
        /// <param name="innerException">The exception that is the cause of the current exception</param>
        public CodeException(string message, int errorCode, Exception innerException) : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
