using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SampleProject.Application.Features.Auth.Commands.RefreshToken;
using SampleProject.Application.Interfaces;
using SampleProject.Application.Interfaces.SampleProject.Authorization;
using SampleProject.Application.Dto;
using SampleProject.Domain.Enums;
using SampleProject.Domain.Responses;
using Xunit;

namespace SampleProject.Test.Unit.Application.Features.Auth.Commands
{
    /// <summary>
    /// Unit tests for RefreshTokenCommand handler
    /// </summary>
    public class RefreshTokenCommandTests
    {
        private readonly Mock<IAuthorization> _authorizationMock;
        private readonly RefreshTokenCommandHandler _handler;
        private readonly Mock<IJwtService> _jwtServiceMock;
        private readonly Mock<ILogger<RefreshTokenCommandHandler>> _loggerMock;

        public RefreshTokenCommandTests()
        {
            _authorizationMock = new Mock<IAuthorization>();
            _jwtServiceMock = new Mock<IJwtService>();
            _loggerMock = new Mock<ILogger<RefreshTokenCommandHandler>>();
            _handler = new RefreshTokenCommandHandler(
                _authorizationMock.Object,
                _jwtServiceMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_WithDatabaseError_ShouldReturnFailure()
        {
            // Arrange
            var command = new RefreshTokenCommand
            {
                RefreshToken = "valid_refresh_token"
            };

            var user = new UserDto
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                IsActive = true,
                Role = UserRole.User
            };

            var tokenResponse = new TokenResponse
            {
                AccessToken = "new_access_token",
                RefreshToken = "new_refresh_token",
                ExpiresIn = 3600,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                TokenType = "Bearer"
            };

            _authorizationMock
                .Setup(x => x.ValidateRefreshTokenAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            _jwtServiceMock
                .Setup(x => x.GenerateTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<UserRole>()))
                .ReturnsAsync(tokenResponse);

            _authorizationMock
                .Setup(x => x.SaveRefreshTokenAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_WithEmptyRefreshToken_ShouldReturnFailure()
        {
            // Arrange
            var command = new RefreshTokenCommand
            {
                RefreshToken = ""
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            _authorizationMock.Verify(x => x.ValidateRefreshTokenAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithExpiredRefreshToken_ShouldReturnFailure()
        {
            // Arrange
            var command = new RefreshTokenCommand
            {
                RefreshToken = "expired_refresh_token"
            };

            _authorizationMock
                .Setup(x => x.ValidateRefreshTokenAsync(It.IsAny<string>()))
                .ReturnsAsync((UserDto?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            _authorizationMock.Verify(x => x.ValidateRefreshTokenAsync(command.RefreshToken), Times.Once);
            _jwtServiceMock.Verify(x => x.GenerateTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<UserRole>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithInactiveUser_ShouldReturnFailure()
        {
            // Arrange
            var command = new RefreshTokenCommand
            {
                RefreshToken = "valid_refresh_token"
            };

            var user = new UserDto
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                IsActive = false, // Inactive user
                Role = UserRole.User
            };

            _authorizationMock
                .Setup(x => x.ValidateRefreshTokenAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            _authorizationMock.Verify(x => x.ValidateRefreshTokenAsync(command.RefreshToken), Times.Once);
            _jwtServiceMock.Verify(x => x.GenerateTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<UserRole>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithInvalidRefreshToken_ShouldReturnFailure()
        {
            // Arrange
            var command = new RefreshTokenCommand
            {
                RefreshToken = "invalid_refresh_token"
            };

            _authorizationMock
                .Setup(x => x.ValidateRefreshTokenAsync(It.IsAny<string>()))
                .ReturnsAsync((UserDto?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            _authorizationMock.Verify(x => x.ValidateRefreshTokenAsync(command.RefreshToken), Times.Once);
            _jwtServiceMock.Verify(x => x.GenerateTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<UserRole>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithNullRefreshToken_ShouldReturnFailure()
        {
            // Arrange
            var command = new RefreshTokenCommand
            {
                RefreshToken = null!
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            _authorizationMock.Verify(x => x.ValidateRefreshTokenAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithValidRefreshToken_ShouldReturnNewTokens()
        {
            // Arrange
            var command = new RefreshTokenCommand
            {
                RefreshToken = "valid_refresh_token"
            };

            var user = new UserDto
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                IsActive = true,
                Role = UserRole.User
            };

            var tokenResponse = new TokenResponse
            {
                AccessToken = "new_access_token",
                RefreshToken = "new_refresh_token",
                ExpiresIn = 3600,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                TokenType = "Bearer"
            };

            _authorizationMock
                .Setup(x => x.ValidateRefreshTokenAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            _jwtServiceMock
                .Setup(x => x.GenerateTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<UserRole>()))
                .ReturnsAsync(tokenResponse);

            _authorizationMock
                .Setup(x => x.SaveRefreshTokenAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value!.AccessToken.Should().Be("new_access_token");
            result.Value!.RefreshToken.Should().Be("new_refresh_token");

            _authorizationMock.Verify(x => x.ValidateRefreshTokenAsync(command.RefreshToken), Times.Once);
            _jwtServiceMock.Verify(x => x.GenerateTokenAsync(user.Id.ToString(), user.Email, user.Email, user.FirstName, user.LastName, user.Role), Times.Once);
            _authorizationMock.Verify(x => x.SaveRefreshTokenAsync(user.Id, It.IsAny<string>(), It.IsAny<DateTime>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithValidRefreshToken_ShouldUpdateUserRefreshToken()
        {
            // Arrange
            var command = new RefreshTokenCommand
            {
                RefreshToken = "valid_refresh_token"
            };

            var user = new UserDto
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                IsActive = true,
                Role = UserRole.User
            };

            var tokenResponse = new TokenResponse
            {
                AccessToken = "new_access_token",
                RefreshToken = "new_refresh_token",
                ExpiresIn = 3600,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                TokenType = "Bearer"
            };

            _authorizationMock
                .Setup(x => x.ValidateRefreshTokenAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            _jwtServiceMock
                .Setup(x => x.GenerateTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<UserRole>()))
                .ReturnsAsync(tokenResponse);

            _authorizationMock
                .Setup(x => x.SaveRefreshTokenAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();

            // Verify that the refresh token was saved
            _authorizationMock.Verify(x => x.SaveRefreshTokenAsync(
                user.Id,
                tokenResponse.RefreshToken,
                It.IsAny<DateTime>()), Times.Once);
        }
    }
}