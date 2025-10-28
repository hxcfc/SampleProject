using Microsoft.Extensions.Logging;
using SampleProject.Application.Interfaces;
using SampleProject.Domain.Common;
using SampleProject.Application.Dto;
using SampleProject.Domain.Entities;
using SampleProject.Domain.Enums;
using SampleProject.Infrastructure.Interfaces;

namespace SampleProject.Infrastructure.Implementations
{
    /// <summary>
    /// Service for user management operations
    /// </summary>
    public class UserService : IUserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly IPasswordService _passwordService;
        private readonly IUserRepository _userRepository;

        public UserService(
            ILogger<UserService> logger,
            IPasswordService passwordService,
            IUserRepository userRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        /// <inheritdoc />
        public async Task<Result<UserDto>> CreateUserAsync(string email, string firstName, string lastName, string password)
        {
            try
            {
                _logger.LogInformation(StringMessages.AttemptingUserRegistration, email);

                // Check if user already exists
                if (await UserExistsAsync(email))
                {
                    _logger.LogWarning(StringMessages.UserAlreadyExists, email);
                    return Result<UserDto>.Failure(StringMessages.UserAlreadyExists);
                }

                // Hash password
                var (passwordHash, passwordSalt) = _passwordService.HashPassword(password);

                // Create user entity
                var user = new UserEntity
                {
                    Id = Guid.NewGuid(),
                    Email = email.ToLowerInvariant(),
                    FirstName = firstName,
                    LastName = lastName,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    Role = UserRole.User, // Default role for new users
                    IsActive = false, // User needs to be activated via email (future feature)
                    IsEmailVerified = false, // Email verification required
                    CreatedAt = DateTime.UtcNow
                };

                // Save user to database
                var savedUser = await _userRepository.CreateAsync(user);
                if (savedUser == null)
                {
                    _logger.LogError(StringMessages.FailedToCreateUser, email);
                    return Result<UserDto>.Failure(StringMessages.FailedToCreateUser);
                }

                // Convert to DTO
                var userDto = new UserDto
                {
                    Id = savedUser.Id,
                    Email = savedUser.Email,
                    FirstName = savedUser.FirstName,
                    LastName = savedUser.LastName,
                    CreatedAt = savedUser.CreatedAt
                };

                _logger.LogInformation(StringMessages.UserRegisteredSuccessfully, savedUser.Id, email);
                return Result<UserDto>.Success(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, StringMessages.ErrorOccurredDuringUserRegistration, email);
                return Result<UserDto>.Failure(StringMessages.ErrorOccurredDuringUserRegistration);
            }
        }

        /// <inheritdoc />
        public async Task<Result<UserDto>> GetUserByIdAsync(Guid userId)
        {
            try
            {
                _logger.LogInformation(StringMessages.AttemptingToGetUserById, userId);

                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning(StringMessages.UserNotFoundForId, userId);
                    return Result<UserDto>.Failure(StringMessages.UserNotFound);
                }

                var userDto = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    CreatedAt = user.CreatedAt
                };

                _logger.LogInformation(StringMessages.UserRetrievedSuccessfully, userId);
                return Result<UserDto>.Success(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, StringMessages.ErrorOccurredWhileGettingUserById, userId);
                return Result<UserDto>.Failure(StringMessages.ErrorOccurredWhileGettingUserById);
            }
        }

        /// <inheritdoc />
        public async Task<Result<UserEntity>> GetUserByEmailAsync(string email)
        {
            try
            {
                _logger.LogInformation(StringMessages.AttemptingToGetUserByEmail, email);

                var user = await _userRepository.GetByEmailAsync(email);
                if (user == null)
                {
                    _logger.LogWarning(StringMessages.UserNotFoundForEmail, email);
                    return Result<UserEntity>.Failure(StringMessages.UserNotFound);
                }

                _logger.LogInformation(StringMessages.UserRetrievedSuccessfullyByEmail, email);
                return Result<UserEntity>.Success(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, StringMessages.ErrorOccurredWhileGettingUserByEmail, email);
                return Result<UserEntity>.Failure(StringMessages.ErrorOccurredWhileGettingUserByEmail);
            }
        }

        /// <inheritdoc />
        public async Task<bool> UserExistsAsync(string email)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(email.ToLowerInvariant());
                return user != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, StringMessages.ErrorOccurredWhileCheckingUserExistence, email);
                return false;
            }
        }

        public async Task<Result<PagedResult<UserDto>>> GetUsersListAsync(int page, int pageSize, UserFilters filters)
        {
            _logger.LogInformation(StringMessages.GettingUsersList, page, pageSize);
            try
            {
                var pagedResult = await _userRepository.GetUsersListAsync(page, pageSize, filters);

                var userDtos = pagedResult.Items.Select(UserDto.FromEntity).ToList();

                var result = new PagedResult<UserDto>
                {
                    Items = userDtos,
                    Page = pagedResult.Page,
                    PageSize = pagedResult.PageSize,
                    TotalCount = pagedResult.TotalCount
                };

                _logger.LogInformation(StringMessages.UsersListRetrieved, userDtos.Count, pagedResult.TotalCount);
                return Result<PagedResult<UserDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, StringMessages.ErrorOccurredWhileGettingUsersList);
                return Result<PagedResult<UserDto>>.Failure(StringMessages.ErrorOccurredWhileGettingUsersList);
            }
        }

        public async Task<Result<UserDto>> UpdateUserAsync(Guid userId, string? firstName, string? lastName, string? email, bool? isActive, bool? isEmailVerified, UserRole? role)
        {
            _logger.LogInformation(StringMessages.UpdatingUser, userId);
            try
            {
                var userEntity = await _userRepository.GetByIdAsync(userId);
                if (userEntity == null)
                {
                    _logger.LogWarning(StringMessages.UserNotFoundForId, userId);
                    return Result<UserDto>.Failure(StringMessages.UserNotFoundForId);
                }

                // Check if email is being changed and if it's already taken
                if (!string.IsNullOrEmpty(email))
                {
                    // Normalize email to lowercase for consistency
                    var normalizedEmail = email.ToLowerInvariant();

                    if (!string.Equals(normalizedEmail, userEntity.Email, StringComparison.Ordinal))
                    {
                        if (await _userRepository.GetByEmailAsync(normalizedEmail) != null)
                        {
                            _logger.LogWarning(StringMessages.UserAlreadyExists, normalizedEmail);
                            return Result<UserDto>.Failure(StringMessages.UserAlreadyExists);
                        }
                        userEntity.Email = normalizedEmail;
                    }
                }

                // Update basic fields
                if (!string.IsNullOrEmpty(firstName))
                    userEntity.FirstName = firstName;
                
                if (!string.IsNullOrEmpty(lastName))
                    userEntity.LastName = lastName;

                // Update admin-only fields (these should be validated at controller level)
                if (isActive.HasValue)
                    userEntity.IsActive = isActive.Value;
                
                if (isEmailVerified.HasValue)
                    userEntity.IsEmailVerified = isEmailVerified.Value;

                userEntity.UpdatedAt = DateTime.UtcNow;

                var updatedUser = await _userRepository.UpdateUserAsync(userEntity);
                if (updatedUser == null)
                {
                    _logger.LogError(StringMessages.FailedToUpdateUser, userId);
                    return Result<UserDto>.Failure(StringMessages.FailedToUpdateUser);
                }

                _logger.LogInformation(StringMessages.UserUpdatedSuccessfully, userId);
                return Result<UserDto>.Success(UserDto.FromEntity(updatedUser));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, StringMessages.ErrorOccurredWhileUpdatingUser, userId);
                return Result<UserDto>.Failure(StringMessages.ErrorOccurredWhileUpdatingUser);
            }
        }

        public async Task<Result<bool>> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
        {
            _logger.LogInformation(StringMessages.ChangingPasswordForUser, userId);
            try
            {
                var userEntity = await _userRepository.GetByIdAsync(userId);
                if (userEntity == null)
                {
                    _logger.LogWarning(StringMessages.UserNotFoundForId, userId);
                    return Result<bool>.Failure(StringMessages.UserNotFoundForId);
                }

                // Verify current password
                var isCurrentPasswordValid = _passwordService.VerifyPassword(currentPassword, userEntity.PasswordHash, userEntity.PasswordSalt);
                if (!isCurrentPasswordValid)
                {
                    _logger.LogWarning(StringMessages.InvalidCurrentPassword, userId);
                    return Result<bool>.Failure(StringMessages.InvalidCurrentPassword);
                }

                // Hash new password
                var (passwordHash, passwordSalt) = _passwordService.HashPassword(newPassword);

                // Update password
                var success = await _userRepository.UpdatePasswordAsync(userId, passwordHash, passwordSalt);
                if (!success)
                {
                    _logger.LogError(StringMessages.FailedToChangePassword, userId);
                    return Result<bool>.Failure(StringMessages.FailedToChangePassword);
                }

                _logger.LogInformation(StringMessages.PasswordChangedSuccessfully, userId);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, StringMessages.ErrorOccurredWhileChangingPassword, userId);
                return Result<bool>.Failure(StringMessages.ErrorOccurredWhileChangingPassword);
            }
        }

        public async Task<Result<UserDto>> ChangeUserRoleAsync(Guid userId, UserRole newRole)
        {
            _logger.LogInformation(StringMessages.ChangingUserRole, userId, newRole);
            try
            {
                var userEntity = await _userRepository.GetByIdAsync(userId);
                if (userEntity == null)
                {
                    _logger.LogWarning(StringMessages.UserNotFoundForId, userId);
                    return Result<UserDto>.Failure(StringMessages.UserNotFoundForId);
                }

                userEntity.Role = newRole;
                userEntity.UpdatedAt = DateTime.UtcNow;

                var updatedUser = await _userRepository.UpdateUserAsync(userEntity);
                if (updatedUser == null)
                {
                    _logger.LogError(StringMessages.FailedToChangeUserRole, userId);
                    return Result<UserDto>.Failure(StringMessages.FailedToChangeUserRole);
                }

                _logger.LogInformation(StringMessages.UserRoleChangedSuccessfully, userId, newRole);
                return Result<UserDto>.Success(UserDto.FromEntity(updatedUser));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, StringMessages.ErrorOccurredWhileChangingUserRole, userId);
                return Result<UserDto>.Failure(StringMessages.ErrorOccurredWhileChangingUserRole);
            }
        }
    }
}
