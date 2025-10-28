using Microsoft.AspNetCore.Http;
using SampleProject.Application.Interfaces;
using SampleProject.Domain.Responses;
using System.Security.Claims;

namespace SampleProject.Infrastructure.Implementations
{
    /// <summary>
    /// Implementation of current user service
    /// </summary>
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public CurrentUserResponse? GetCurrentUser()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            var user = httpContext.User;
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = user.FindFirst(ClaimTypes.Name)?.Value;
            var email = user.FindFirst(ClaimTypes.Email)?.Value;
            var firstName = user.FindFirst(ClaimTypes.GivenName)?.Value;
            var lastName = user.FindFirst(ClaimTypes.Surname)?.Value;
            var role = user.FindFirst(ClaimTypes.Role)?.Value;

            var fullName = string.IsNullOrEmpty(firstName) && string.IsNullOrEmpty(lastName)
                ? null
                : $"{firstName} {lastName}".Trim();

            return new CurrentUserResponse
            {
                UserId = userId,
                Username = username,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                FullName = fullName,
                Role = role
            };
        }

        public string? GetCurrentUserEmail()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            return httpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;
        }

        public string? GetCurrentUserId()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            return httpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public string? GetCurrentUserRole()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated != true)
            {
                return String.Empty;
            }

            return httpContext.User.FindFirst(ClaimTypes.Role)?.Value;
        }

        public bool HasRole(string role)
        {
            var userRole = GetCurrentUserRole();
            return userRole == role;
        }

        public bool IsAdmin()
        {
            return HasRole(StringMessages.AdminRole);
        }

        public bool IsAuthenticated()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            return httpContext?.User?.Identity?.IsAuthenticated == true;
        }
    }
}