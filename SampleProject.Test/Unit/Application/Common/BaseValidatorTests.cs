using FluentAssertions;
using FluentValidation;
using SampleProject.Application.Common;
using Xunit;

namespace SampleProject.Test.Unit.Application.Common
{
    /// <summary>
    /// Unit tests for BaseValidator
    /// </summary>
    public class BaseValidatorTests
    {
        private readonly TestValidator _validator;

        public BaseValidatorTests()
        {
            _validator = new TestValidator();
        }

        [Fact]
        public void BeSafeFromSqlInjection_WithSafeInput_ShouldReturnTrue()
        {
            // Arrange
            var model = new TestModel { Email = "test@example.com", Name = "John", Password = "password123", Id = Guid.NewGuid(), Input = "Safe input text" };

            // Act
            var result = _validator.Validate(model);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData("SELECT * FROM users")]
        [InlineData("UNION SELECT password")]
        [InlineData("INSERT INTO users")]
        [InlineData("UPDATE users SET")]
        [InlineData("DELETE FROM users")]
        [InlineData("DROP TABLE users")]
        [InlineData("CREATE TABLE users")]
        [InlineData("ALTER TABLE users")]
        [InlineData("EXEC sp_help")]
        [InlineData("-- comment")]
        [InlineData("/* comment */")]
        [InlineData("xp_cmdshell")]
        [InlineData("sp_executesql")]
        [InlineData("WAITFOR DELAY")]
        public void BeSafeFromSqlInjection_WithSqlInjectionPatterns_ShouldReturnFalse(string input)
        {
            // Arrange
            var model = new TestModel { Email = "test@example.com", Name = "John", Password = "password123", Id = Guid.NewGuid(), Input = input };

            // Act
            var result = _validator.Validate(model);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Input");
        }

        [Theory]
        [InlineData("test<script>@example.com")]
        [InlineData("testjavascript@example.com")]
        public void BeValidEmailFormat_WithDangerousCharacters_ShouldReturnFalse(string email)
        {
            // Arrange
            var model = new TestModel { Email = email, Name = "John", Password = "password123", Id = Guid.NewGuid(), Input = "safe input" };

            // Act
            var result = _validator.Validate(model);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Email");
        }

        [Fact]
        public void BeValidEmailFormat_WithValidEmail_ShouldReturnTrue()
        {
            // Arrange
            var model = new TestModel { Email = "test@example.com", Name = "John", Password = "password123", Id = Guid.NewGuid(), Input = "safe input" };

            // Act
            var result = _validator.Validate(model);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void BeValidGuid_WithEmptyGuid_ShouldReturnFalse()
        {
            // Arrange
            var model = new TestModel { Id = Guid.Empty };

            // Act
            var result = _validator.Validate(model);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Id");
        }

        [Fact]
        public void BeValidGuid_WithValidGuid_ShouldReturnTrue()
        {
            // Arrange
            var model = new TestModel { Email = "test@example.com", Name = "John", Password = "password123", Id = Guid.NewGuid(), Input = "safe input" };

            // Act
            var result = _validator.Validate(model);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData("John<script>")]
        [InlineData("JohnjavascriptDoe")]
        [InlineData("JohnunionDoe")]
        [InlineData("JohnselectDoe")]
        public void BeValidName_WithDangerousCharacters_ShouldReturnFalse(string name)
        {
            // Arrange
            var model = new TestModel { Email = "test@example.com", Name = name, Password = "password123", Id = Guid.NewGuid(), Input = "safe input" };

            // Act
            var result = _validator.Validate(model);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Name");
        }

        [Fact]
        public void BeValidName_WithValidName_ShouldReturnTrue()
        {
            // Arrange
            var model = new TestModel { Email = "test@example.com", Name = "John Doe", Password = "password123", Id = Guid.NewGuid(), Input = "safe input" };

            // Act
            var result = _validator.Validate(model);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData("password<script>")]
        [InlineData("passwordjavascript")]
        [InlineData("passwordunion")]
        [InlineData("passwordselect")]
        [InlineData("passwordinsert")]
        [InlineData("passwordupdate")]
        [InlineData("passworddelete")]
        [InlineData("passworddrop")]
        public void NotContainDangerousCharacters_WithDangerousCharacters_ShouldReturnFalse(string password)
        {
            // Arrange
            var model = new TestModel { Email = "test@example.com", Name = "John", Password = password, Id = Guid.NewGuid(), Input = "safe input" };

            // Act
            var result = _validator.Validate(model);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Password");
        }

        [Fact]
        public void NotContainDangerousCharacters_WithValidPassword_ShouldReturnTrue()
        {
            // Arrange
            var model = new TestModel { Email = "test@example.com", Name = "John", Password = "ValidPassword123!", Id = Guid.NewGuid(), Input = "safe input" };

            // Act
            var result = _validator.Validate(model);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        /// <summary>
        /// Test model for validation
        /// </summary>
        private class TestModel
        {
            public string Email { get; set; } = string.Empty;
            public Guid Id { get; set; }
            public string Input { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        /// <summary>
        /// Test class for BaseValidator testing
        /// </summary>
        private class TestValidator : BaseValidator<TestModel>
        {
            public TestValidator()
            {
                RuleFor(x => x.Email)
                    .Must(BeValidEmailFormat)
                    .WithMessage("Email contains invalid characters");

                RuleFor(x => x.Name)
                    .Must(BeValidName)
                    .WithMessage("Name contains invalid characters");

                RuleFor(x => x.Password)
                    .Must(NotContainDangerousCharacters)
                    .WithMessage("Password contains invalid characters");

                RuleFor(x => x.Id)
                    .Must(BeValidGuid)
                    .WithMessage("ID cannot be empty GUID");

                RuleFor(x => x.Input)
                    .Must(BeSafeFromSqlInjection)
                    .WithMessage("Input contains SQL injection patterns");
            }
        }
    }
}