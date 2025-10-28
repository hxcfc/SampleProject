﻿namespace Common.Shared.Exceptions
{
    /// <summary>
    /// Exception thrown when a bad request is made
    /// </summary>
    public class BadRequestException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BadRequestException"/> class
        /// </summary>
        public BadRequestException() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BadRequestException"/> class with a specified error message
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        public BadRequestException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BadRequestException"/> class with a specified error message and inner exception
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        /// <param name="innerException">The exception that is the cause of the current exception</param>
        public BadRequestException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}