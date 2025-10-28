namespace Common.Shared.Interfaces
{
    /// <summary>
    /// Security headers service interface
    /// </summary>
    public interface ISecurityHeadersService
    {
        /// <summary>
        /// Applies security headers to HTTP response
        /// </summary>
        /// <param name="response">HTTP response object</param>
        void ApplySecurityHeaders(object response);
    }
}
