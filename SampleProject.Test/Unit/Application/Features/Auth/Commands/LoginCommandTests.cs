using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SampleProject.Application.Features.Auth.Commands.Login;
using SampleProject.Application.Interfaces;
using SampleProject.Application.Interfaces.SampleProject.Authorization;
using SampleProject.Application.Dto;
using SampleProject.Domain.Enums;
using SampleProject.Domain.Responses;
using Xunit;

namespace SampleProject.Test.Unit.Application.Features.Auth.Commands
{
    /// <summary>
    /// Unit tests for LoginCommand handler
    /// </summary>
    public class LoginCommandTests
    {
        private readonly Mock<IAuthorization> _authorizationMock;
        private readonly LoginCommandHandler _handler;
        private readonly Mock<IJwtService> _jwtServiceMock;
        private readonly Mock<ILogger<LoginCommandHandler>> _loggerMock;

        public LoginCommandTests()
        {
            _authorizationMock = new Mock<IAuthorization>();
            _jwtServiceMock = new Mock<IJwtService>();
            _loggerMock = new Mock<ILogger<LoginCommandHandler>>();
            _handler = new LoginCommandHandler(
                _authorizationMock.Object,
                _jwtServiceMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_WithInactiveUser_ShouldReturnFailure()
        {
            // Arrange
            var command = new LoginCommand
            {
                Email = "test@example.com",
                Password = "password123"
            };

            var user = new UserDto
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                IsActive = false,
                IsEmailVerified = false,
                CreatedAt = DateTime.UtcNow,
                Role = UserRole.User
            };

            _authorizationMock
                .Setup(x => x.ValidateCredentialsAsync(command.Email, command.Password))
                .ReturnsAsync(user);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Account is not active");

            _authorizationMock.Verify(x => x.ValidateCredentialsAsync(command.Email, command.Password), Times.Once);
        }

        [Fact]
        public async Task Handle_WithInvalidEmail_ShouldReturnFailure()
        {
            // Arrange
            var command = new LoginCommand
            {
                Email = "nonexistent@example.com",
                Password = "password123"
            };

            _authorizationMock
                .Setup(x => x.ValidateCredentialsAsync(command.Email, command.Password))
                .ReturnsAsync((UserDto?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Invalid email or password");

            _authorizationMock.Verify(x => x.ValidateCredentialsAsync(command.Email, command.Password), Times.Once);
        }

        [Fact]
        public async Task Handle_WithInvalidPassword_ShouldReturnFailure()
        {
            // Arrange
            var command = new LoginCommand
            {
                Email = "test@example.com",
                Password = "wrong_password"
            };

            _authorizationMock
                .Setup(x => x.ValidateCredentialsAsync(command.Email, command.Password))
                .ReturnsAsync((UserDto?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Invalid email or password");

            _authorizationMock.Verify(x => x.ValidateCredentialsAsync(command.Email, command.Password), Times.Once);
        }

        [Fact]
        public async Task Handle_WithJwtServiceException_ShouldReturnFailure()
        {
            // Arrange
            var command = new LoginCommand
            {
                Email = "test@example.com",
                Password = "password123"
            };

            var user = new UserDto
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                IsActive = true,
                IsEmailVerified = false,
                CreatedAt = DateTime.UtcNow,
                Role = UserRole.User
            };

            _authorizationMock
                .Setup(x => x.ValidateCredentialsAsync(command.Email, command.Password))
                .ReturnsAsync(user);

            _jwtServiceMock
                .Setup(x => x.GenerateTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<UserRole>()))
                .ThrowsAsync(new Exception("JWT generation failed"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("An error occurred during login");

            _authorizationMock.Verify(x => x.ValidateCredentialsAsync(command.Email, command.Password), Times.Once);
            _jwtServiceMock.Verify(x => x.GenerateTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<UserRole>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithRefreshTokenSaveFailure_ShouldStillReturnSuccess()
        {
            // Arrange
            var command = new LoginCommand
            {
                Email = "test@example.com",
                Password = "password123"
            };

            var user = new UserDto
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                IsActive = true,
                IsEmailVerified = false,
                CreatedAt = DateTime.UtcNow,
                Role = UserRole.User
            };

            _authorizationMock
                .Setup(x => x.ValidateCredentialsAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(user);

            _jwtServiceMock
                .Setup(x => x.GenerateTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<UserRole>()))
                .ReturnsAsync(new TokenResponse
                {
                    AccessToken = "access_token",
                    RefreshToken = "refresh_token",
                    ExpiresIn = 3600,
                    ExpiresAt = DateTime.UtcNow.AddHours(1),
                    TokenType = "Bearer"
                });

            _authorizationMock
                .Setup(x => x.SaveRefreshTokenAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .ReturnsAsync(false);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value!.AccessToken.Should().Be("access_token");
            result.Value!.RefreshToken.Should().Be("refresh_token");

            _authorizationMock.Verify(x => x.ValidateCredentialsAsync(command.Email, command.Password), Times.Once);
            _jwtServiceMock.Verify(x => x.GenerateTokenAsync(user.Id.ToString(), user.Email, user.Email, user.FirstName, user.LastName, user.Role), Times.Once);
            _authorizationMock.Verify(x => x.SaveRefreshTokenAsync(user.Id, It.IsAny<string>(), It.IsAny<DateTime>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithValidCredentials_ShouldReturnSuccess()
        {
            // Arrange
            var command = new LoginCommand
            {
                Email = "test@example.com",
                Password = "password123"
            };

            var user = new UserDto
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                IsActive = true,
                IsEmailVerified = false,
                CreatedAt = DateTime.UtcNow,
                Role = UserRole.User
            };

            _authorizationMock
                .Setup(x => x.ValidateCredentialsAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(user);

            _jwtServiceMock
                .Setup(x => x.GenerateTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<UserRole>()))
                .ReturnsAsync(new TokenResponse
                {
                    AccessToken = "access_token",
                    RefreshToken = "refresh_token",
                    ExpiresIn = 3600,
                    ExpiresAt = DateTime.UtcNow.AddHours(1),
                    TokenType = "Bearer"
                });

            _authorizationMock
                .Setup(x => x.SaveRefreshTokenAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value!.AccessToken.Should().Be("access_token");
            result.Value!.RefreshToken.Should().Be("refresh_token");

            _authorizationMock.Verify(x => x.ValidateCredentialsAsync(command.Email, command.Password), Times.Once);
            _jwtServiceMock.Verify(x => x.GenerateTokenAsync(user.Id.ToString(), user.Email, user.Email, user.FirstName, user.LastName, user.Role), Times.Once);
            _authorizationMock.Verify(x => x.SaveRefreshTokenAsync(user.Id, It.IsAny<string>(), It.IsAny<DateTime>()), Times.Once);
        }
    }
}