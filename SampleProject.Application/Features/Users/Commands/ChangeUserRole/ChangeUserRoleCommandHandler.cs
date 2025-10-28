using MediatR;
using Microsoft.Extensions.Logging;
using SampleProject.Application.Interfaces;
using SampleProject.Domain.Common;
using SampleProject.Application.Dto;
using Common.Shared;

namespace SampleProject.Application.Features.Users.Commands.ChangeUserRole
{
    /// <summary>
    /// Handler for changing user role
    /// </summary>
    public class ChangeUserRoleCommandHandler : IRequestHandler<ChangeUserRoleCommand, Result<UserDto>>
    {
        private readonly IUserService _userService;
        private readonly ILogger<ChangeUserRoleCommandHandler> _logger;

        public ChangeUserRoleCommandHandler(
            IUserService userService,
            ILogger<ChangeUserRoleCommandHandler> logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<UserDto>> Handle(ChangeUserRoleCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(StringMessages.ChangingUserRole, request.UserId, request.NewRole);

                var result = await _userService.ChangeUserRoleAsync(request.UserId, request.NewRole);

                if (result.IsSuccess)
                {
                    _logger.LogInformation(StringMessages.UserRoleChangedSuccessfully, request.UserId, request.NewRole);
                }
                else
                {
                    _logger.LogWarning(StringMessages.FailedToChangeUserRole, request.UserId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, StringMessages.ErrorOccurredWhileChangingUserRole, request.UserId);
                return Result<UserDto>.Failure(StringMessages.ErrorOccurredWhileChangingUserRole);
            }
        }
    }
}
