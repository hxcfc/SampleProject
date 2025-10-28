using SampleProject.Application.Features.Auth.Commands.Login;
using FluentAssertions;
using Xunit;

namespace SampleProject.Test.Unit.Application.Features.Auth.Commands
{
    /// <summary>
    /// Unit tests for LoginCommandValidator
    /// </summary>
    public class LoginCommandValidatorTests
    {
        private readonly LoginCommandValidator _validator;

        public LoginCommandValidatorTests()
        {
            _validator = new LoginCommandValidator();
        }

        [Fact]
        public void Validate_WithValidData_ShouldPass()
        {
            // Arrange
            var command = new LoginCommand
            {
                Email = "test@example.com",
                Password = "password123"
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
            var command = new LoginCommand
            {
                Email = email,
                Password = "password123"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginCommand.Email));
        }

        [Theory]
        [InlineData("invalid-email")]
        [InlineData("test@")]
        [InlineData("@example.com")]
        [InlineData("test.example.com")]
        public void Validate_WithInvalidEmailFormat_ShouldFail(string email)
        {
            // Arrange
            var command = new LoginCommand
            {
                Email = email,
                Password = "password123"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginCommand.Email));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Validate_WithEmptyPassword_ShouldFail(string password)
        {
            // Arrange
            var command = new LoginCommand
            {
                Email = "test@example.com",
                Password = password
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginCommand.Password));
        }

        [Fact]
        public void Validate_WithShortPassword_ShouldFail()
        {
            // Arrange
            var command = new LoginCommand
            {
                Email = "test@example.com",
                Password = "123"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginCommand.Password));
        }

        [Fact]
        public void Validate_WithMinLengthPassword_ShouldPass()
        {
            // Arrange
            var command = new LoginCommand
            {
                Email = "test@example.com",
                Password = "12345678" // Exactly 8 characters
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
            var command = new LoginCommand
            {
                Email = "test@example.com",
                Password = "very_long_password_with_many_characters_123456789"
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
            var command = new LoginCommand
            {
                Email = "user@example.com",
                Password = "password123"
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
            var command = new LoginCommand
            {
                Email = "user@subdomain.example.com",
                Password = "password123"
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
            var command = new LoginCommand
            {
                Email = "user123@example123.com",
                Password = "password123"
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
            var command = new LoginCommand
            {
                Email = "user.name+tag@example-domain.com",
                Password = "password123"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_WithValidPassword_ShouldPass()
        {
            // Arrange
            var command = new LoginCommand
            {
                Email = "test@example.com",
                Password = "password123"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_WithPasswordContainingNumbers_ShouldPass()
        {
            // Arrange
            var command = new LoginCommand
            {
                Email = "test@example.com",
                Password = "password123"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_WithPasswordContainingSpecialCharacters_ShouldPass()
        {
            // Arrange
            var command = new LoginCommand
            {
                Email = "test@example.com",
                Password = "password!@#$%^&*()"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_WithPasswordContainingMixedCase_ShouldPass()
        {
            // Arrange
            var command = new LoginCommand
            {
                Email = "test@example.com",
                Password = "Password123"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_WithPasswordContainingSpaces_ShouldPass()
        {
            // Arrange
            var command = new LoginCommand
            {
                Email = "test@example.com",
                Password = "password with spaces"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_WithPasswordContainingTabs_ShouldPass()
        {
            // Arrange
            var command = new LoginCommand
            {
                Email = "test@example.com",
                Password = "password\twith\ttabs"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_WithPasswordContainingNewlines_ShouldPass()
        {
            // Arrange
            var command = new LoginCommand
            {
                Email = "test@example.com",
                Password = "password\nwith\nnewlines"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_WithPasswordContainingCarriageReturns_ShouldPass()
        {
            // Arrange
            var command = new LoginCommand
            {
                Email = "test@example.com",
                Password = "password\rwith\rcarriage\rreturns"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_WithPasswordContainingUnicode_ShouldPass()
        {
            // Arrange
            var command = new LoginCommand
            {
                Email = "test@example.com",
                Password = "password_with_unicode_🚀"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_WithPasswordContainingEmojis_ShouldPass()
        {
            // Arrange
            var command = new LoginCommand
            {
                Email = "test@example.com",
                Password = "password_with_emojis_😀🎉🔥"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_WithPasswordContainingSymbols_ShouldPass()
        {
            // Arrange
            var command = new LoginCommand
            {
                Email = "test@example.com",
                Password = "password_with_symbols_!@#$%^&*()_+-=[]{}|;':\",./<>?"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_WithPasswordContainingNumbersAndSymbols_ShouldPass()
        {
            // Arrange
            var command = new LoginCommand
            {
                Email = "test@example.com",
                Password = "password123!@#$%^&*()"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_WithPasswordContainingMixedCaseAndNumbers_ShouldPass()
        {
            // Arrange
            var command = new LoginCommand
            {
                Email = "test@example.com",
                Password = "Password123"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_WithPasswordContainingMixedCaseAndSymbols_ShouldPass()
        {
            // Arrange
            var command = new LoginCommand
            {
                Email = "test@example.com",
                Password = "Password!@#"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_WithPasswordContainingNumbersAndSymbolsAndMixedCase_ShouldPass()
        {
            // Arrange
            var command = new LoginCommand
            {
                Email = "test@example.com",
                Password = "Password123!@#"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_WithBothInvalidFields_ShouldFailWithMultipleErrors()
        {
            // Arrange
            var command = new LoginCommand
            {
                Email = "invalid-email",
                Password = ""
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(3);
            result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginCommand.Email));
            result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginCommand.Password));
            
            // Verify specific error messages
            result.Errors.Should().Contain(e => e.ErrorMessage == "Email must be a valid email address");
            result.Errors.Should().Contain(e => e.ErrorMessage == "Password is required");
            result.Errors.Should().Contain(e => e.ErrorMessage == "Password must be at least 6 characters long");
        }
    }
}