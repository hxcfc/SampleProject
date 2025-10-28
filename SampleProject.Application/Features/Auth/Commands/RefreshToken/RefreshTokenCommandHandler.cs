using SampleProject.Application.Interfaces;
using SampleProject.Application.Interfaces.SampleProject.Authorization;
using SampleProject.Application.Dto;
using SampleProject.Domain.Common;
using SampleProject.Domain.Responses;
using SampleProject.Domain.Enums;
using SampleProject.Domain.Entities;
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

                // Validate refresh token and get user information with security checks
                var validationResult = await ValidateRefreshTokenWithSecurityAsync(refreshToken);
                if (!validationResult.IsSuccess)
                {
                    return Result<TokenResponse>.Failure(validationResult.Error ?? StringMessages.InvalidRefreshToken);
                }

                var user = validationResult.Value!;

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

        /// <summary>
        /// Validates refresh token with security checks including reuse attack detection
        /// </summary>
        /// <param name="refreshToken">Refresh token to validate</param>
        /// <returns>Validation result with user data or error message</returns>
        private async Task<Result<UserDto>> ValidateRefreshTokenWithSecurityAsync(string refreshToken)
        {
            try
            {
                // Get user entity with refresh token details for security checks
                var userEntity = await GetUserEntityByRefreshTokenAsync(refreshToken);
                if (userEntity == null)
                {
                    _logger.LogWarning(StringMessages.InvalidOrExpiredRefreshToken);
                    return Result<UserDto>.Failure(StringMessages.InvalidRefreshToken);
                }

                // Check if refresh token has been used before (reuse attack detection)
                if (userEntity.RefreshTokenUseCount > 0)
                {
                    _logger.LogWarning("Refresh token reuse attack detected for user: {UserId}. Revoking all tokens.", userEntity.Id);
                    
                    // Revoke all refresh tokens for security
                    await _authorization.RevokeAllRefreshTokensAsync(userEntity.Id);
                    
                    return Result<UserDto>.Failure("Security breach detected. Please log in again.");
                }

                // Map to DTO
                var userDto = new UserDto
                {
                    Id = userEntity.Id,
                    Email = userEntity.Email,
                    FirstName = userEntity.FirstName,
                    LastName = userEntity.LastName,
                    IsActive = userEntity.IsActive,
                    IsEmailVerified = userEntity.IsEmailVerified,
                    LastLoginAt = userEntity.LastLoginAt,
                    CreatedAt = userEntity.CreatedAt,
                    UpdatedAt = userEntity.UpdatedAt,
                    Role = userEntity.Role
                };

                _logger.LogInformation("Refresh token validated successfully for user: {UserId}", userEntity.Id);
                return Result<UserDto>.Success(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while validating refresh token with security checks");
                return Result<UserDto>.Failure(StringMessages.ErrorDuringTokenRefresh);
            }
        }

        /// <summary>
        /// Gets user entity by refresh token for security validation
        /// </summary>
        /// <param name="refreshToken">Refresh token</param>
        /// <returns>User entity or null if not found</returns>
        private async Task<UserEntity?> GetUserEntityByRefreshTokenAsync(string refreshToken)
        {
            // This method should be implemented in IAuthorization interface
            // For now, we'll use the existing ValidateRefreshTokenAsync but we need to modify it
            // to return the entity instead of DTO for security checks
            return await _authorization.GetUserEntityByRefreshTokenAsync(refreshToken);
        }
    }
}