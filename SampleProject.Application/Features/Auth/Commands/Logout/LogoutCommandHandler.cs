using MediatR;
using SampleProject.Application.Interfaces;
using SampleProject.Application.Interfaces.SampleProject.Authorization;
using SampleProject.Domain.Common;

namespace SampleProject.Application.Features.Auth.Commands.Logout
{
    /// <summary>
    /// Handler for user logout
    /// </summary>
    public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result<LogoutResponse>>
    {
        private readonly IAuthorization _authorization;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<LogoutCommandHandler> _logger;

        public LogoutCommandHandler(
            IAuthorization authorization,
            ICurrentUserService currentUserService,
            ILogger<LogoutCommandHandler> logger)
        {
            _authorization = authorization ?? throw new ArgumentNullException(nameof(authorization));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<LogoutResponse>> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(StringMessages.ProcessingUserLogout);

                // Get current user to revoke their refresh token
                var currentUserId = _currentUserService.GetCurrentUserId();
                if (!string.IsNullOrEmpty(currentUserId) && Guid.TryParse(currentUserId, out var userId))
                {
                    // Revoke refresh token from database
                    var tokenRevoked = await _authorization.RevokeRefreshTokenAsync(userId);
                    if (!tokenRevoked)
                    {
                        _logger.LogWarning(StringMessages.FailedToRevokeRefreshToken, userId);
                        // Continue with logout even if token revocation fails
                    }
                    else
                    {
                        _logger.LogInformation(StringMessages.RefreshTokenRevokedSuccessfullyForUser, userId);
                    }
                }
                else
                {
                    _logger.LogWarning(StringMessages.NoCurrentUserFoundDuringLogout);
                }

                // Note: Cookie clearing will be handled in the controller
                // as we need access to HttpResponse object

                var response = new LogoutResponse
                {
                    Message = StringMessages.LogoutSuccessful
                };

                _logger.LogInformation(StringMessages.UserLogoutProcessedSuccessfully);
                return Result<LogoutResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, StringMessages.ErrorOccurredDuringLogout);
                return Result<LogoutResponse>.Failure(StringMessages.ErrorDuringLogout);
            }
        }
    }
}