using SampleProject.Application.Features.Auth.Commands.Login;
using SampleProject.Application.Features.Auth.Commands.RefreshToken;
using SampleProject.Application.Interfaces;
using SampleProject.Infrastructure.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace SampleProject.Controllers.Auth
{
    /// <summary>
    /// Authentication controller handling login, logout, and token refresh
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/auth")]
    [SwaggerTag("Authentication endpoints for user login, logout, token refresh, and user information")]
    [ApiExplorerSettings(GroupName = "v1")]
    [Produces("application/json")]
    public class AuthController : BaseController
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IExtendedJwtService _jwtService;

        /// <summary>
        /// Initializes a new instance of the AuthController class
        /// </summary>
        /// <param name="mediator">MediatR mediator</param>
        /// <param name="jwtService">JWT service</param>
        /// <param name="currentUserService">Current user service for accessing authenticated user info</param>
        public AuthController(IMediator mediator, IExtendedJwtService jwtService, ICurrentUserService currentUserService) : base(mediator)
        {
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        /// <summary>
        /// Get current authenticated user information
        /// </summary>
        /// <returns>Current user details extracted from JWT token</returns>
        /// <response code="200">User information retrieved successfully</response>
        [SwaggerOperation(
            Summary = "Get current user information",
            Description = "Retrieves information about the currently authenticated user from the JWT token. Requires valid authentication token.",
            OperationId = "GetCurrentUser"
        )]
        [SwaggerResponse(200, "User information retrieved successfully")]
        [SwaggerResponse(401, "Unauthorized - Invalid or expired token")]
        [HttpGet("me")]
        [Authorize(Policy = "RequireAuthenticatedUser")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var user = _currentUserService.GetCurrentUser();

            if (user == null)
            {
                return Unauthorized(new
                {
                    Success = false,
                    Message = StringMessages.TokenIsInvalidOrExpired
                });
            }

            return Ok(new
            {
                Success = true,
                User = user
            });
        }

        /// <summary>
        /// Authenticate user and return JWT tokens
        /// </summary>
        /// <param name="command">Login credentials (username and password)</param>
        /// <returns>JWT access and refresh tokens stored in HTTP-only cookies</returns>
        /// <response code="200">Login successful - tokens returned in cookies</response>
        [SwaggerOperation(
            Summary = "User login",
            Description = "Authenticates a user with email and password. Returns JWT access and refresh tokens stored in HTTP-only cookies. Access token expires in 60 minutes, refresh token expires in 7 days.",
            OperationId = "Login"
        )]
        [SwaggerResponse(200, "Login successful - tokens are stored in HTTP-only cookies")]
        [SwaggerResponse(400, "Invalid credentials or request data")]
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginCommand command)
        {
            var result = await Mediator.Send(command);

            if (result.IsSuccess && result.Value != null)
            {
                // Set tokens as HTTP-only cookies
                _jwtService.SetTokenCookies(Response, result.Value.AccessToken, result.Value.RefreshToken);

                // Return success response (tokens are in cookies)
                return Ok(new
                {
                    Success = true,
                    Message = StringMessages.LoginSuccessful,
                    User = new
                    {
                        Email = command.Email,
                        ExpiresAt = result.Value.ExpiresAt
                    }
                });
            }

            return BadRequest(new
            {
                Success = false,
                Message = result.Error ?? StringMessages.LoginFailed
            });
        }

        /// <summary>
        /// Logout user and clear authentication cookies
        /// </summary>
        /// <returns>Logout confirmation message</returns>
        /// <response code="200">Logout successful - cookies cleared</response>
        [SwaggerOperation(
            Summary = "User logout",
            Description = "Logs out the current user, revokes their refresh token in the database, and clears authentication cookies.",
            OperationId = "Logout"
        )]
        [SwaggerResponse(200, "Logout successful - tokens revoked and cookies cleared")]
        [SwaggerResponse(400, "Failed to logout")]
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var command = new SampleProject.Application.Features.Auth.Commands.Logout.LogoutCommand();
            var result = await Mediator.Send(command);

            if (result.IsSuccess)
            {
                // Clear token cookies
                _jwtService.ClearTokenCookies(Response);

                return Ok(new
                {
                    Success = true,
                    Message = result.Value?.Message ?? StringMessages.LogoutSuccessful
                });
            }

            return BadRequest(new
            {
                Success = false,
                Message = result.Error ?? StringMessages.LogoutFailed
            });
        }

        /// <summary>
        /// Refresh JWT access token using refresh token
        /// </summary>
        /// <param name="command">Refresh token command containing the refresh token</param>
        /// <returns>New JWT access and refresh tokens stored in HTTP-only cookies</returns>
        /// <response code="200">Token refreshed successfully - new tokens in cookies</response>
        [SwaggerOperation(
            Summary = "Refresh access token",
            Description = "Refreshes the JWT access token using a valid refresh token. Generates new access and refresh tokens and updates them in HTTP-only cookies. The refresh token is validated against the database and its usage count is incremented.",
            OperationId = "RefreshToken"
        )]
        [SwaggerResponse(200, "Token refreshed successfully - new tokens stored in cookies")]
        [SwaggerResponse(400, "Invalid or expired refresh token")]
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command)
        {
            var result = await Mediator.Send(command);

            if (result.IsSuccess && result.Value != null)
            {
                // Update tokens in cookies
                _jwtService.SetTokenCookies(Response, result.Value.AccessToken, result.Value.RefreshToken);

                return Ok(new
                {
                    Success = true,
                    Message = StringMessages.TokenRefreshedSuccessfully,
                    ExpiresAt = result.Value.ExpiresAt
                });
            }

            return BadRequest(new
            {
                Success = false,
                Message = result.Error ?? StringMessages.TokenRefreshFailed
            });
        }

        /// <summary>
        /// Validate current JWT token and return user information
        /// </summary>
        /// <returns>Token validation result with user details if valid</returns>
        /// <response code="200">Token is valid - user information returned</response>
        [SwaggerOperation(
            Summary = "Validate JWT token",
            Description = "Validates the current JWT access token and returns user information if the token is valid. This endpoint can be used to check if a token is still valid without making a full authenticated request.",
            OperationId = "ValidateToken"
        )]
        [SwaggerResponse(200, "Token is valid - user information returned")]
        [SwaggerResponse(401, "Unauthorized - Invalid or expired token")]
        [HttpGet("validate")]
        [Authorize(Policy = "RequireAuthenticatedUser")]
        public async Task<IActionResult> ValidateToken()
        {
            // If we reach here, middleware already validated the token
            // Get user information from current user service
            var user = _currentUserService.GetCurrentUser();

            if (user == null)
            {
                return Unauthorized(new
                {
                    Success = false,
                    Message = StringMessages.TokenIsInvalidOrExpired
                });
            }

            return Ok(new
            {
                Success = true,
                Message = StringMessages.TokenIsValid,
                User = user
            });
        }
    }
}