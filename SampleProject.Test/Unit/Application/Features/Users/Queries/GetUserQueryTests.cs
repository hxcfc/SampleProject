using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SampleProject.Application.Features.Users.Queries.GetUserById;
using SampleProject.Application.Interfaces;
using SampleProject.Domain.Common;
using SampleProject.Application.Dto;
using SampleProject.Domain.Enums;
using Xunit;

namespace SampleProject.Test.Unit.Application.Features.Users.Queries
{
    /// <summary>
    /// Unit tests for GetUserByIdQuery handler
    /// </summary>
    public class GetUserByIdQueryTests
    {
        private readonly GetUserByIdQueryHandler _handler;
        private readonly Mock<IUserService> _userServiceMock;

        public GetUserByIdQueryTests()
        {
            _userServiceMock = new Mock<IUserService>();
            _handler = new GetUserByIdQueryHandler(_userServiceMock.Object, Mock.Of<ILogger<GetUserByIdQueryHandler>>());
        }

        [Fact]
        public async Task Handle_WithAdminUser_ShouldReturnAdminRole()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var query = new GetUserByIdQuery { UserId = userId };

            var user = new UserDto
            {
                Id = userId,
                Email = "admin@example.com",
                FirstName = "Admin",
                LastName = "User",
                IsActive = true,
                Role = UserRole.Admin,
                CreatedAt = DateTime.UtcNow
            };

            _userServiceMock
                .Setup(x => x.GetUserByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(Result<UserDto>.Success(user));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value!.Role.Should().Be(UserRole.Admin);
        }

        [Fact]
        public async Task Handle_WithDatabaseError_ShouldReturnFailure()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var query = new GetUserByIdQuery { UserId = userId };

            _userServiceMock
                .Setup(x => x.GetUserByIdAsync(It.IsAny<Guid>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_WithEmailVerifiedUser_ShouldReturnVerifiedStatus()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var query = new GetUserByIdQuery { UserId = userId };

            var user = new UserDto
            {
                Id = userId,
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                IsActive = true,
                IsEmailVerified = true,
                Role = UserRole.User,
                CreatedAt = DateTime.UtcNow
            };

            _userServiceMock
                .Setup(x => x.GetUserByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(Result<UserDto>.Success(user));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value!.IsEmailVerified.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_WithEmptyUserId_ShouldReturnFailure()
        {
            // Arrange
            var query = new GetUserByIdQuery { UserId = Guid.Empty };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            _userServiceMock.Verify(x => x.GetUserByIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithInactiveUser_ShouldReturnInactiveStatus()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var query = new GetUserByIdQuery { UserId = userId };

            var user = new UserDto
            {
                Id = userId,
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                IsActive = false,
                Role = UserRole.User,
                CreatedAt = DateTime.UtcNow
            };

            _userServiceMock
                .Setup(x => x.GetUserByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(Result<UserDto>.Success(user));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value!.IsActive.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_WithNonExistentUserId_ShouldReturnFailure()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var query = new GetUserByIdQuery { UserId = userId };

            _userServiceMock
                .Setup(x => x.GetUserByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(Result<UserDto>.Failure("User not found"));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            _userServiceMock.Verify(x => x.GetUserByIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task Handle_WithValidUserId_ShouldMapUserCorrectly()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var query = new GetUserByIdQuery { UserId = userId };

            var user = new UserDto
            {
                Id = userId,
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                IsActive = true,
                IsEmailVerified = false,
                Role = UserRole.User,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _userServiceMock
                .Setup(x => x.GetUserByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(Result<UserDto>.Success(user));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();

            var userDto = result.Value!;
            userDto.Id.Should().Be(user.Id);
            userDto.Email.Should().Be(user.Email);
            userDto.FirstName.Should().Be(user.FirstName);
            userDto.LastName.Should().Be(user.LastName);
            userDto.FullName.Should().Be($"{user.FirstName} {user.LastName}");
            userDto.Role.Should().Be(user.Role);
            userDto.IsActive.Should().Be(user.IsActive);
            userDto.IsEmailVerified.Should().Be(user.IsEmailVerified);
            userDto.CreatedAt.Should().Be(user.CreatedAt);
        }

        [Fact]
        public async Task Handle_WithValidUserId_ShouldReturnUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var query = new GetUserByIdQuery { UserId = userId };

            var user = new UserDto
            {
                Id = userId,
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                IsActive = true,
                IsEmailVerified = false,
                Role = UserRole.User,
                CreatedAt = DateTime.UtcNow
            };

            _userServiceMock
                .Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(Result<UserDto>.Success(user));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value!.Id.Should().Be(userId);
            result.Value!.Email.Should().Be(user.Email);
            result.Value!.FirstName.Should().Be(user.FirstName);
            result.Value!.LastName.Should().Be(user.LastName);

            _userServiceMock.Verify(x => x.GetUserByIdAsync(userId), Times.Once);
        }
    }
}