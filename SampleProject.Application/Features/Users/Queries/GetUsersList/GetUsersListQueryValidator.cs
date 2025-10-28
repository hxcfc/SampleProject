using FluentValidation;

namespace SampleProject.Application.Features.Users.Queries.GetUsersList
{
    /// <summary>
    /// Validator for GetUsersListQuery
    /// </summary>
    public class GetUsersListQueryValidator : AbstractValidator<GetUsersListQuery>
    {
        public GetUsersListQueryValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThan(0)
                .WithMessage("Page must be greater than 0");

            RuleFor(x => x.PageSize)
                .GreaterThan(0)
                .WithMessage("Page size must be greater than 0")
                .LessThanOrEqualTo(100)
                .WithMessage("Page size cannot exceed 100");

            RuleFor(x => x.Filters.SearchTerm)
                .MaximumLength(100)
                .WithMessage("Search term cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.Filters.SearchTerm));

            RuleFor(x => x.Filters.SortBy)
                .Must(BeValidSortField)
                .WithMessage("Invalid sort field. Valid fields: FirstName, LastName, Email, CreatedAt, IsActive, IsEmailVerified")
                .When(x => !string.IsNullOrEmpty(x.Filters.SortBy));

            RuleFor(x => x.Filters.SortDirection)
                .Must(BeValidSortDirection)
                .WithMessage("Sort direction must be 'asc' or 'desc'")
                .When(x => !string.IsNullOrEmpty(x.Filters.SortDirection));

            RuleFor(x => x.Filters.CreatedFrom)
                .LessThanOrEqualTo(x => x.Filters.CreatedTo)
                .WithMessage("CreatedFrom must be less than or equal to CreatedTo")
                .When(x => x.Filters.CreatedFrom.HasValue && x.Filters.CreatedTo.HasValue);
        }

        private static bool BeValidSortField(string? sortField)
        {
            if (string.IsNullOrEmpty(sortField))
                return true;

            var validFields = new[] { "FirstName", "LastName", "Email", "CreatedAt", "IsActive", "IsEmailVerified" };
            return validFields.Contains(sortField, StringComparer.OrdinalIgnoreCase);
        }

        private static bool BeValidSortDirection(string? sortDirection)
        {
            if (string.IsNullOrEmpty(sortDirection))
                return true;

            return sortDirection.Equals("asc", StringComparison.OrdinalIgnoreCase) ||
                   sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);
        }
    }
}
