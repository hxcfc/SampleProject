namespace Common.Shared.Interfaces
{
    /// <summary>
    /// Cache service interface
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Gets a value from cache
        /// </summary>
        /// <typeparam name="T">Type of value</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Cached value or default</returns>
        Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets a value in cache
        /// </summary>
        /// <typeparam name="T">Type of value</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="value">Value to cache</param>
        /// <param name="expiration">Expiration time</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes a value from cache
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        Task RemoveAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes values from cache by pattern
        /// </summary>
        /// <param name="pattern">Pattern to match</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);
    }
}
