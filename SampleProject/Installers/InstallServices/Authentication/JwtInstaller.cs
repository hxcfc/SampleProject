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
            var jwtOptions = configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
            
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
