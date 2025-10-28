using Microsoft.AspNetCore.Http;
using SampleProject.Application.Interfaces;
using SampleProject.Domain.Dto;
using SampleProject.Domain.Responses;
using System.Security.Claims;
using Common.Shared;

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
            var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

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
                Roles = roles
            };
        }

        public string? GetCurrentUserId()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            return httpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public string? GetCurrentUserEmail()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            return httpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;
        }

        public List<string> GetCurrentUserRoles()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated != true)
            {
                return new List<string>();
            }

            return httpContext.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        }

        public bool IsAuthenticated()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            return httpContext?.User?.Identity?.IsAuthenticated == true;
        }

        public bool IsAdmin()
        {
            return HasRole(StringMessages.AdminRole);
        }

        public bool HasRole(string role)
        {
            var roles = GetCurrentUserRoles();
            return roles.Contains(role, StringComparer.OrdinalIgnoreCase);
        }
    }
}
