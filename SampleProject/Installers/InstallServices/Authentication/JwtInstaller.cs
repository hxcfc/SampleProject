using Common.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace SampleProject.Installers.InstallServices.Authentication
{
    /// <summary>
    /// Installer for JWT authentication services
    /// </summary>
    public class JwtInstaller : IInstaller
    {
        /// <summary>
        /// Gets the installation order priority
        /// </summary>
        public int Order => 8;
        /// <summary>
        /// Installs JWT authentication services
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Configuration</param>
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            // Get JWT options - combine environment variables (sensitive data) with appsettings (technical config)
            var jwtOptions = new JwtOptions
            {
                // Sensitive data from environment variables
                SecretKey = configuration["JWT_SECRET_KEY"] ?? throw new InvalidOperationException("JWT_SECRET_KEY environment variable is required"),
                Issuer = configuration["JWT_ISSUER"] ?? throw new InvalidOperationException("JWT_ISSUER environment variable is required"),
                Audience = configuration["JWT_AUDIENCE"] ?? throw new InvalidOperationException("JWT_AUDIENCE environment variable is required"),
                
                // Technical configuration from appsettings.json
                ExpirationMinutes = configuration.GetValue<int>("Jwt:ExpirationMinutes", 60),
                RefreshTokenExpirationDays = configuration.GetValue<int>("Jwt:RefreshTokenExpirationDays", 7),
                UseCookies = configuration.GetValue<bool>("Jwt:UseCookies", true),
                AccessTokenCookieName = configuration.GetValue<string>("Jwt:AccessTokenCookieName", "auth_session"),
                RefreshTokenCookieName = configuration.GetValue<string>("Jwt:RefreshTokenCookieName", "auth_refresh"),
                CookieDomain = configuration.GetValue<string>("Jwt:CookieDomain"),
                CookiePath = configuration.GetValue<string>("Jwt:CookiePath", "/"),
                SecureCookies = configuration.GetValue<bool>("Jwt:SecureCookies", true),
                SameSiteMode = configuration.GetValue<string>("Jwt:SameSiteMode", "Strict")
            };
            
            if (string.IsNullOrEmpty(jwtOptions.SecretKey))
            {
                throw new InvalidOperationException(StringMessages.JWTSecretKeyNotConfigured);
            }

            if (jwtOptions.SecretKey.Length < 32)
            {
                throw new InvalidOperationException(StringMessages.JWTSecretKeyTooShort);
            }

            if (string.IsNullOrEmpty(jwtOptions.Issuer))
            {
                throw new InvalidOperationException(StringMessages.JWTIssuerNotConfigured);
            }

            if (string.IsNullOrEmpty(jwtOptions.Audience))
            {
                throw new InvalidOperationException(StringMessages.JWTAudienceNotConfigured);
            }

            // Configure JWT authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
                    ClockSkew = TimeSpan.Zero
                };

                // Configure token extraction from cookies if enabled
                if (jwtOptions.UseCookies)
                {
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            // Try to get token from cookies first
                            var token = context.Request.Cookies[jwtOptions.AccessTokenCookieName];
                            if (!string.IsNullOrEmpty(token))
                            {
                                context.Token = token;
                                return Task.CompletedTask;
                            }

                            // Fallback to Authorization header
                            var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
                            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith(StringMessages.JWTBearerPrefix))
                            {
                                context.Token = authHeader.Substring(StringMessages.JWTBearerPrefix.Length).Trim();
                            }

                            return Task.CompletedTask;
                        },
                        OnAuthenticationFailed = context =>
                        {
                            Log.Warning(StringMessages.JWTAuthenticationFailed, context.Exception.Message);
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = context =>
                        {
                            Log.Debug(StringMessages.JWTTokenValidatedSuccessfully, 
                                context.Principal?.Identity?.Name ?? StringMessages.UnknownUser);
                            return Task.CompletedTask;
                        },
                        OnChallenge = context =>
                        {
                            Log.Warning(StringMessages.JWTAuthenticationChallenge, context.ErrorDescription);
                            return Task.CompletedTask;
                        }
                    };
                }
                else
                {
                    // Standard header-based authentication
                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            Log.Warning(StringMessages.JWTAuthenticationFailed, context.Exception.Message);
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = context =>
                        {
                            Log.Debug(StringMessages.JWTTokenValidatedSuccessfully, 
                                context.Principal?.Identity?.Name ?? StringMessages.UnknownUser);
                            return Task.CompletedTask;
                        },
                        OnChallenge = context =>
                        {
                            Log.Warning(StringMessages.JWTAuthenticationChallenge, context.ErrorDescription);
                            return Task.CompletedTask;
                        }
                    };
                }
            });

            // Configure authorization
            services.AddAuthorization(options =>
            {
                options.AddPolicy(StringMessages.RequireAuthenticatedUserPolicy, policy =>
                {
                    policy.RequireAuthenticatedUser();
                });

                options.AddPolicy(StringMessages.AdminOnlyPolicy, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole(StringMessages.AdminRole);
                });

                options.AddPolicy(StringMessages.UserOrAdminPolicy, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole(StringMessages.UserRole, StringMessages.AdminRole);
                });
            });

            // Note: IJwtService is now registered in InfrastructureInstaller

            Log.Information(StringMessages.JWTAuthenticationServicesConfiguredSuccessfully);
        }

        /// <summary>
        /// Configures JWT services in the web application
        /// </summary>
        /// <param name="app">Web application</param>
        public void ConfigureServices(WebApplication app)
        {
            // Use authentication and authorization
            app.UseAuthentication();
            app.UseAuthorization();

            Log.Information(StringMessages.JWTAuthenticationMiddlewareConfigured);
        }
    }
}
