using MediatR;
using Microsoft.Extensions.Logging;
using SampleProject.Application.Interfaces;
using SampleProject.Domain.Common;

namespace SampleProject.Application.Features.Users.Queries.CheckEmailAvailability
{
    /// <summary>
    /// Handler for checking email availability
    /// </summary>
    public class CheckEmailAvailabilityQueryHandler : IRequestHandler<CheckEmailAvailabilityQuery, Result<bool>>
    {
        private readonly IUserService _userService;
        private readonly ILogger<CheckEmailAvailabilityQueryHandler> _logger;

        public CheckEmailAvailabilityQueryHandler(
            IUserService userService,
            ILogger<CheckEmailAvailabilityQueryHandler> logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<bool>> Handle(CheckEmailAvailabilityQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(StringMessages.CheckingEmailAvailability, request.Email);

                // Check if user exists with this email
                var userExists = await _userService.UserExistsAsync(request.Email);
                
                // Email is available if user doesn't exist
                var isAvailable = !userExists;

                _logger.LogInformation(StringMessages.EmailAvailabilityChecked, request.Email, isAvailable);
                
                return Result<bool>.Success(isAvailable);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, StringMessages.ErrorOccurredWhileCheckingEmailAvailability, request.Email);
                return Result<bool>.Failure(StringMessages.ErrorOccurredWhileCheckingEmailAvailability);
            }
        }
    }
}
