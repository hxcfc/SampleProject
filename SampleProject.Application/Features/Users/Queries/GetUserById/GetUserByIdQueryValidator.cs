using FluentValidation;
using SampleProject.Application.Common;

namespace SampleProject.Application.Features.Users.Queries.GetUserById
{
    /// <summary>
    /// Validator for GetUserByIdQuery
    /// </summary>
    public class GetUserByIdQueryValidator : BaseValidator<GetUserByIdQuery>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetUserByIdQueryValidator"/> class
        /// </summary>
        public GetUserByIdQueryValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User ID is required")
                .Must(BeValidGuid)
                .WithMessage("User ID cannot be empty GUID");
        }
    }
}
