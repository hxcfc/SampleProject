# Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .

# Build args for JWT configuration (non-sensitive only)
ARG JWT_ISSUER
ARG JWT_AUDIENCE
ARG JWT_EXPIRATION_MINUTES
ARG JWT_USE_COOKIES
ARG JWT_ACCESS_TOKEN_COOKIE_NAME
ARG JWT_REFRESH_TOKEN_COOKIE_NAME
ARG JWT_COOKIE_DOMAIN
ARG JWT_COOKIE_PATH
ARG JWT_SECURE_COOKIES
ARG JWT_SAME_SITE_MODE

# Set environment variables for build (non-sensitive only)
ENV JWT_ISSUER=$JWT_ISSUER
ENV JWT_AUDIENCE=$JWT_AUDIENCE
ENV JWT_EXPIRATION_MINUTES=$JWT_EXPIRATION_MINUTES
ENV JWT_USE_COOKIES=$JWT_USE_COOKIES
ENV JWT_ACCESS_TOKEN_COOKIE_NAME=$JWT_ACCESS_TOKEN_COOKIE_NAME
ENV JWT_REFRESH_TOKEN_COOKIE_NAME=$JWT_REFRESH_TOKEN_COOKIE_NAME
ENV JWT_COOKIE_DOMAIN=$JWT_COOKIE_DOMAIN
ENV JWT_COOKIE_PATH=$JWT_COOKIE_PATH
ENV JWT_SECURE_COOKIES=$JWT_SECURE_COOKIES
ENV JWT_SAME_SITE_MODE=$JWT_SAME_SITE_MODE

# Note: JWT_SECRET_KEY and JWT_REFRESH_TOKEN_EXPIRATION_DAYS should be set at runtime
# via docker run -e or docker-compose environment variables for security

RUN dotnet restore
RUN dotnet publish SampleProject/SampleProject.csproj -c Release -o /app/publish --no-restore

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

# Install curl for health check
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Create non-root user for security
RUN addgroup --system --gid 1001 app && \
    adduser --system --uid 1001 --gid 1001 app

WORKDIR /app
COPY --from=build /app/publish .

# Change ownership to app user
RUN chown -R app:app /app

# Switch to non-root user
USER app

EXPOSE 80

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost/health || exit 1

ENTRYPOINT ["dotnet", "SampleProject.dll"]
