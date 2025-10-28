using SampleProject.Application.Interfaces;
using SampleProject.Application.Interfaces.SampleProject.Authorization;
using SampleProject.Domain.Common;
using SampleProject.Domain.Responses;
using SampleProject.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace SampleProject.Application.Features.Auth.Commands.RefreshToken
{
    /// <summary>
    /// Handler for refresh token command
    /// </summary>
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<TokenResponse>>
    {
        private readonly IAuthorization _authorization;
        private readonly IJwtService _jwtService;
        private readonly ILogger<RefreshTokenCommandHandler> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RefreshTokenCommandHandler(
            IAuthorization authorization,
            IJwtService jwtService,
            ILogger<RefreshTokenCommandHandler> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _authorization = authorization ?? throw new ArgumentNullException(nameof(authorization));
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public async Task<Result<TokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(StringMessages.ProcessingTokenRefreshRequest);

                // Extract refresh token from HTTP-only cookies
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null)
                {
                    _logger.LogWarning("HTTP context is not available");
                    return Result<TokenResponse>.Failure(StringMessages.InvalidRefreshToken);
                }

                var refreshToken = _jwtService.GetRefreshTokenFromCookies(httpContext.Request);
                if (string.IsNullOrWhiteSpace(refreshToken))
                {
                    _logger.LogWarning(StringMessages.InvalidRefreshTokenProvided);
                    return Result<TokenResponse>.Failure(StringMessages.InvalidRefreshToken);
                }

                // Validate refresh token and get user information
                var user = await _authorization.ValidateRefreshTokenAsync(refreshToken);
                if (user == null)
                {
                    _logger.LogWarning(StringMessages.InvalidOrExpiredRefreshToken);
                    return Result<TokenResponse>.Failure(StringMessages.InvalidRefreshToken);
                }

                // Check if user is active
                if (!user.IsActive)
                {
                    _logger.LogWarning("Account is not active for user: {UserId}", user.Id);
                    return Result<TokenResponse>.Failure(StringMessages.AccountNotActive);
                }

                // Generate new JWT token
                var tokenResponse = await _jwtService.GenerateTokenAsync(
                    user.Id.ToString(),
                    user.Email,
                    user.Email,
                    user.FirstName,
                    user.LastName,
                    user.Role);

                // Save new refresh token to database (replace old one)
                var refreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // 7 days expiry
                var refreshTokenSaved = await _authorization.SaveRefreshTokenAsync(
                    user.Id, 
                    tokenResponse.RefreshToken, 
                    refreshTokenExpiryTime);

                if (!refreshTokenSaved)
                {
                        _logger.LogWarning(StringMessages.FailedToSaveNewRefreshToken, user.Id);
                    // Continue with token refresh even if save fails
                }

                _logger.LogInformation(StringMessages.TokenRefreshedSuccessfullyForUser, user.Id);
                return Result<TokenResponse>.Success(tokenResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, StringMessages.ErrorOccurredDuringTokenRefresh);
                return Result<TokenResponse>.Failure(StringMessages.ErrorDuringTokenRefresh);
            }
        }
    }
}