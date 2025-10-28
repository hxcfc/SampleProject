using Microsoft.EntityFrameworkCore;
using SampleProject.Application.Interfaces;
using SampleProject.Application.Interfaces.SampleProject.Authorization;
using SampleProject.Application.Interfaces.Persistence;
using SampleProject.Application.Dto;
using SampleProject.Domain.Entities;
using SampleProject.Domain.Enums;
using Common.Shared;

namespace SampleProject.Infrastructure.Implementations
{
    /// <summary>
    /// Implementation of user authorization operations
    /// </summary>
    public class Authorization : IAuthorization
    {
        private readonly ILogger<Authorization> _logger;
        private readonly IPasswordService _passwordService;
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Initializes a new instance of the <see cref="Authorization"/> class
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <param name="unitOfWork">Unit of work for database operations</param>
        /// <param name="passwordService">Password hashing service</param>
        public Authorization(ILogger<Authorization> logger, IUnitOfWork unitOfWork, IPasswordService passwordService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
        }

        /// <inheritdoc />
        public async Task<string?> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                _logger.LogInformation(StringMessages.RefreshingToken);

                if (string.IsNullOrWhiteSpace(refreshToken))
                {
                    _logger.LogWarning(StringMessages.InvalidRefreshTokenProvided);
                    return null;
                }

                // Validate refresh token and get user information
                var user = await ValidateRefreshTokenAsync(refreshToken);
                if (user == null)
                {
                    _logger.LogWarning(StringMessages.InvalidOrExpiredRefreshToken);
                    return null;
                }

                // Check if user is active
                if (!user.IsActive)
                {
                    _logger.LogWarning("Account is not active for user: {UserId}", user.Id);
                    return null;
                }

                // Generate new refresh token
                var newRefreshToken = GenerateRefreshToken();
                var refreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // 7 days expiry

                // Save new refresh token to database (replace old one)
                var refreshTokenSaved = await SaveRefreshTokenAsync(
                    user.Id, 
                    newRefreshToken, 
                    refreshTokenExpiryTime);

                if (!refreshTokenSaved)
                {
                    _logger.LogWarning("Failed to save new refresh token for user: {UserId}", user.Id);
                    return null;
                }

                _logger.LogInformation(StringMessages.TokenRefreshedSuccessfully);
                return newRefreshToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, StringMessages.ErrorOccurredWhileRefreshingToken);
                return null;
            }
        }

        /// <summary>
        /// Generates a new refresh token
        /// </summary>
        /// <returns>Refresh token</returns>
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        /// <inheritdoc />
        public async Task<bool> RevokeRefreshTokenAsync(Guid userId)
        {
            try
            {
                _logger.LogInformation(StringMessages.RevokingRefreshTokenForUser, userId);

                var userRepository = _unitOfWork.Repository<UserEntity>();
                var userEntity = await userRepository.GetByIdAsync(userId);

                if (userEntity == null)
                {
                    _logger.LogWarning("User not found for ID: {UserId}", userId);
                    return false;
                }

                userEntity.RefreshToken = null;
                userEntity.RefreshTokenExpiryTime = null;
                userEntity.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(StringMessages.RefreshTokenRevokedSuccessfully, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, StringMessages.ErrorOccurredWhileRevokingRefreshToken, userId);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> SaveRefreshTokenAsync(Guid userId, string refreshToken, DateTime expiryTime)
        {
            try
            {
                _logger.LogInformation(StringMessages.SavingRefreshTokenForUser, userId);

                var userRepository = _unitOfWork.Repository<UserEntity>();
                var userEntity = await userRepository.GetByIdAsync(userId);

                if (userEntity == null)
                {
                    _logger.LogWarning(StringMessages.UserNotFoundForEmail, userId);
                    return false;
                }

                userEntity.RefreshToken = refreshToken;
                userEntity.RefreshTokenExpiryTime = expiryTime;
                userEntity.RefreshTokenUseCount = 0; // Reset use count for new token
                userEntity.RefreshTokenLastUsedAt = null;
                userEntity.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(StringMessages.RefreshTokenSavedSuccessfully, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, StringMessages.ErrorOccurredWhileSavingRefreshToken, userId);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<UserDto?> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(StringMessages.ValidatingCredentialsForEmail, email);

                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    _logger.LogWarning(StringMessages.InvalidCredentialsProvidedForEmail, email);
                    return null;
                }

                // Get user from database
                var userRepository = _unitOfWork.Repository<UserEntity>();
                var userEntity = await userRepository.GetFirstAsync(u =>
                    u.Email.ToLower() == email.ToLower());

                if (userEntity == null)
                {
                    _logger.LogWarning(StringMessages.UserNotFoundForEmail, email);
                    return null;
                }

                // Verify password
                if (!_passwordService.VerifyPassword(password, userEntity.PasswordHash, userEntity.PasswordSalt))
                {
                    _logger.LogWarning(StringMessages.InvalidPasswordForEmail, email);
                    return null;
                }

                // Update last login
                userEntity.LastLoginAt = DateTime.UtcNow;
                userEntity.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Map to DTO
                var userDto = new UserDto
                {
                    Id = userEntity.Id,
                    Email = userEntity.Email,
                    FirstName = userEntity.FirstName,
                    LastName = userEntity.LastName,
                    IsActive = userEntity.IsActive,
                    IsEmailVerified = userEntity.IsEmailVerified,
                    LastLoginAt = userEntity.LastLoginAt,
                    CreatedAt = userEntity.CreatedAt,
                    UpdatedAt = userEntity.UpdatedAt,
                    Role = userEntity.Role // Use enum flags directly
                };

                _logger.LogInformation(StringMessages.CredentialsValidatedSuccessfullyForEmail, email);
                return userDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, StringMessages.ErrorOccurredWhileValidatingCredentialsForEmail, email);
                return null;
            }
        }

        /// <inheritdoc />
        public async Task<UserDto?> ValidateRefreshTokenAsync(string refreshToken)
        {
            try
            {
                _logger.LogInformation(StringMessages.ValidatingRefreshToken);

                if (string.IsNullOrWhiteSpace(refreshToken))
                {
                    _logger.LogWarning(StringMessages.InvalidRefreshTokenProvided);
                    return null;
                }

                var userRepository = _unitOfWork.Repository<UserEntity>();
                var userEntity = await userRepository.GetFirstAsync(u =>
                    u.RefreshToken == refreshToken &&
                    u.RefreshTokenExpiryTime > DateTime.UtcNow &&
                    u.IsActive);

                if (userEntity == null)
                {
                    _logger.LogWarning(StringMessages.InvalidOrExpiredRefreshToken);
                    return null;
                }

                // Track token usage
                userEntity.RefreshTokenUseCount++;
                userEntity.RefreshTokenLastUsedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();

                // Map to DTO
                var userDto = new UserDto
                {
                    Id = userEntity.Id,
                    Email = userEntity.Email,
                    FirstName = userEntity.FirstName,
                    LastName = userEntity.LastName,
                    IsActive = userEntity.IsActive,
                    IsEmailVerified = userEntity.IsEmailVerified,
                    LastLoginAt = userEntity.LastLoginAt,
                    CreatedAt = userEntity.CreatedAt,
                    UpdatedAt = userEntity.UpdatedAt,
                    Role = userEntity.Role
                };

                _logger.LogInformation(StringMessages.RefreshTokenValidatedSuccessfully, userEntity.Id);
                return userDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, StringMessages.ErrorOccurredWhileValidatingRefreshToken);
                return null;
            }
        }
    }
}