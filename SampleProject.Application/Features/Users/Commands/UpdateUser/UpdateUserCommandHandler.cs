using MediatR;
using Microsoft.Extensions.Logging;
using SampleProject.Application.Interfaces;
using SampleProject.Domain.Common;
using SampleProject.Application.Dto;
using Common.Shared;

namespace SampleProject.Application.Features.Users.Commands.UpdateUser
{
    /// <summary>
    /// Handler for updating user information
    /// </summary>
    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result<UserDto>>
    {
        private readonly IUserService _userService;
        private readonly ILogger<UpdateUserCommandHandler> _logger;

        public UpdateUserCommandHandler(
            IUserService userService,
            ILogger<UpdateUserCommandHandler> logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<UserDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(StringMessages.UpdatingUser, request.UserId);

                var result = await _userService.UpdateUserAsync(
                    request.UserId,
                    request.FirstName,
                    request.LastName,
                    request.Email,
                    request.IsActive,
                    request.IsEmailVerified,
                    null); // Role is changed via separate endpoint

                if (result.IsSuccess)
                {
                    _logger.LogInformation(StringMessages.UserUpdatedSuccessfully, request.UserId);
                }
                else
                {
                    _logger.LogWarning(StringMessages.FailedToUpdateUser, request.UserId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, StringMessages.ErrorOccurredWhileUpdatingUser, request.UserId);
                return Result<UserDto>.Failure(StringMessages.ErrorOccurredWhileUpdatingUser);
            }
        }
    }
}
