namespace Common.Shared.Exceptions
{
    /// <summary>
    /// Exception thrown when a short error occurs within a method
    /// </summary>
    public class ShortInMethodException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShortInMethodException"/> class
        /// </summary>
        public ShortInMethodException() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShortInMethodException"/> class with a specified error message
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        public ShortInMethodException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShortInMethodException"/> class with a specified error message and inner exception
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        /// <param name="innerException">The exception that is the cause of the current exception</param>
        public ShortInMethodException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}