using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SampleProject.Application.Features.Users.Commands.CreateUser;
using SampleProject.Application.Interfaces;
using SampleProject.Domain.Common;
using SampleProject.Application.Dto;
using SampleProject.Domain.Enums;
using Xunit;

namespace SampleProject.Test.Unit.Application.Features.Users.Commands
{
    /// <summary>
    /// Unit tests for CreateUserCommand handler
    /// </summary>
    public class CreateUserCommandTests
    {
        private readonly CreateUserCommandHandler _handler;
        private readonly Mock<IUserService> _userServiceMock;

        public CreateUserCommandTests()
        {
            _userServiceMock = new Mock<IUserService>();
            _handler = new CreateUserCommandHandler(
                _userServiceMock.Object,
                Mock.Of<ILogger<CreateUserCommandHandler>>());
        }

        [Fact]
        public async Task Handle_WithDatabaseError_ShouldReturnFailure()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "test@example.com",
                Password = "password123",
                FirstName = "John",
                LastName = "Doe"
            };

            _userServiceMock
                .Setup(x => x.CreateUserAsync(command.Email, command.FirstName, command.LastName, command.Password))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("Error occurred during user registration");
        }

        [Fact]
        public async Task Handle_WithEmptyFirstName_ShouldReturnFailure()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "test@example.com",
                Password = "password123",
                FirstName = "", // Empty first name
                LastName = "Doe"
            };

            _userServiceMock
                .Setup(x => x.CreateUserAsync(command.Email, command.FirstName, command.LastName, command.Password))
                .ReturnsAsync(Result<UserDto>.Failure("First name is required"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("First name is required");

            _userServiceMock.Verify(x => x.CreateUserAsync(command.Email, command.FirstName, command.LastName, command.Password), Times.Once);
        }

        [Fact]
        public async Task Handle_WithExistingEmail_ShouldReturnFailure()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "existing@example.com",
                Password = "password123",
                FirstName = "John",
                LastName = "Doe"
            };

            _userServiceMock
                .Setup(x => x.CreateUserAsync(command.Email, command.FirstName, command.LastName, command.Password))
                .ReturnsAsync(Result<UserDto>.Failure("User with this email already exists"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("User with this email already exists");

            _userServiceMock.Verify(x => x.CreateUserAsync(command.Email, command.FirstName, command.LastName, command.Password), Times.Once);
        }

        [Fact]
        public async Task Handle_WithInvalidEmail_ShouldReturnFailure()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "invalid-email",
                Password = "password123",
                FirstName = "John",
                LastName = "Doe"
            };

            _userServiceMock
                .Setup(x => x.CreateUserAsync(command.Email, command.FirstName, command.LastName, command.Password))
                .ReturnsAsync(Result<UserDto>.Failure("Invalid email format"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("Invalid email format");

            _userServiceMock.Verify(x => x.CreateUserAsync(command.Email, command.FirstName, command.LastName, command.Password), Times.Once);
        }

        [Fact]
        public async Task Handle_WithValidData_ShouldCreateUserSuccessfully()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "newuser@example.com",
                Password = "password123",
                FirstName = "John",
                LastName = "Doe"
            };

            var expectedUser = new UserDto
            {
                Id = Guid.NewGuid(),
                Email = command.Email,
                FirstName = command.FirstName,
                LastName = command.LastName,
                Role = UserRole.User,
                IsActive = true,
                IsEmailVerified = false,
                CreatedAt = DateTime.UtcNow
            };

            _userServiceMock
                .Setup(x => x.CreateUserAsync(command.Email, command.FirstName, command.LastName, command.Password))
                .ReturnsAsync(Result<UserDto>.Success(expectedUser));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value!.Email.Should().Be(command.Email);
            result.Value.FirstName.Should().Be(command.FirstName);
            result.Value.LastName.Should().Be(command.LastName);
            result.Value.Role.Should().Be(UserRole.User);
            result.Value.IsActive.Should().BeTrue();

            _userServiceMock.Verify(x => x.CreateUserAsync(command.Email, command.FirstName, command.LastName, command.Password), Times.Once);
        }

        [Fact]
        public async Task Handle_WithWeakPassword_ShouldReturnFailure()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "test@example.com",
                Password = "123", // Too short
                FirstName = "John",
                LastName = "Doe"
            };

            _userServiceMock
                .Setup(x => x.CreateUserAsync(command.Email, command.FirstName, command.LastName, command.Password))
                .ReturnsAsync(Result<UserDto>.Failure("Password must be at least 8 characters long"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("Password must be at least 8 characters long");

            _userServiceMock.Verify(x => x.CreateUserAsync(command.Email, command.FirstName, command.LastName, command.Password), Times.Once);
        }
    }
}