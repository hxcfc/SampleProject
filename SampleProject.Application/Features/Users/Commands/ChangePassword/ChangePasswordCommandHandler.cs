using MediatR;
using Microsoft.Extensions.Logging;
using SampleProject.Application.Interfaces;
using SampleProject.Domain.Common;
using Common.Shared;

namespace SampleProject.Application.Features.Users.Commands.ChangePassword
{
    /// <summary>
    /// Handler for changing user password
    /// </summary>
    public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result<bool>>
    {
        private readonly IUserService _userService;
        private readonly ILogger<ChangePasswordCommandHandler> _logger;

        public ChangePasswordCommandHandler(
            IUserService userService,
            ILogger<ChangePasswordCommandHandler> logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<bool>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(StringMessages.ChangingPasswordForUser, request.UserId);

                var result = await _userService.ChangePasswordAsync(
                    request.UserId,
                    request.CurrentPassword,
                    request.NewPassword);

                if (result.IsSuccess)
                {
                    _logger.LogInformation(StringMessages.PasswordChangedSuccessfully, request.UserId);
                }
                else
                {
                    _logger.LogWarning(StringMessages.FailedToChangePassword, request.UserId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, StringMessages.ErrorOccurredWhileChangingPassword, request.UserId);
                return Result<bool>.Failure(StringMessages.ErrorOccurredWhileChangingPassword);
            }
        }
    }
}
