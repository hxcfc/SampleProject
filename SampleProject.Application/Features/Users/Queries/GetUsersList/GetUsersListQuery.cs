using MediatR;
using SampleProject.Domain.Common;
using SampleProject.Application.Dto;

namespace SampleProject.Application.Features.Users.Queries.GetUsersList
{
    /// <summary>
    /// Query to get paginated list of users with filters
    /// </summary>
    public class GetUsersListQuery : IRequest<Result<PagedResult<UserDto>>>
    {
        /// <summary>
        /// Page number (1-based)
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// Number of items per page
        /// </summary>
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// Search filters
        /// </summary>
        public UserFilters Filters { get; set; } = new();

        /// <summary>
        /// Initializes a new instance of GetUsersListQuery
        /// </summary>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="filters">Search filters</param>
        public GetUsersListQuery(int page = 1, int pageSize = 10, UserFilters? filters = null)
        {
            Page = page;
            PageSize = pageSize;
            Filters = filters ?? new UserFilters();
        }
    }
}
