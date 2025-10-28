using SampleProject.Application.Features.Users.Commands.CreateUser;
using FluentAssertions;
using Xunit;

namespace SampleProject.Test.Unit.Application.Features.Users.Commands
{
    /// <summary>
    /// Unit tests for CreateUserCommandValidator
    /// </summary>
    public class CreateUserCommandValidatorTests
    {
        private readonly CreateUserCommandValidator _validator;

        public CreateUserCommandValidatorTests()
        {
            _validator = new CreateUserCommandValidator();
        }

        [Fact]
        public void Validate_WithValidData_ShouldPass()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "test@example.com",
                Password = "password123",
                FirstName = "John",
                LastName = "Doe"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Validate_WithEmptyEmail_ShouldFail(string email)
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = email,
                Password = "password123",
                FirstName = "John",
                LastName = "Doe"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUserCommand.Email));
        }

        [Theory]
        [InlineData("invalid-email")]
        [InlineData("test@")]
        [InlineData("@example.com")]
        [InlineData("test.example.com")]
        public void Validate_WithInvalidEmailFormat_ShouldFail(string email)
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = email,
                Password = "password123",
                FirstName = "John",
                LastName = "Doe"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUserCommand.Email));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Validate_WithEmptyPassword_ShouldFail(string password)
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "test@example.com",
                Password = password,
                FirstName = "John",
                LastName = "Doe"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUserCommand.Password));
        }

        [Fact]
        public void Validate_WithShortPassword_ShouldFail()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "test@example.com",
                Password = "123",
                FirstName = "John",
                LastName = "Doe"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUserCommand.Password));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Validate_WithEmptyFirstName_ShouldFail(string firstName)
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "test@example.com",
                Password = "password123",
                FirstName = firstName,
                LastName = "Doe"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUserCommand.FirstName));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Validate_WithEmptyLastName_ShouldFail(string lastName)
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "test@example.com",
                Password = "password123",
                FirstName = "John",
                LastName = lastName
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUserCommand.LastName));
        }

        [Fact]
        public void Validate_WithLongFirstName_ShouldFail()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "test@example.com",
                Password = "password123",
                FirstName = new string('A', 101), // More than 100 characters
                LastName = "Doe"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUserCommand.FirstName));
        }

        [Fact]
        public void Validate_WithLongLastName_ShouldFail()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "test@example.com",
                Password = "password123",
                FirstName = "John",
                LastName = new string('A', 101) // More than 100 characters
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUserCommand.LastName));
        }

        [Fact]
        public void Validate_WithMaxLengthFirstName_ShouldPass()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "test@example.com",
                Password = "password123",
                FirstName = new string('A', 50), // Exactly 50 characters
                LastName = "Doe"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_WithMaxLengthLastName_ShouldPass()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "test@example.com",
                Password = "password123",
                FirstName = "John",
                LastName = new string('A', 50) // Exactly 50 characters
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_WithMinLengthPassword_ShouldPass()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "test@example.com",
                Password = "12345678", // Exactly 8 characters
                FirstName = "John",
                LastName = "Doe"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_WithLongPassword_ShouldPass()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "test@example.com",
                Password = "very_long_password_with_many_characters_123456789",
                FirstName = "John",
                LastName = "Doe"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_WithValidEmail_ShouldPass()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "user@example.com",
                Password = "password123",
                FirstName = "John",
                LastName = "Doe"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_WithValidEmailWithSubdomain_ShouldPass()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "user@subdomain.example.com",
                Password = "password123",
                FirstName = "John",
                LastName = "Doe"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_WithValidEmailWithNumbers_ShouldPass()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "user123@example123.com",
                Password = "password123",
                FirstName = "John",
                LastName = "Doe"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_WithValidEmailWithSpecialCharacters_ShouldPass()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "user.name+tag@example-domain.com",
                Password = "password123",
                FirstName = "John",
                LastName = "Doe"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_WithValidFirstName_ShouldPass()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "test@example.com",
                Password = "password123",
                FirstName = "John",
                LastName = "Doe"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_WithValidLastName_ShouldPass()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "test@example.com",
                Password = "password123",
                FirstName = "John",
                LastName = "Doe"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_WithFirstNameContainingNumbers_ShouldPass()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "test@example.com",
                Password = "password123",
                FirstName = "John123",
                LastName = "Doe"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_WithLastNameContainingNumbers_ShouldPass()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "test@example.com",
                Password = "password123",
                FirstName = "John",
                LastName = "Doe123"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_WithFirstNameContainingSpecialCharacters_ShouldPass()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "test@example.com",
                Password = "password123",
                FirstName = "John-Paul",
                LastName = "Doe"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_WithLastNameContainingSpecialCharacters_ShouldPass()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "test@example.com",
                Password = "password123",
                FirstName = "John",
                LastName = "O'Connor"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_WithAllInvalidFields_ShouldFailWithMultipleErrors()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "invalid-email",
                Password = "",
                FirstName = "",
                LastName = ""
            };

            // Act
            var result = _validator.Validate(command);


            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(5);
            result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUserCommand.Email));
            result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUserCommand.Password));
            result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUserCommand.FirstName));
            result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUserCommand.LastName));
            
            // Verify specific error messages
            result.Errors.Should().Contain(e => e.ErrorMessage == "Email must be a valid email address");
            result.Errors.Should().Contain(e => e.ErrorMessage == "Password is required");
            result.Errors.Should().Contain(e => e.ErrorMessage == "Password must be at least 6 characters long");
            result.Errors.Should().Contain(e => e.ErrorMessage == "First name is required");
            result.Errors.Should().Contain(e => e.ErrorMessage == "Last name is required");
        }
    }
}