using SampleProject.Application.Interfaces;
using SampleProject.Application.Interfaces.SampleProject.Authorization;
using SampleProject.Domain.Common;
using SampleProject.Domain.Responses;

namespace SampleProject.Application.Features.Auth.Commands.Login
{
    /// <summary>
    /// Handler for user login command
    /// </summary>
    public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<TokenResponse>>
    {
        private readonly IAuthorization _authorization;
        private readonly IJwtService _jwtService;
        private readonly ILogger<LoginCommandHandler> _logger;

        public LoginCommandHandler(
            IAuthorization authorization,
            IJwtService jwtService,
            ILogger<LoginCommandHandler> logger)
        {
            _authorization = authorization ?? throw new ArgumentNullException(nameof(authorization));
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<TokenResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(StringMessages.AttemptingLoginForEmail, request.Email);

                // Validate credentials
                var user = await _authorization.ValidateCredentialsAsync(request.Email, request.Password, cancellationToken);
                if (user == null)
                {
                    _logger.LogWarning(StringMessages.InvalidCredentialsProvidedForEmail, request.Email);
                    return Result<TokenResponse>.Failure(StringMessages.InvalidEmailOrPassword);
                }

                // Check if user is active
                if (!user.IsActive)
                {
                    _logger.LogWarning("Account is not active for email: {Email}", request.Email);
                    return Result<TokenResponse>.Failure(StringMessages.AccountNotActive);
                }

                // Generate JWT token using JwtService
                var tokenResponse = await _jwtService.GenerateTokenAsync(
                    user.Id.ToString(),
                    user.Email, // username is email in this case
                    user.Email,
                    user.FirstName,
                    user.LastName,
                    user.Role);

                // Save refresh token to database
                var refreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // 7 days expiry
                var refreshTokenSaved = await _authorization.SaveRefreshTokenAsync(
                    user.Id, 
                    tokenResponse.RefreshToken, 
                    refreshTokenExpiryTime);

                if (!refreshTokenSaved)
                {
                    _logger.LogWarning(StringMessages.FailedToSaveRefreshTokenForUser, user.Id);
                    // Continue with login even if refresh token save fails
                }

                _logger.LogInformation(StringMessages.LoginSuccessfulForUser, user.Id);
                return Result<TokenResponse>.Success(tokenResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, StringMessages.ErrorOccurredDuringLoginForEmail, request.Email);
                return Result<TokenResponse>.Failure(StringMessages.ErrorDuringLogin);
            }
        }
    }
}