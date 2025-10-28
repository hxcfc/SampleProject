using MediatR;
using Microsoft.Extensions.Logging;
using SampleProject.Application.Interfaces;
using SampleProject.Domain.Common;
using SampleProject.Application.Dto;
using Common.Shared;

namespace SampleProject.Application.Features.Users.Queries.GetUsersList
{
    /// <summary>
    /// Handler for getting paginated list of users
    /// </summary>
    public class GetUsersListQueryHandler : IRequestHandler<GetUsersListQuery, Result<PagedResult<UserDto>>>
    {
        private readonly IUserService _userService;
        private readonly ILogger<GetUsersListQueryHandler> _logger;

        public GetUsersListQueryHandler(
            IUserService userService,
            ILogger<GetUsersListQueryHandler> logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<PagedResult<UserDto>>> Handle(GetUsersListQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(StringMessages.GettingUsersList, request.Page, request.PageSize);

                var result = await _userService.GetUsersListAsync(
                    request.Page,
                    request.PageSize,
                    request.Filters);

                if (result.IsSuccess)
                {
                    _logger.LogInformation(StringMessages.UsersListRetrieved, result.Value!.Items.Count, result.Value.TotalCount);
                }
                else
                {
                    _logger.LogWarning(StringMessages.FailedToGetUsersList);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, StringMessages.ErrorOccurredWhileGettingUsersList);
                return Result<PagedResult<UserDto>>.Failure(StringMessages.ErrorOccurredWhileGettingUsersList);
            }
        }
    }
}
