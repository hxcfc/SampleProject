using SampleProject.Application.Features.Users.Commands.ChangePassword;
using SampleProject.Application.Features.Users.Commands.ChangeUserRole;
using SampleProject.Application.Features.Users.Commands.CreateUser;
using SampleProject.Application.Features.Users.Commands.UpdateUser;
using SampleProject.Application.Features.Users.Queries.CheckEmailAvailability;
using SampleProject.Application.Features.Users.Queries.GetUserById;
using SampleProject.Application.Features.Users.Queries.GetUsersList;
using SampleProject.Application.Dto;
using SampleProject.Domain.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace SampleProject.Controllers.Users
{
    /// <summary>
    /// Controller for managing users
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/users")]
    [Produces("application/json")]
    [ApiExplorerSettings(GroupName = "v1")]
    [SwaggerTag("User management operations - registration, profile management, and administration")]
    public class UsersController : BaseController
    {
        private readonly ICurrentUserService _currentUserService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UsersController"/> class
        /// </summary>
        /// <param name="mediator">MediatR mediator instance</param>
        /// <param name="currentUserService">Current user service</param>
        public UsersController(IMediator mediator, ICurrentUserService currentUserService) : base(mediator)
        {
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        /// <summary>
        /// Changes current user's password
        /// </summary>
        /// <param name="command">Password change command</param>
        /// <returns>Success status</returns>
        [HttpPost("me/change-password")]
        [Authorize(Roles = "User")]
        [SwaggerOperation(
            Summary = "Change my password",
            Description = "Changes current user's password. Requires current password verification.",
            OperationId = "ChangeMyPassword",
            Tags = new[] { "Users" }
        )]
        [SwaggerResponse(200, "Password changed successfully", typeof(bool))]
        [SwaggerResponse(401, "Unauthorized", typeof(ErrorResponseModel))]
        [SwaggerResponse(404, "User not found", typeof(ErrorResponseModel))]
        [SwaggerResponse(400, "Invalid input data", typeof(ErrorResponseModel))]
        [SwaggerResponse(500, "Internal server error", typeof(ErrorResponseModel))]
        public async Task<IActionResult> ChangeMyPassword([FromBody] ChangePasswordRequest request)
        {
            // Get current user ID from JWT token
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId) || !Guid.TryParse(currentUserId, out var userId))
            {
                return Unauthorized(new ErrorResponseModel
                {
                    Error = StringMessages.Unauthorized,
                    ErrorDescription = StringMessages.InvalidToken
                });
            }

            var command = new ChangePasswordCommand
            {
                UserId = userId,
                CurrentPassword = request.CurrentPassword,
                NewPassword = request.NewPassword
            };
            var result = await Mediator.Send(command);

            if (!result.IsSuccess)
            {
                return BadRequest(new ErrorResponseModel
                {
                    Error = StringMessages.ValidationError,
                    ErrorDescription = result.Error ?? StringMessages.UnknownErrorOccurred
                });
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// Changes user password (Admin only)
        /// </summary>
        /// <param name="userId">User ID to change password for</param>
        /// <param name="command">Password change command</param>
        /// <returns>Success status</returns>
        [HttpPost("{userId}/change-password")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(
            Summary = "Change user password (Admin only)",
            Description = "Changes any user's password. Admin can change any user's password without current password verification.",
            OperationId = "ChangeUserPassword",
            Tags = new[] { "Users" }
        )]
        [SwaggerResponse(200, "Password changed successfully", typeof(bool))]
        [SwaggerResponse(401, "Unauthorized", typeof(ErrorResponseModel))]
        [SwaggerResponse(403, "Forbidden - Admin role required", typeof(ErrorResponseModel))]
        [SwaggerResponse(404, "User not found", typeof(ErrorResponseModel))]
        [SwaggerResponse(400, "Invalid input data", typeof(ErrorResponseModel))]
        [SwaggerResponse(500, "Internal server error", typeof(ErrorResponseModel))]
        public async Task<IActionResult> ChangeUserPassword(Guid userId, [FromBody] ChangePasswordRequest request)
        {
            var command = new ChangePasswordCommand
            {
                UserId = userId,
                CurrentPassword = request.CurrentPassword,
                NewPassword = request.NewPassword
            };
            var result = await Mediator.Send(command);

            if (!result.IsSuccess)
            {
                return BadRequest(new ErrorResponseModel
                {
                    Error = StringMessages.ValidationError,
                    ErrorDescription = result.Error ?? StringMessages.UnknownErrorOccurred
                });
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// Changes user role (Admin only)
        /// </summary>
        /// <param name="userId">User ID to change role for</param>
        /// <param name="command">Role change command</param>
        /// <returns>Updated user information</returns>
        [HttpPost("{userId}/change-role")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(
            Summary = "Change user role (Admin only)",
            Description = "Changes user role.",
            OperationId = "ChangeUserRole",
            Tags = new[] { "Users" }
        )]
        [SwaggerResponse(200, "User role changed successfully", typeof(UserDto))]
        [SwaggerResponse(401, "Unauthorized", typeof(ErrorResponseModel))]
        [SwaggerResponse(403, "Forbidden - Admin role required", typeof(ErrorResponseModel))]
        [SwaggerResponse(404, "User not found", typeof(ErrorResponseModel))]
        [SwaggerResponse(400, "Invalid input data", typeof(ErrorResponseModel))]
        [SwaggerResponse(500, "Internal server error", typeof(ErrorResponseModel))]
        public async Task<IActionResult> ChangeUserRole(Guid userId, [FromBody] ChangeUserRoleRequest request)
        {
            var command = new ChangeUserRoleCommand
            {
                UserId = userId,
                NewRole = request.NewRole
            };
            var result = await Mediator.Send(command);

            if (!result.IsSuccess)
            {
                return BadRequest(new ErrorResponseModel
                {
                    Error = StringMessages.ValidationError,
                    ErrorDescription = result.Error ?? StringMessages.UnknownErrorOccurred
                });
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// Checks if email is available for registration
        /// </summary>
        /// <param name="email">Email address to check</param>
        /// <returns>True if email is available, false if already taken</returns>
        [HttpGet("check-email/{email}")]
        [SwaggerOperation(
            Summary = "Check email availability",
            Description = "Checks if the provided email address is available for registration.",
            OperationId = "CheckEmailAvailability",
            Tags = new[] { "Users" }
        )]
        [SwaggerResponse(200, "Email availability checked successfully", typeof(bool))]
        [SwaggerResponse(400, "Invalid email format", typeof(ErrorResponseModel))]
        [SwaggerResponse(500, "Internal server error", typeof(ErrorResponseModel))]
        public async Task<IActionResult> CheckEmailAvailability(string email)
        {
            var query = new CheckEmailAvailabilityQuery(email);
            var result = await Mediator.Send(query);

            if (!result.IsSuccess)
            {
                return BadRequest(new ErrorResponseModel
                {
                    Error = StringMessages.ValidationError,
                    ErrorDescription = result.Error ?? StringMessages.UnknownErrorOccurred
                });
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// Registers a new user
        /// </summary>
        /// <param name="command">User registration command</param>
        /// <returns>Created user information</returns>
        [HttpPost]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Register a new user",
            Description = "Registers a new user with email, password, and personal information. User will be inactive until email verification (future feature).",
            OperationId = "RegisterUser",
            Tags = new[] { "Users" }
        )]
        [SwaggerResponse(201, "User registered successfully", typeof(UserDto))]
        [SwaggerResponse(400, "Invalid input data or user already exists", typeof(ErrorResponseModel))]
        [SwaggerResponse(500, "Internal server error", typeof(ErrorResponseModel))]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
        {
            var result = await Mediator.Send(command);

            if (!result.IsSuccess)
            {
                return BadRequest(new ErrorResponseModel
                {
                    Error = StringMessages.ValidationError,
                    ErrorDescription = result.Error ?? StringMessages.UnknownErrorOccurred
                });
            }

            return CreatedAtAction(nameof(GetUserById), new { userId = result.Value!.Id }, result.Value);
        }

        /// <summary>
        /// Gets a user by their ID
        /// </summary>
        /// <param name="userId">The unique identifier of the user</param>
        /// <returns>User information</returns>
        [HttpGet("{userId}")]
        [Authorize(Roles = "User,Admin")]
        [SwaggerOperation(
            Summary = "Get user by ID",
            Description = "Retrieves user information by their unique identifier. Users can only access their own data, Admins can access any user's data.",
            OperationId = "GetUserById",
            Tags = new[] { "Users" }
        )]
        [SwaggerResponse(200, "User found", typeof(UserDto))]
        [SwaggerResponse(401, "Unauthorized", typeof(ErrorResponseModel))]
        [SwaggerResponse(403, "Forbidden - insufficient permissions", typeof(ErrorResponseModel))]
        [SwaggerResponse(404, "User not found", typeof(ErrorResponseModel))]
        [SwaggerResponse(500, "Internal server error", typeof(ErrorResponseModel))]
        public async Task<IActionResult> GetUserById(Guid userId)
        {
            // Check if user is trying to access their own data or is admin
            var currentUserId = _currentUserService.GetCurrentUserId();
            var currentUserRole = _currentUserService.GetCurrentUserRole();

            if (string.IsNullOrEmpty(currentUserId) || !Guid.TryParse(currentUserId, out var currentUserIdGuid))
            {
                return Unauthorized(new ErrorResponseModel
                {
                    Error = StringMessages.Unauthorized,
                    ErrorDescription = StringMessages.InvalidToken
                });
            }

            // Check if user is trying to access their own data or is admin
            if (currentUserIdGuid != userId && !currentUserRole.Contains("Admin"))
            {
                return Forbid();
            }

            var query = new GetUserByIdQuery { UserId = userId };
            var result = await Mediator.Send(query);

            if (!result.IsSuccess)
            {
                return NotFound(new ErrorResponseModel
                {
                    Error = StringMessages.NotFound,
                    ErrorDescription = result.Error ?? StringMessages.UserNotFound
                });
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// Gets a paginated list of users with filters (Admin only)
        /// </summary>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10, max: 100)</param>
        /// <param name="searchTerm">Search term for name or email</param>
        /// <param name="role">Filter by user role</param>
        /// <param name="isActive">Filter by active status</param>
        /// <param name="isEmailVerified">Filter by email verification status</param>
        /// <param name="createdFrom">Filter by creation date from</param>
        /// <param name="createdTo">Filter by creation date to</param>
        /// <param name="sortBy">Sort field (FirstName, LastName, Email, CreatedAt, IsActive, IsEmailVerified)</param>
        /// <param name="sortDirection">Sort direction (asc/desc)</param>
        /// <returns>Paginated list of users</returns>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(
            Summary = "Get users list (Admin only)",
            Description = "Gets a paginated list of users with optional filters.",
            OperationId = "GetUsersList",
            Tags = new[] { "Users" }
        )]
        [SwaggerResponse(200, "Users list retrieved successfully", typeof(PagedResult<UserDto>))]
        [SwaggerResponse(401, "Unauthorized", typeof(ErrorResponseModel))]
        [SwaggerResponse(403, "Forbidden - Admin role required", typeof(ErrorResponseModel))]
        [SwaggerResponse(400, "Invalid parameters", typeof(ErrorResponseModel))]
        [SwaggerResponse(500, "Internal server error", typeof(ErrorResponseModel))]
        public async Task<IActionResult> GetUsersList(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] UserRole? role = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] bool? isEmailVerified = null,
            [FromQuery] DateTime? createdFrom = null,
            [FromQuery] DateTime? createdTo = null,
            [FromQuery] string? sortBy = "CreatedAt",
            [FromQuery] string? sortDirection = "desc")
        {
            var filters = new UserFilters
            {
                SearchTerm = searchTerm,
                Role = role,
                IsActive = isActive,
                IsEmailVerified = isEmailVerified,
                CreatedFrom = createdFrom,
                CreatedTo = createdTo,
                SortBy = sortBy,
                SortDirection = sortDirection
            };

            var query = new GetUsersListQuery(page, pageSize, filters);
            var result = await Mediator.Send(query);

            if (!result.IsSuccess)
            {
                return BadRequest(new ErrorResponseModel
                {
                    Error = StringMessages.ValidationError,
                    ErrorDescription = result.Error ?? StringMessages.UnknownErrorOccurred
                });
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// Updates current user's information (PATCH/PUT)
        /// </summary>
        /// <param name="command">User update command</param>
        /// <returns>Updated user information</returns>
        [HttpPut("me")]
        [HttpPatch("me")]
        [Authorize(Roles = "User")]
        [SwaggerOperation(
            Summary = "Update my profile",
            Description = "Updates current user's profile information. Password changes require separate endpoint.",
            OperationId = "UpdateMyProfile",
            Tags = new[] { "Users" }
        )]
        [SwaggerResponse(200, "Profile updated successfully", typeof(UserDto))]
        [SwaggerResponse(401, "Unauthorized", typeof(ErrorResponseModel))]
        [SwaggerResponse(404, "User not found", typeof(ErrorResponseModel))]
        [SwaggerResponse(400, "Invalid input data", typeof(ErrorResponseModel))]
        [SwaggerResponse(500, "Internal server error", typeof(ErrorResponseModel))]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateMyProfileRequest request)
        {
            // Get current user ID from JWT token
            var currentUserId = _currentUserService.GetCurrentUserId();

            if (string.IsNullOrEmpty(currentUserId) || !Guid.TryParse(currentUserId, out var currentUserIdGuid))
            {
                return Unauthorized(new ErrorResponseModel
                {
                    Error = StringMessages.Unauthorized,
                    ErrorDescription = StringMessages.InvalidToken
                });
            }

            // Create command with user ID from JWT token and restrict admin-only fields
            var command = new UpdateUserCommand
            {
                UserId = currentUserIdGuid,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                IsActive = null, // Users cannot change their own active status
                IsEmailVerified = null, // Users cannot change their own email verification status
                Role = null // Users cannot change their own role
            };

            var result = await Mediator.Send(command);

            if (!result.IsSuccess)
            {
                return BadRequest(new ErrorResponseModel
                {
                    Error = StringMessages.ValidationError,
                    ErrorDescription = result.Error ?? StringMessages.UnknownErrorOccurred
                });
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// Updates any user's information (Admin only)
        /// </summary>
        /// <param name="userId">User ID to update</param>
        /// <param name="command">User update command</param>
        /// <returns>Updated user information</returns>
        [HttpPut("{userId}")]
        [HttpPatch("{userId}")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(
            Summary = "Update user information (Admin only)",
            Description = "Updates any user's information including admin-only fields.",
            OperationId = "UpdateUser",
            Tags = new[] { "Users" }
        )]
        [SwaggerResponse(200, "User updated successfully", typeof(UserDto))]
        [SwaggerResponse(401, "Unauthorized", typeof(ErrorResponseModel))]
        [SwaggerResponse(403, "Forbidden - Admin role required", typeof(ErrorResponseModel))]
        [SwaggerResponse(404, "User not found", typeof(ErrorResponseModel))]
        [SwaggerResponse(400, "Invalid input data", typeof(ErrorResponseModel))]
        [SwaggerResponse(500, "Internal server error", typeof(ErrorResponseModel))]
        public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] UpdateUserRequest request)
        {
            var command = new UpdateUserCommand
            {
                UserId = userId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                IsActive = request.IsActive,
                IsEmailVerified = request.IsEmailVerified,
                Role = request.Role
            };
            var result = await Mediator.Send(command);

            if (!result.IsSuccess)
            {
                return BadRequest(new ErrorResponseModel
                {
                    Error = StringMessages.ValidationError,
                    ErrorDescription = result.Error ?? StringMessages.UnknownErrorOccurred
                });
            }

            return Ok(result.Value);
        }
    }
}