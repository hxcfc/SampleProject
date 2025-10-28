namespace SampleProject.Domain.Common
{
    /// <summary>
    /// Generic result pattern for handling success and failure scenarios
    /// </summary>
    /// <typeparam name="T">The type of data returned on success</typeparam>
    public class Result<T>
    {
        private Result(bool isSuccess, T? value, string? error)
        {
            IsSuccess = isSuccess;
            Value = value;
            Error = error;
        }

        /// <summary>
        /// Error message on failure
        /// </summary>
        public string? Error { get; }

        /// <summary>
        /// Indicates whether the operation was successful
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// The data returned on success
        /// </summary>
        public T? Value { get; }

        /// <summary>
        /// Creates a failed result with error message
        /// </summary>
        /// <param name="error">The error message</param>
        /// <returns>Failed result</returns>
        public static Result<T> Failure(string error) => new(false, default, error);

        /// <summary>
        /// Implicit conversion to bool for easy success checking
        /// </summary>
        /// <param name="result">The result to convert</param>
        /// <returns>True if successful, false otherwise</returns>
        public static implicit operator bool(Result<T> result) => result.IsSuccess;

        /// <summary>
        /// Creates a successful result with data
        /// </summary>
        /// <param name="value">The data to return</param>
        /// <returns>Successful result</returns>
        public static Result<T> Success(T value) => new(true, value, null);
    }

    /// <summary>
    /// Non-generic result for operations that don't return data
    /// </summary>
    public class Result
    {
        private Result(bool isSuccess, string? error)
        {
            IsSuccess = isSuccess;
            Error = error;
        }

        /// <summary>
        /// Error message on failure
        /// </summary>
        public string? Error { get; }

        /// <summary>
        /// Indicates whether the operation was successful
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// Creates a failed result with error message
        /// </summary>
        /// <param name="error">The error message</param>
        /// <returns>Failed result</returns>
        public static Result Failure(string error) => new(false, error);

        /// <summary>
        /// Implicit conversion to bool for easy success checking
        /// </summary>
        /// <param name="result">The result to convert</param>
        /// <returns>True if successful, false otherwise</returns>
        public static implicit operator bool(Result result) => result.IsSuccess;

        /// <summary>
        /// Creates a successful result
        /// </summary>
        /// <returns>Successful result</returns>
        public static Result Success() => new(true, null);
    }
}