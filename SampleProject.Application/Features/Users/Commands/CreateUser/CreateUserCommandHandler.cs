using MediatR;
using Microsoft.Extensions.Logging;
using SampleProject.Application.Interfaces;
using SampleProject.Application.Interfaces.SampleProject.Authorization;
using SampleProject.Domain.Common;
using SampleProject.Domain.Dto;
using SampleProject.Domain.Entities;
using SampleProject.Domain.Enums;

namespace SampleProject.Application.Features.Users.Commands.CreateUser
{
    /// <summary>
    /// Handler for creating a new user (registration)
    /// </summary>
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<UserDto>>
    {
        private readonly IUserService _userService;
        private readonly ILogger<CreateUserCommandHandler> _logger;

        public CreateUserCommandHandler(
            IUserService userService,
            ILogger<CreateUserCommandHandler> logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(StringMessages.AttemptingUserRegistration, request.Email);

                // Use UserService to create user
                var result = await _userService.CreateUserAsync(
                    request.Email,
                    request.FirstName,
                    request.LastName,
                    request.Password);

                if (result.IsSuccess)
                {
                    _logger.LogInformation(StringMessages.UserRegisteredSuccessfully, result.Value!.Id, request.Email);
                }
                else
                {
                    _logger.LogWarning(StringMessages.FailedToCreateUser, request.Email);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, StringMessages.ErrorOccurredDuringUserRegistration, request.Email);
                return Result<UserDto>.Failure(StringMessages.ErrorOccurredDuringUserRegistration);
            }
        }
    }
}