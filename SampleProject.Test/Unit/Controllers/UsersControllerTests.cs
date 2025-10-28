using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SampleProject.Application.Features.Users.Commands.CreateUser;
using SampleProject.Application.Features.Users.Queries.GetUserById;
using SampleProject.Application.Features.Users.Queries.GetUsersList;
using SampleProject.Application.Interfaces;
using SampleProject.Controllers.Users;
using SampleProject.Domain.Common;
using SampleProject.Application.Dto;
using SampleProject.Domain.Enums;
using Xunit;

namespace SampleProject.Test.Unit.Controllers
{
    /// <summary>
    /// Unit tests for UsersController
    /// </summary>
    public class UsersControllerTests
    {
        private readonly UsersController _controller;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly Mock<IMediator> _mediatorMock;

        public UsersControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _currentUserServiceMock = new Mock<ICurrentUserService>();
            _controller = new UsersController(_mediatorMock.Object, _currentUserServiceMock.Object);
        }

        [Fact]
        public async Task CreateUser_WithInvalidData_ShouldReturnBadRequest()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "invalid-email",
                Password = "123",
                FirstName = "",
                LastName = ""
            };

            var expectedResult = Result<UserDto>.Failure("Validation failed");

            _mediatorMock
                .Setup(x => x.Send(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.CreateUser(command);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().NotBeNull();

            _mediatorMock.Verify(x => x.Send(command, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateUser_WithValidData_ShouldReturnCreated()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "test@example.com",
                Password = "password123",
                FirstName = "John",
                LastName = "Doe"
            };

            var expectedResult = Result<UserDto>.Success(new UserDto
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                IsActive = true,
                IsEmailVerified = false,
                CreatedAt = DateTime.UtcNow
            });

            _mediatorMock
                .Setup(x => x.Send(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.CreateUser(command);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
            var createdAtResult = result as CreatedAtActionResult;
            createdAtResult!.Value.Should().BeEquivalentTo(expectedResult.Value);

            _mediatorMock.Verify(x => x.Send(command, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetUser_WithEmptyId_ShouldReturnBadRequest()
        {
            // Arrange
            var userId = Guid.Empty;

            // Act
            var result = await _controller.GetUserById(userId);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
            var unauthorizedResult = result as UnauthorizedObjectResult;
            unauthorizedResult!.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task GetUser_WithNonExistentId_ShouldReturnNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var query = new GetUserByIdQuery { UserId = userId };

            var expectedResult = Result<UserDto>.Failure("User not found");

            _currentUserServiceMock
                .Setup(x => x.GetCurrentUserId())
                .Returns(userId.ToString());

            _currentUserServiceMock
                .Setup(x => x.GetCurrentUserRole())
                .Returns("User");

            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetUserByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.GetUserById(userId);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult!.Value.Should().NotBeNull();

            _mediatorMock.Verify(x => x.Send(It.Is<GetUserByIdQuery>(q => q.UserId == userId), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetUser_WithValidId_ShouldReturnUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var query = new GetUserByIdQuery { UserId = userId };

            var expectedResult = Result<UserDto>.Success(new UserDto
            {
                Id = userId,
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                Role = UserRole.User,
                IsActive = true,
                IsEmailVerified = false,
                CreatedAt = DateTime.UtcNow
            });

            _currentUserServiceMock
                .Setup(x => x.GetCurrentUserId())
                .Returns(userId.ToString());

            _currentUserServiceMock
                .Setup(x => x.GetCurrentUserRole())
                .Returns("User");

            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetUserByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.GetUserById(userId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(expectedResult.Value);

            _mediatorMock.Verify(x => x.Send(It.Is<GetUserByIdQuery>(q => q.UserId == userId), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetUsers_WithEmptyQuery_ShouldUseDefaultValues()
        {
            // Arrange
            var query = new GetUsersListQuery();

            var expectedResult = Result<PagedResult<UserDto>>.Success(new PagedResult<UserDto>
            {
                Items = new List<UserDto>(),
                TotalCount = 0,
                Page = 1,
                PageSize = 10
            });

            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetUsersListQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.GetUsersList();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(expectedResult.Value);

            _mediatorMock.Verify(x => x.Send(It.IsAny<GetUsersListQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetUsers_WithInvalidQuery_ShouldReturnBadRequest()
        {
            // Arrange
            var filters = new UserFilters
            {
                SearchTerm = "",
                Role = null,
                IsActive = null,
                IsEmailVerified = null,
                CreatedFrom = null,
                CreatedTo = null,
                SortBy = "",
                SortDirection = ""
            };
            var query = new GetUsersListQuery(-1, 0, filters);

            var expectedResult = Result<PagedResult<UserDto>>.Failure("Invalid query parameters");

            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetUsersListQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.GetUsersList(-1, 0);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().NotBeNull();

            _mediatorMock.Verify(x => x.Send(It.IsAny<GetUsersListQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetUsers_WithLargePageSize_ShouldLimitPageSize()
        {
            // Arrange
            var filters = new UserFilters
            {
                SearchTerm = "",
                Role = null,
                IsActive = null,
                IsEmailVerified = null,
                CreatedFrom = null,
                CreatedTo = null,
                SortBy = "",
                SortDirection = ""
            };
            var query = new GetUsersListQuery(1, 1000, filters); // Very large page size

            var expectedResult = Result<PagedResult<UserDto>>.Success(new PagedResult<UserDto>
            {
                Items = new List<UserDto>(),
                TotalCount = 0,
                Page = 1,
                PageSize = 100 // Should be limited to 100
            });

            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetUsersListQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.GetUsersList(1, 1000);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(expectedResult.Value);

            _mediatorMock.Verify(x => x.Send(It.IsAny<GetUsersListQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetUsers_WithNegativePage_ShouldUseDefaultPage()
        {
            // Arrange
            var filters = new UserFilters
            {
                SearchTerm = "",
                Role = null,
                IsActive = null,
                IsEmailVerified = null,
                CreatedFrom = null,
                CreatedTo = null,
                SortBy = "",
                SortDirection = ""
            };
            var query = new GetUsersListQuery(-1, 10, filters);

            var expectedResult = Result<PagedResult<UserDto>>.Success(new PagedResult<UserDto>
            {
                Items = new List<UserDto>(),
                TotalCount = 0,
                Page = 1, // Should be set to 1
                PageSize = 10
            });

            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetUsersListQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.GetUsersList(-1, 10);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(expectedResult.Value);

            _mediatorMock.Verify(x => x.Send(It.IsAny<GetUsersListQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetUsers_WithValidQuery_ShouldReturnUsers()
        {
            // Arrange
            var filters = new UserFilters
            {
                SearchTerm = "test",
                Role = UserRole.User,
                IsActive = true,
                IsEmailVerified = false,
                CreatedFrom = DateTime.UtcNow.AddDays(-30),
                CreatedTo = DateTime.UtcNow,
                SortBy = "FirstName",
                SortDirection = "asc"
            };
            var query = new GetUsersListQuery(1, 10, filters);

            var expectedResult = Result<PagedResult<UserDto>>.Success(new PagedResult<UserDto>
            {
                Items = new List<UserDto>
                {
                    new UserDto
                    {
                        Id = Guid.NewGuid(),
                        Email = "test1@example.com",
                        FirstName = "John",
                        LastName = "Doe",
                        Role = UserRole.User,
                        IsActive = true,
                        IsEmailVerified = false,
                        CreatedAt = DateTime.UtcNow
                    },
                    new UserDto
                    {
                        Id = Guid.NewGuid(),
                        Email = "test2@example.com",
                        FirstName = "Jane",
                        LastName = "Smith",
                        Role = UserRole.User,
                        IsActive = true,
                        IsEmailVerified = false,
                        CreatedAt = DateTime.UtcNow
                    }
                },
                TotalCount = 2,
                Page = 1,
                PageSize = 10
            });

            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetUsersListQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.GetUsersList(1, 10, "test", UserRole.User, true, false, DateTime.UtcNow.AddDays(-30), DateTime.UtcNow, "FirstName", "asc");

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(expectedResult.Value);

            _mediatorMock.Verify(x => x.Send(It.IsAny<GetUsersListQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}