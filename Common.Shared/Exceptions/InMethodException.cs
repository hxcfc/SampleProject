namespace Common.Shared.Exceptions
{
    /// <summary>
    /// Exception thrown when an error occurs within a method
    /// </summary>
    public class InMethodException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InMethodException"/> class
        /// </summary>
        public InMethodException() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InMethodException"/> class with a specified error message
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        public InMethodException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InMethodException"/> class with a specified error message and inner exception
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        /// <param name="innerException">The exception that is the cause of the current exception</param>
        public InMethodException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}