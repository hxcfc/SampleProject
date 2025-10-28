using SampleProject.Application.Interfaces;
using SampleProject.Infrastructure.Interfaces;
using SampleProject.Domain.Responses;
using SampleProject.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Serilog;
using Common.Shared;

namespace SampleProject.Infrastructure.Implementations
{
    /// <summary>
    /// JWT service implementation with ASP.NET Core specific functionality
    /// </summary>
    public class JwtService : IExtendedJwtService
    {
        private readonly JwtOptions _jwtOptions;
        private readonly JwtSecurityTokenHandler _tokenHandler;

        /// <summary>
        /// Initializes a new instance of the JwtService class
        /// </summary>
        /// <param name="jwtOptions">JWT options</param>
        public JwtService(IOptions<JwtOptions> jwtOptions)
        {
            _jwtOptions = jwtOptions.Value;
            _tokenHandler = new JwtSecurityTokenHandler();
        }

        /// <summary>
        /// Generates a JWT token for the specified user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="username">Username</param>
        /// <param name="email">User email</param>
        /// <param name="firstName">User first name</param>
        /// <param name="lastName">User last name</param>
        /// <param name="roles">User roles as enum flags</param>
        /// <returns>Token response</returns>
        public async Task<TokenResponse> GenerateTokenAsync(string userId, string username, string email, string firstName, string lastName, UserRole roles)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId),
                new(ClaimTypes.Name, username),
                new(ClaimTypes.Email, email),
                new(ClaimTypes.GivenName, firstName),
                new(ClaimTypes.Surname, lastName),
                new(JwtRegisteredClaimNames.Sub, userId),
                new(JwtRegisteredClaimNames.Email, email),
                new(JwtRegisteredClaimNames.GivenName, firstName),
                new(JwtRegisteredClaimNames.FamilyName, lastName),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            // Add role claims from enum flags
            var roleNames = roles.GetRoleNames();
            foreach (var role in roleNames)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes),
                Issuer = _jwtOptions.Issuer,
                Audience = _jwtOptions.Audience,
                SigningCredentials = credentials
            };

            var token = _tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = _tokenHandler.WriteToken(token);
            var refreshToken = GenerateRefreshToken();

            Log.Information(StringMessages.JwtTokenGeneratedForUser, 
                username, string.Join(", ", roleNames));

            return await Task.FromResult(new TokenResponse
            {
                AccessToken = tokenString,
                RefreshToken = refreshToken,
                TokenType = StringMessages.BearerTokenType,
                ExpiresIn = _jwtOptions.ExpirationMinutes * 60,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes)
            });
        }

        /// <summary>
        /// Generates a refresh token
        /// </summary>
        /// <returns>Refresh token</returns>
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        /// <summary>
        /// Validates a JWT token
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>True if valid, false otherwise</returns>
        public bool ValidateToken(string token)
        {
            try
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _jwtOptions.Issuer,
                    ValidAudience = _jwtOptions.Audience,
                    IssuerSigningKey = key,
                    ClockSkew = TimeSpan.Zero
                };

                _tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);
                return true;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, StringMessages.JwtTokenValidationFailed);
                return false;
            }
        }

        /// <summary>
        /// Gets user ID from JWT token
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>User ID or null if invalid</returns>
        public string? GetUserIdFromToken(string token)
        {
            try
            {
                var jwtToken = _tokenHandler.ReadJwtToken(token);
                return jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, StringMessages.FailedToGetUserIdFromToken);
                return null;
            }
        }

        /// <summary>
        /// Gets username from JWT token
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>Username or null if invalid</returns>
        public string? GetUsernameFromToken(string token)
        {
            try
            {
                var jwtToken = _tokenHandler.ReadJwtToken(token);
                return jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, StringMessages.FailedToGetUsernameFromToken);
                return null;
            }
        }

        /// <summary>
        /// Gets roles from JWT token
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>List of roles or empty list if invalid</returns>
        public IEnumerable<string> GetRolesFromToken(string token)
        {
            try
            {
                var jwtToken = _tokenHandler.ReadJwtToken(token);
                return jwtToken.Claims
                    .Where(x => x.Type == ClaimTypes.Role)
                    .Select(x => x.Value)
                    .ToList();
            }
            catch (Exception ex)
            {
                Log.Warning(ex, StringMessages.FailedToGetRolesFromToken);
                return new List<string>();
            }
        }

        /// <summary>
        /// Gets email from JWT token
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>Email or null if invalid</returns>
        public string? GetEmailFromToken(string token)
        {
            try
            {
                var jwtToken = _tokenHandler.ReadJwtToken(token);
                return jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email || x.Type == JwtRegisteredClaimNames.Email)?.Value;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, StringMessages.FailedToGetEmailFromToken);
                return null;
            }
        }

        /// <summary>
        /// Gets first name from JWT token
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>First name or null if invalid</returns>
        public string? GetFirstNameFromToken(string token)
        {
            try
            {
                var jwtToken = _tokenHandler.ReadJwtToken(token);
                return jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.GivenName || x.Type == JwtRegisteredClaimNames.GivenName)?.Value;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, StringMessages.FailedToGetFirstNameFromToken);
                return null;
            }
        }

        /// <summary>
        /// Gets last name from JWT token
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>Last name or null if invalid</returns>
        public string? GetLastNameFromToken(string token)
        {
            try
            {
                var jwtToken = _tokenHandler.ReadJwtToken(token);
                return jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Surname || x.Type == JwtRegisteredClaimNames.FamilyName)?.Value;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, StringMessages.FailedToGetLastNameFromToken);
                return null;
            }
        }

        /// <summary>
        /// Gets full name from JWT token
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>Full name or null if invalid</returns>
        public string? GetFullNameFromToken(string token)
        {
            try
            {
                var firstName = GetFirstNameFromToken(token);
                var lastName = GetLastNameFromToken(token);
                
                if (string.IsNullOrEmpty(firstName) && string.IsNullOrEmpty(lastName))
                    return null;
                
                return $"{firstName} {lastName}".Trim();
            }
            catch (Exception ex)
            {
                Log.Warning(ex, StringMessages.FailedToGetFullNameFromToken);
                return null;
            }
        }

        /// <summary>
        /// Sets JWT tokens as HTTP-only cookies
        /// </summary>
        /// <param name="response">HTTP response</param>
        /// <param name="accessToken">Access token</param>
        /// <param name="refreshToken">Refresh token</param>
        public void SetTokenCookies(HttpResponse response, string accessToken, string refreshToken)
        {
            if (!_jwtOptions.UseCookies) return;

            // Set access token cookie
            var accessTokenOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = _jwtOptions.SecureCookies,
                SameSite = Enum.Parse<Microsoft.AspNetCore.Http.SameSiteMode>(_jwtOptions.SameSiteMode),
                Path = _jwtOptions.CookiePath,
                Expires = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes)
            };

            if (!string.IsNullOrEmpty(_jwtOptions.CookieDomain))
            {
                accessTokenOptions.Domain = _jwtOptions.CookieDomain;
            }

            response.Cookies.Append(_jwtOptions.AccessTokenCookieName, accessToken, accessTokenOptions);

            // Set refresh token cookie
            var refreshTokenOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = _jwtOptions.SecureCookies,
                SameSite = Enum.Parse<Microsoft.AspNetCore.Http.SameSiteMode>(_jwtOptions.SameSiteMode),
                Path = _jwtOptions.CookiePath,
                Expires = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays)
            };

            if (!string.IsNullOrEmpty(_jwtOptions.CookieDomain))
            {
                refreshTokenOptions.Domain = _jwtOptions.CookieDomain;
            }

            response.Cookies.Append(_jwtOptions.RefreshTokenCookieName, refreshToken, refreshTokenOptions);

            Log.Information(StringMessages.JwtTokensSetAsHttpOnlyCookies);
        }

        /// <summary>
        /// Gets access token from cookies
        /// </summary>
        /// <param name="request">HTTP request</param>
        /// <returns>Access token or null if not found</returns>
        public string? GetAccessTokenFromCookies(HttpRequest request)
        {
            if (!_jwtOptions.UseCookies) return null;

            return request.Cookies[_jwtOptions.AccessTokenCookieName];
        }

        /// <summary>
        /// Gets refresh token from cookies
        /// </summary>
        /// <param name="request">HTTP request</param>
        /// <returns>Refresh token or null if not found</returns>
        public string? GetRefreshTokenFromCookies(HttpRequest request)
        {
            if (!_jwtOptions.UseCookies) return null;

            return request.Cookies[_jwtOptions.RefreshTokenCookieName];
        }

        /// <summary>
        /// Clears JWT token cookies
        /// </summary>
        /// <param name="response">HTTP response</param>
        public void ClearTokenCookies(HttpResponse response)
        {
            if (!_jwtOptions.UseCookies) return;

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = _jwtOptions.SecureCookies,
                SameSite = Enum.Parse<Microsoft.AspNetCore.Http.SameSiteMode>(_jwtOptions.SameSiteMode),
                Path = _jwtOptions.CookiePath,
                Expires = DateTime.UtcNow.AddDays(-1) // Expire in the past
            };

            if (!string.IsNullOrEmpty(_jwtOptions.CookieDomain))
            {
                cookieOptions.Domain = _jwtOptions.CookieDomain;
            }

            response.Cookies.Append(_jwtOptions.AccessTokenCookieName, string.Empty, cookieOptions);
            response.Cookies.Append(_jwtOptions.RefreshTokenCookieName, string.Empty, cookieOptions);

            Log.Information("JWT token cookies cleared");
        }

        /// <summary>
        /// Gets token from either Authorization header or cookies
        /// </summary>
        /// <param name="request">HTTP request</param>
        /// <returns>Token or null if not found</returns>
        public string? GetTokenFromRequest(HttpRequest request)
        {
            // First try to get from Authorization header
            var authHeader = request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                return authHeader.Substring("Bearer ".Length).Trim();
            }

            // If not found in header and cookies are enabled, try cookies
            if (_jwtOptions.UseCookies)
            {
                return GetAccessTokenFromCookies(request);
            }

            return null;
        }
    }
}