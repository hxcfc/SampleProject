namespace Common.Shared.Exceptions
{
    /// <summary>
    /// Exception thrown when a database operation fails
    /// </summary>
    public class DbBadRequestException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DbBadRequestException"/> class
        /// </summary>
        public DbBadRequestException() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbBadRequestException"/> class with a specified error message
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        public DbBadRequestException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbBadRequestException"/> class with a specified error message and inner exception
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        /// <param name="innerException">The exception that is the cause of the current exception</param>
        public DbBadRequestException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}