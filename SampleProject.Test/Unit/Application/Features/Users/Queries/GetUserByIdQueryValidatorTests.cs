using FluentAssertions;
using SampleProject.Application.Features.Users.Queries.GetUserById;
using Xunit;

namespace SampleProject.Test.Unit.Application.Features.Users.Queries
{
    /// <summary>
    /// Unit tests for GetUserByIdQueryValidator
    /// </summary>
    public class GetUserByIdQueryValidatorTests
    {
        private readonly GetUserByIdQueryValidator _validator;

        public GetUserByIdQueryValidatorTests()
        {
            _validator = new GetUserByIdQueryValidator();
        }

        [Fact]
        public void Validate_WithDefaultUserId_ShouldReturnInvalid()
        {
            // Arrange
            var query = new GetUserByIdQuery
            {
                UserId = default(Guid)
            };

            // Act
            var result = _validator.Validate(query);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(2);
            result.Errors.Should().Contain(e => e.PropertyName == "UserId");
            result.Errors.Should().Contain(e => e.ErrorMessage == "User ID is required");
            result.Errors.Should().Contain(e => e.ErrorMessage == "User ID cannot be empty GUID");
        }

        [Fact]
        public void Validate_WithEmptyUserId_ShouldReturnInvalid()
        {
            // Arrange
            var query = new GetUserByIdQuery
            {
                UserId = Guid.Empty
            };

            // Act
            var result = _validator.Validate(query);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(2);
            result.Errors.Should().Contain(e => e.PropertyName == "UserId");
            result.Errors.Should().Contain(e => e.ErrorMessage == "User ID is required");
            result.Errors.Should().Contain(e => e.ErrorMessage == "User ID cannot be empty GUID");
        }

        [Fact]
        public void Validate_WithValidUserId_ShouldReturnValid()
        {
            // Arrange
            var query = new GetUserByIdQuery
            {
                UserId = Guid.NewGuid()
            };

            // Act
            var result = _validator.Validate(query);

            // Assert
            result.IsValid.Should().BeTrue();
        }
    }
}