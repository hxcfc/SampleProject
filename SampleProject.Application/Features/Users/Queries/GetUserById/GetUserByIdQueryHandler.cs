using MediatR;
using Microsoft.Extensions.Logging;
using SampleProject.Application.Interfaces;
using SampleProject.Domain.Common;
using SampleProject.Application.Dto;

namespace SampleProject.Application.Features.Users.Queries.GetUserById
{
    /// <summary>
    /// Handler for getting a user by ID
    /// </summary>
    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
    {
        private readonly IUserService _userService;
        private readonly ILogger<GetUserByIdQueryHandler> _logger;

        public GetUserByIdQueryHandler(
            IUserService userService,
            ILogger<GetUserByIdQueryHandler> logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Check if UserId is empty
                if (request.UserId == Guid.Empty)
                {
                    _logger.LogWarning(StringMessages.InvalidUserIdProvided);
                    return Result<UserDto>.Failure(StringMessages.InvalidUserId);
                }

                _logger.LogInformation(StringMessages.AttemptingToGetUserById, request.UserId);

                // Use UserService to get user
                var result = await _userService.GetUserByIdAsync(request.UserId);

                if (result.IsSuccess)
                {
                    _logger.LogInformation(StringMessages.UserRetrievedSuccessfully, request.UserId);
                }
                else
                {
                    _logger.LogWarning(StringMessages.UserNotFoundForId, request.UserId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, StringMessages.ErrorOccurredWhileGettingUserById, request.UserId);
                return Result<UserDto>.Failure(StringMessages.ErrorOccurredWhileGettingUserById);
            }
        }
    }
}