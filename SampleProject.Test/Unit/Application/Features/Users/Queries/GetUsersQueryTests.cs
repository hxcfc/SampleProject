using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SampleProject.Application.Features.Users.Queries.GetUsersList;
using SampleProject.Application.Interfaces;
using SampleProject.Domain.Common;
using SampleProject.Application.Dto;
using SampleProject.Domain.Enums;
using Xunit;

namespace SampleProject.Test.Unit.Application.Features.Users.Queries
{
    /// <summary>
    /// Unit tests for GetUsersListQuery handler
    /// </summary>
    public class GetUsersListQueryTests
    {
        private readonly GetUsersListQueryHandler _handler;
        private readonly Mock<IUserService> _userServiceMock;

        public GetUsersListQueryTests()
        {
            _userServiceMock = new Mock<IUserService>();
            _handler = new GetUsersListQueryHandler(_userServiceMock.Object, Mock.Of<ILogger<GetUsersListQueryHandler>>());
        }

        [Fact]
        public async Task Handle_WithDatabaseError_ShouldReturnFailure()
        {
            // Arrange
            var query = new GetUsersListQuery
            {
                Page = 1,
                PageSize = 10
            };

            _userServiceMock
                .Setup(x => x.GetUsersListAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<UserFilters>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("Error occurred while getting users list");
        }

        [Fact]
        public async Task Handle_WithEmptyQuery_ShouldUseDefaultValues()
        {
            // Arrange
            var query = new GetUsersListQuery();

            var users = new List<UserDto>();
            var pagedResult = new PagedResult<UserDto>
            {
                Items = users,
                TotalCount = 0,
                Page = 1,
                PageSize = 10,
            };

            _userServiceMock
                .Setup(x => x.GetUsersListAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<UserFilters>()))
                .ReturnsAsync(Result<PagedResult<UserDto>>.Success(pagedResult));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Items.Should().BeEmpty();
            result.Value.TotalCount.Should().Be(0);
            result.Value.Page.Should().Be(1);
            result.Value.PageSize.Should().Be(10);
            result.Value.TotalPages.Should().Be(0);
        }

        [Fact]
        public async Task Handle_WithLargePageSize_ShouldLimitPageSize()
        {
            // Arrange
            var query = new GetUsersListQuery(1, 1000); // Very large page size

            var users = new List<UserDto>();
            var pagedResult = new PagedResult<UserDto>
            {
                Items = users,
                TotalCount = 0,
                Page = 1,
                PageSize = 100,
            };

            _userServiceMock
                .Setup(x => x.GetUsersListAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<UserFilters>()))
                .ReturnsAsync(Result<PagedResult<UserDto>>.Success(pagedResult));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.PageSize.Should().Be(100); // Should be limited to 100
        }

