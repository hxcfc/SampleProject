using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SampleProject.Domain.Entities;
using SampleProject.Application.Dto;
using SampleProject.Infrastructure.Interfaces;
using SampleProject.Persistence.Data;
using Common.Shared;

namespace SampleProject.Infrastructure.Implementations
{
    /// <summary>
    /// Repository implementation for user data access
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(ApplicationDbContext context, ILogger<UserRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<UserEntity?> CreateAsync(UserEntity user)
        {
            try
            {
                _logger.LogInformation(StringMessages.CreatingUserInDatabase, user.Email);

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation(StringMessages.UserCreatedInDatabase, user.Id, user.Email);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, StringMessages.ErrorOccurredWhileCreatingUserInDatabase, user.Email);
                return null;
            }
        }

        /// <inheritdoc />
        public async Task<UserEntity?> GetByIdAsync(Guid id)
        {
            try
            {
                _logger.LogInformation(StringMessages.RetrievingUserByIdFromDatabase, id);

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user != null)
                {
                    _logger.LogInformation(StringMessages.UserRetrievedByIdFromDatabase, id);
                }
                else
                {
                    _logger.LogWarning(StringMessages.UserNotFoundByIdInDatabase, id);
                }

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, StringMessages.ErrorOccurredWhileRetrievingUserByIdFromDatabase, id);
                return null;
            }
        }

        /// <inheritdoc />
        public async Task<UserEntity?> GetByEmailAsync(string email)
        {
            try
            {
                _logger.LogInformation(StringMessages.RetrievingUserByEmailFromDatabase, email);

                var user = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant());

                if (user != null)
                {
                    _logger.LogInformation(StringMessages.UserRetrievedByEmailFromDatabase, email);
                }
                else
                {
                    _logger.LogWarning(StringMessages.UserNotFoundByEmailInDatabase, email);
                }

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, StringMessages.ErrorOccurredWhileRetrievingUserByEmailFromDatabase, email);
                return null;
            }
        }

        /// <inheritdoc />
        public async Task<bool> ExistsByEmailAsync(string email)
        {
            try
            {
                var normalized = email.ToLowerInvariant();
                return await _context.Users
                    .AsNoTracking()
                    .AnyAsync(u => u.Email == normalized);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, StringMessages.ErrorOccurredWhileRetrievingUserByEmailFromDatabase, email);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<UserEntity?> UpdateAsync(UserEntity user)
        {
            try
            {
                _logger.LogInformation(StringMessages.UpdatingUserInDatabase, user.Id, user.Email);

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation(StringMessages.UserUpdatedInDatabase, user.Id, user.Email);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, StringMessages.ErrorOccurredWhileUpdatingUserInDatabase, user.Id, user.Email);
                return null;
            }
        }

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                _logger.LogInformation(StringMessages.DeletingUserFromDatabase, id);

                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    _logger.LogWarning(StringMessages.UserNotFoundForDeletion, id);
                    return false;
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation(StringMessages.UserDeletedFromDatabase, id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, StringMessages.ErrorOccurredWhileDeletingUserFromDatabase, id);
                return false;
            }
        }

        public async Task<PagedResult<UserEntity>> GetUsersListAsync(int page, int pageSize, UserFilters filters)
        {
            _logger.LogInformation(StringMessages.GettingUsersListFromDatabase, page, pageSize);
            try
            {
                var query = _context.Users.AsQueryable();

                // Apply search filter
                if (!string.IsNullOrEmpty(filters.SearchTerm))
                {
                    var searchTerm = filters.SearchTerm.ToLower();
                    query = query.Where(u => 
                        u.FirstName.ToLower().Contains(searchTerm) ||
                        u.LastName.ToLower().Contains(searchTerm) ||
                        u.Email.ToLower().Contains(searchTerm));
                }

                // Apply role filter
                if (filters.Role.HasValue)
                {
                    query = query.Where(u => u.Role == filters.Role.Value);
                }

                // Apply active status filter
                if (filters.IsActive.HasValue)
                {
                    query = query.Where(u => u.IsActive == filters.IsActive.Value);
                }

                // Apply email verification filter
                if (filters.IsEmailVerified.HasValue)
                {
                    query = query.Where(u => u.IsEmailVerified == filters.IsEmailVerified.Value);
                }

                // Apply date range filter
                if (filters.CreatedFrom.HasValue)
                {
                    query = query.Where(u => u.CreatedAt >= filters.CreatedFrom.Value);
                }

                if (filters.CreatedTo.HasValue)
                {
                    query = query.Where(u => u.CreatedAt <= filters.CreatedTo.Value);
                }

                // Apply sorting
                query = ApplySorting(query, filters.SortBy, filters.SortDirection);

                // Get total count
                var totalCount = await query.CountAsync();

                // Apply pagination
                var users = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                _logger.LogInformation(StringMessages.UsersListRetrievedFromDatabase, users.Count, totalCount);

                return new PagedResult<UserEntity>
                {
                    Items = users,
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, StringMessages.ErrorOccurredWhileGettingUsersListFromDatabase);
                throw;
            }
        }

        private static IQueryable<UserEntity> ApplySorting(IQueryable<UserEntity> query, string? sortBy, string? sortDirection)
        {
            var isDescending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return sortBy?.ToLowerInvariant() switch
            {
                "firstname" => isDescending ? query.OrderByDescending(u => u.FirstName) : query.OrderBy(u => u.FirstName),
                "lastname" => isDescending ? query.OrderByDescending(u => u.LastName) : query.OrderBy(u => u.LastName),
                "email" => isDescending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
                "isactive" => isDescending ? query.OrderByDescending(u => u.IsActive) : query.OrderBy(u => u.IsActive),
                "isemailverified" => isDescending ? query.OrderByDescending(u => u.IsEmailVerified) : query.OrderBy(u => u.IsEmailVerified),
                "createdat" or _ => isDescending ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt)
            };
        }

        public async Task<UserEntity?> UpdateUserAsync(UserEntity user)
        {
            _logger.LogInformation(StringMessages.UpdatingUserInDatabase, user.Id, user.Email);
            try
            {
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                _logger.LogInformation(StringMessages.UserUpdatedInDatabase, user.Id, user.Email);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, StringMessages.ErrorOccurredWhileUpdatingUserInDatabase, user.Id, user.Email);
                return null;
            }
        }

        public async Task<bool> UpdatePasswordAsync(Guid userId, string passwordHash, string passwordSalt)
        {
            _logger.LogInformation(StringMessages.UpdatingPasswordForUser, userId);
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning(StringMessages.UserNotFoundForPasswordUpdate, userId);
                    return false;
                }

                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                _logger.LogInformation(StringMessages.PasswordUpdatedForUser, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, StringMessages.ErrorOccurredWhileUpdatingPassword, userId);
                return false;
            }
        }
    }
}