        [Fact]
        public async Task Handle_WithMultipleUsers_ShouldReturnAllUsers()
        {
            // Arrange
            var query = new GetUsersListQuery
            {
                Page = 1,
                PageSize = 10
            };

            var users = new List<UserDto>
            {
                new UserDto
                {
                    Id = Guid.NewGuid(),
                    Email = "user1@example.com",
                    FirstName = "John",
                    LastName = "Doe",
                    IsActive = true,
                    Role = UserRole.User,
                    CreatedAt = DateTime.UtcNow
                },
                new UserDto
                {
                    Id = Guid.NewGuid(),
                    Email = "user2@example.com",
                    FirstName = "Jane",
                    LastName = "Smith",
                    IsActive = true,
                    Role = UserRole.User,
                    CreatedAt = DateTime.UtcNow
                },
                new UserDto
                {
                    Id = Guid.NewGuid(),
                    Email = "admin@example.com",
                    FirstName = "Admin",
                    LastName = "User",
                    IsActive = true,
                    Role = UserRole.Admin,
                    CreatedAt = DateTime.UtcNow
                }
            };

            var pagedResult = new PagedResult<UserDto>
            {
                Items = users,
                TotalCount = users.Count,
                Page = 1,
                PageSize = 10,
            };

            _userServiceMock
                .Setup(x => x.GetUsersListAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<UserFilters>()))
                .ReturnsAsync(Result<PagedResult<UserDto>>.Success(pagedResult));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Items.Should().HaveCount(3);
            result.Value.TotalCount.Should().Be(3);
        }

        [Fact]
        public async Task Handle_WithNegativePage_ShouldUseDefaultPage()
        {
            // Arrange
            var query = new GetUsersListQuery
            {
                Page = -1,
                PageSize = 10
            };

            var users = new List<UserDto>();
            var pagedResult = new PagedResult<UserDto>
            {
                Items = users,
                TotalCount = 0,
                Page = 1,
                PageSize = 10,
            };

            _userServiceMock
                .Setup(x => x.GetUsersListAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<UserFilters>()))
                .ReturnsAsync(Result<PagedResult<UserDto>>.Success(pagedResult));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Page.Should().Be(1); // Should be set to 1
        }

        [Fact]
        public async Task Handle_WithPagination_ShouldCalculateTotalPagesCorrectly()
        {
            // Arrange
            var query = new GetUsersListQuery
            {
                Page = 1,
                PageSize = 5
            };

            var users = new List<UserDto>();
            var totalCount = 23; // 23 users total
            var pagedResult = new PagedResult<UserDto>
            {
                Items = users,
                TotalCount = totalCount,
                Page = 1,
                PageSize = 5,
            };

            _userServiceMock
                .Setup(x => x.GetUsersListAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<UserFilters>()))
                .ReturnsAsync(Result<PagedResult<UserDto>>.Success(pagedResult));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.TotalCount.Should().Be(23);
            result.Value.PageSize.Should().Be(5);
            result.Value.TotalPages.Should().Be(5); // 23 / 5 = 4.6, rounded up to 5
        }

        [Fact]
        public async Task Handle_WithValidQuery_ShouldMapUsersCorrectly()
        {
            // Arrange
            var query = new GetUsersListQuery
            {
                Page = 1,
                PageSize = 10
            };

            var users = new List<UserDto>
            {
                new UserDto
                {
                    Id = Guid.NewGuid(),
                    Email = "test@example.com",
                    FirstName = "John",
                    LastName = "Doe",
                    IsActive = true,
                    IsEmailVerified = false,
                    Role = UserRole.User,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            var pagedResult = new PagedResult<UserDto>
            {
                Items = users,
                TotalCount = users.Count,
                Page = 1,
                PageSize = 10
            };

            _userServiceMock
                .Setup(x => x.GetUsersListAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<UserFilters>()))
                .ReturnsAsync(Result<PagedResult<UserDto>>.Success(pagedResult));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Items.Should().HaveCount(1);

            var userDto = result.Value.Items.First();
            userDto.Id.Should().Be(users[0].Id);
            userDto.Email.Should().Be(users[0].Email);
            userDto.FirstName.Should().Be(users[0].FirstName);
            userDto.LastName.Should().Be(users[0].LastName);
            userDto.FullName.Should().Be($"{users[0].FirstName} {users[0].LastName}");
            userDto.Role.Should().Be(users[0].Role);
            userDto.IsActive.Should().Be(users[0].IsActive);
            userDto.IsEmailVerified.Should().Be(users[0].IsEmailVerified);
            userDto.CreatedAt.Should().Be(users[0].CreatedAt);
        }

        [Fact]
        public async Task Handle_WithValidQuery_ShouldReturnUsers()
        {
            // Arrange
            var query = new GetUsersListQuery(1, 10, new UserFilters
            {
                SearchTerm = "test",
                Role = UserRole.User,
                IsActive = true,
                IsEmailVerified = false,
                CreatedFrom = DateTime.UtcNow.AddDays(-30),
                CreatedTo = DateTime.UtcNow,
                SortBy = "FirstName",
                SortDirection = "asc"
            });

            var users = new List<UserDto>
            {
                new UserDto
                {
                    Id = Guid.NewGuid(),
                    Email = "test1@example.com",
                    FirstName = "John",
                    LastName = "Doe",
                    IsActive = true,
                    IsEmailVerified = false,
                    Role = UserRole.User,
                    CreatedAt = DateTime.UtcNow
                },
                new UserDto
                {
                    Id = Guid.NewGuid(),
                    Email = "test2@example.com",
                    FirstName = "Jane",
                    LastName = "Smith",
                    IsActive = true,
                    IsEmailVerified = false,
                    Role = UserRole.User,
                    CreatedAt = DateTime.UtcNow
                }
            };

            var pagedResult = new PagedResult<UserDto>
            {
                Items = users,
                TotalCount = 2,
                Page = 1,
                PageSize = 10,
            };

            _userServiceMock
                .Setup(x => x.GetUsersListAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<UserFilters>()))
                .ReturnsAsync(Result<PagedResult<UserDto>>.Success(pagedResult));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Items.Should().HaveCount(2);
            result.Value.TotalCount.Should().Be(2);
            result.Value.Page.Should().Be(1);
            result.Value.PageSize.Should().Be(10);
            result.Value.TotalPages.Should().Be(1);

            _userServiceMock.Verify(x => x.GetUsersListAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<UserFilters>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithZeroPageSize_ShouldUseDefaultPageSize()
        {
            // Arrange
            var query = new GetUsersListQuery
            {
                Page = 1,
                PageSize = 0
            };

            var users = new List<UserDto>();
            var pagedResult = new PagedResult<UserDto>
            {
                Items = users,
                TotalCount = 0,
                Page = 1,
                PageSize = 10
            };

            _userServiceMock
                .Setup(x => x.GetUsersListAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<UserFilters>()))
                .ReturnsAsync(Result<PagedResult<UserDto>>.Success(pagedResult));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.PageSize.Should().Be(10); // Should be set to 10
        }
    }
}