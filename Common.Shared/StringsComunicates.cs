namespace Common.Shared
{
    /// <summary>
    /// Common string constants and messages used throughout the application
    /// </summary>
    public static class StringMessages
    {
        public static string AdminOnlyPolicy = "AdminOnly";

        public static string AdminRole = "Admin";

        public static string AdvancedMetricsServiceDisposed = "Advanced metrics service disposed";
        public static string AdvancedMetricsServiceInitialized = "Advanced metrics service initialized";

        // Advanced Metrics Service messages
        public static string AdvancedMetricsServiceMeterName = "SampleProject.AdvancedMetrics";

        public static string AdvancedMetricsServiceVersion = "1.0.0";
        public static string AdvancedMonitoringMiddlewareConfigured = "Advanced monitoring middleware configured";

        // Advanced Monitoring messages
        public static string AdvancedMonitoringServicesConfigured = "Advanced monitoring services configured successfully";

        public static string AdvancedMonitoringServicesReady = "Advanced monitoring services ready (middleware configured in Program.cs)";
        public static string AllServicesConfiguredSuccessfully = "All services configured successfully";

        public static string AllServicesInstalledSuccessfully = "All services installed successfully";

        // Success messages
        public static string ApiSuccessMessage = "Operation completed successfully";

        // API versioning configuration
        public static string ApiVersioningDefaultVersion = "1.0";

        public static string ApiVersioningGroupNameFormat = "'v'VVV";

        public static string ApiVersioningHeaderName = "X-Version";

        public static string ApiVersioningMediaTypeParameter = "ver";

        public static string ApiVersioningMiddlewareConfigured = "API versioning middleware configured";

        public static string ApiVersioningQueryParameter = "version";

        // API Versioning messages
        public static string ApiVersioningServicesConfiguredSuccessfully = "API versioning services configured successfully";

        // Application messages
        public static string ApplicationIsListeningOn = "Application is listening on: {Urls}";

        public static string ApplicationJsonContentType = "application/json";

        // LoggingInstaller messages
        public static string ApplicationRunningInEnvironment = "Application is running in {Environment} environment";

        public static string ApplicationShutdown = $"{ApplicationInfo.Name} is shutting down gracefully";
        public static string ApplicationStarted = $"{ApplicationInfo.Name} v{ApplicationInfo.Version} by {ApplicationInfo.Owner} started successfully";

        // Auth Command Handler messages
        public static string AttemptingLoginForEmail = "Attempting login for email: {Email}";

        public static string AttemptingToGetUserByEmail = "Attempting to get user by email: {Email}";
        public static string AttemptingToGetUserById = "Attempting to get user by ID: {UserId}";

        // User Service messages
        public static string AttemptingUserRegistration = "Attempting user registration for email: {Email}";

        public static string AuthenticationEventsTotalDescription = "Total number of authentication events";

        public static string AuthenticationEventsTotalMetric = "authentication_events_total";

        public static string AuthLoginEndpoint = "auth/login";

        // Error messages
        public static string AuthorizationFailed = "Authorization failed";

        public static string AuthRefreshEndpoint = "auth/refresh";
        public static string AutoMapperServicesConfigured = "AutoMapper services configured";

        // AutoMapper messages
        public static string AutoMapperServicesRegisteredSuccessfully = "AutoMapper services registered successfully";

        public static string BadToken = "Invalid access token or refresh token";

        public static string BadUserOrPassword = "Invalid username or password";

        public static string BasicHealthCheckDescription = "Application is running";

        public static string BasicHealthCheckName = "basic";

        public static string BasicHealthCheckTags = "basic,ready";

        public static string BearerTokenType = "Bearer";
        public static string BusinessEventsTotalDescription = "Total number of business events";
        public static string BusinessEventsTotalMetric = "business_events_total";

        // Password Change messages
        public static string ChangingPasswordForUser = "Changing password for user with ID: {UserId}";

        // User Role Change messages
        public static string ChangingUserRole = "Changing user role for user with ID: {UserId} to role: {NewRole}";

        // Email Availability Check messages
        public static string CheckingEmailAvailability = "Checking email availability for: {Email}";

        public static string ConfigurationOptionsConfigured = "Configuration options configured";

        // Configuration options messages
        public static string ConfigurationOptionsRegisteredSuccessfully = "Configuration options registered successfully";

        public static string ConfiguringServicesFrom = "Configuring services from {InstallerName} (Order: {Order})";

        public static string ContactDeveloper = "Please contact the application developer";

        public static string CorrelationIdTag = "correlation_id";
        public static string CpuMetricsCollected = "CPU metrics collected - Process CPU: {ProcessCpu}ms, GC Collections: {GcCollections}";

        // User Repository messages
        public static string CreatingUserInDatabase = "Creating user in database with email: {Email}";

        public static string CredentialsValidatedSuccessfullyForEmail = "Credentials validated successfully for email: {Email}";

        // File system messages
        public static string CurrentFolder = "Current folder";

        public static string CustomMetricDescription = "Custom metric";

        public static string DatabaseConfigurationNotFound = "Database configuration not found";

        // Database related messages
        public static string DatabaseConnectionInfo = "Database connection information";

        public static string DatabaseContextConfiguredSuccessfully = "Database context configured successfully";
        public static string DatabaseEnsuredCreatedSuccessfully = "Database ensured created successfully";
        public static string DatabaseHealthCheckDescription = "Database connectivity check";
        public static string DatabaseHealthCheckName = "database";
        public static string DatabaseHealthCheckTags = "database,ready,live";
        public static string DatabaseOperationDelete = "DELETE";
        public static string DatabaseOperationInsert = "INSERT";

        // Database operations for metrics
        public static string DatabaseOperationSelect = "SELECT";

        public static string DatabaseOperationUpdate = "UPDATE";
        public static string DatabaseQuery = "Database query prepared";
        public static string DatabaseQueryDurationDescription = "Database query duration in seconds";
        public static string DatabaseQueryDurationMetric = "database_query_duration_seconds";
        public static string DatabaseServicesConfiguredSuccessfully = "Database services configured successfully";
        public static string DataIncomplete = "Incomplete data provided";
        public static string DataReadSuccessfully = "Data read successfully";
        public static string DecryptionError = "Decryption error occurred";

        // Database fallback values
        public static string DefaultDatabaseName = "SampleProjectDb";

        public static string DeletingUserFromDatabase = "Deleting user from database with ID: {UserId}";
        public static string DesiredFolderPath = "Desired folder path selected";
        public static string DevelopmentEnvironment = "Development";

        // Docker configuration
        public static string DockerApiContainerName = "sampleproject-api";

        public static string DockerGrafanaContainerName = "sampleproject-grafana";

        public static string DockerPostgresContainerName = "sampleproject-postgres";

        public static string DockerPrometheusContainerName = "sampleproject-prometheus";

        public static string DockerRedisContainerName = "sampleproject-redis";

        public static string EmailAvailabilityChecked = "Email availability checked for {Email}: {IsAvailable}";
        public static string EncryptionError = "Encryption error occurred";

        public static string EndpointRateLimitExceeded = "Endpoint rate limit exceeded for IP: {ClientIp}, Endpoint: {Endpoint}";
        public static string EndRequest = "========== END REQUEST ==========";

        public static string EndResponse = "========== END RESPONSE ==========";

        public static string EntityIdTag = "entity_id";
        public static string EntityTypeTag = "entity_type";
        public static string EnvAspNetCoreEnvironment = "ASPNETCORE_ENVIRONMENT";

        public static string EnvCorsAllowedOrigins = "CORS_ALLOWED_ORIGINS";

        // Environment variables
        public static string EnvDatabaseConnectionString = "DATABASE_CONNECTION_STRING";

        public static string EnvJwtAudience = "JWT_AUDIENCE";

        public static string EnvJwtIssuer = "JWT_ISSUER";

        public static string EnvJwtSecretKey = "JWT_SECRET_KEY";

        public static string EnvPostgresDb = "POSTGRES_DB";

        public static string EnvPostgresPassword = "POSTGRES_PASSWORD";

        public static string EnvPostgresUser = "POSTGRES_USER";

        // SecurityHeadersMiddleware messages
        public static string ErrorApplyingSecurityHeaders = "Error applying security headers";

        public static string ErrorCollectingSystemMetrics = "Error collecting system metrics";
        public static string ErrorDuringLogin = "An error occurred during login";

        public static string ErrorDuringLogout = "An error occurred during logout";

        public static string ErrorDuringTokenRefresh = "An error occurred during token refresh";

        public static string ErrorGettingUserInfo = "An error occurred while getting user information";

        public static string ErrorOccurredDuringLoginForEmail = "Error occurred during login for email: {Email}";
        public static string ErrorOccurredDuringLogout = "Error occurred during logout";
        public static string ErrorOccurredDuringTokenRefresh = "Error occurred during token refresh";
        public static string ErrorOccurredDuringUserRegistration = "Error occurred during user registration for email: {Email}";
        public static string ErrorOccurredWhileChangingPassword = "Error occurred while changing password for user with ID: {UserId}";
        public static string ErrorOccurredWhileChangingUserRole = "Error occurred while changing user role for user with ID: {UserId}";
        public static string ErrorOccurredWhileCheckingEmailAvailability = "Error occurred while checking email availability for: {Email}";
        public static string ErrorOccurredWhileCheckingUserExistence = "Error occurred while checking user existence for email: {Email}";
        public static string ErrorOccurredWhileCreatingUserInDatabase = "Error occurred while creating user in database with email: {Email}";
        public static string ErrorOccurredWhileDeletingUserFromDatabase = "Error occurred while deleting user from database with ID: {UserId}";
        public static string ErrorOccurredWhileGettingUserByEmail = "Error occurred while getting user by email: {Email}";
        public static string ErrorOccurredWhileGettingUserById = "Error occurred while getting user by ID: {UserId}";
        public static string ErrorOccurredWhileGettingUsersList = "Error occurred while getting users list";
        public static string ErrorOccurredWhileGettingUsersListFromDatabase = "Error occurred while getting users list from database";
        public static string ErrorOccurredWhileRefreshingToken = "Error occurred while refreshing token";

        public static string ErrorOccurredWhileRetrievingUserByEmailFromDatabase = "Error occurred while retrieving user by email from database: {Email}";
        public static string ErrorOccurredWhileRetrievingUserByIdFromDatabase = "Error occurred while retrieving user by ID from database: {UserId}";
        public static string ErrorOccurredWhileRevokingRefreshToken = "Error occurred while revoking refresh token for user: {UserId}";
        public static string ErrorOccurredWhileSavingRefreshToken = "Error occurred while saving refresh token for user: {UserId}";
        public static string ErrorOccurredWhileUpdatingPassword = "Error occurred while updating password for user with ID: {UserId}";
        public static string ErrorOccurredWhileUpdatingUser = "Error occurred while updating user with ID: {UserId}";
        public static string ErrorOccurredWhileUpdatingUserInDatabase = "Error occurred while updating user in database with ID: {UserId} and email: {Email}";
        public static string ErrorOccurredWhileValidatingCredentialsForEmail = "Error occurred while validating credentials for email: {Email}";

        public static string ErrorOccurredWhileValidatingRefreshToken = "Error occurred while validating refresh token";
        public static string ErrorValidatingToken = "An error occurred while validating token";

        // Advanced Metrics Service tag names
        public static string EventTypeTag = "event_type";

        public static string FailedToChangePassword = "Failed to change password for user with ID: {UserId}";
        public static string FailedToChangeUserRole = "Failed to change user role for user with ID: {UserId}";
        public static string FailedToCollectCpuMetrics = "Failed to collect CPU metrics";
        public static string FailedToCollectMemoryMetrics = "Failed to collect memory metrics";
        public static string FailedToConfigureServicesFromAssembly = "Failed to configure services from assembly";

        public static string FailedToCreateUser = "Failed to create user with email: {Email}";
        public static string FailedToEnsureDatabaseCreated = "Failed to ensure database is created";

        public static string FailedToGetEmailFromToken = "Failed to get email from token";
        public static string FailedToGetFirstNameFromToken = "Failed to get first name from token";
        public static string FailedToGetFullNameFromToken = "Failed to get full name from token";
        public static string FailedToGetLastNameFromToken = "Failed to get last name from token";
        public static string FailedToGetRoleFromToken = "Failed to get Role from token";
        public static string FailedToGetUserIdFromToken = "Failed to get user ID from token";
        public static string FailedToGetUserInfo = "Failed to get user information";

        public static string FailedToGetUsernameFromToken = "Failed to get username from token";
        public static string FailedToGetUsersList = "Failed to get users list";
        public static string FailedToInstallServicesFromAssembly = "Failed to install services from assembly";

        public static string FailedToRevokeRefreshToken = "Failed to revoke refresh token for user: {UserId}";
        public static string FailedToSaveNewRefreshToken = "Failed to save new refresh token for user: {UserId}";
        public static string FailedToSaveRefreshTokenForUser = "Failed to save refresh token for user: {UserId}";
        public static string FailedToUpdateUser = "Failed to update user with ID: {UserId}";
        public static string FileDataRead = "Reading from file";

        public static string FileNotFound = "File not found";

        public static string FindFileInFolder = "Searching for file in folder";

        public static string FluentValidationServicesConfigured = "FluentValidation services configured";

        // Validation messages
        public static string FluentValidationServicesRegisteredSuccessfully = "FluentValidation services registered successfully";

        public static string FoundInstallersToConfigure = "Found {Count} installers to configure";

        // Installer messages
        public static string FoundInstallersToRegister = "Found {Count} installers to register";

        public static string GcCollectionsMetric = "gc_collections_total";
        public static string GcTotalTypeTag = "gc_total";
        public static string GeneratedNewCorrelationId = "Generated new correlation ID: {CorrelationId}";

        // Users List messages
        public static string GettingUsersList = "Getting users list - Page: {Page}, PageSize: {PageSize}";

        public static string GettingUsersListFromDatabase = "Getting users list from database - Page: {Page}, PageSize: {PageSize}";
        public static string GlobalRateLimitExceeded = "Global rate limit exceeded for IP: {ClientIp}";
        public static string GlobalRateLimitKey = "global";
        public static string GrafanaDashboardRefresh = "5s";

        public static string GrafanaDashboardTags = "sampleproject,api";

        public static string GrafanaDashboardTitle = "SampleProject API Dashboard";

        public static string GrafanaDataSourceName = "Prometheus";

        public static string GrafanaDataSourceType = "prometheus";

        public static string GrafanaDataSourceUrl = "http://prometheus:9090";

        // Grafana configuration
        public static string GrafanaDefaultPassword = "admin123";

        public static string HealthCheckDurationFormat = @"hh\:mm\:ss\.fffffff";

        public static string HealthCheckEndpointsConfiguredAt = "Health check endpoints configured at /{HealthCheckEndpoint}";

        public static string HealthCheckInterval = "10s";

        // Health check configurations
        public static string HealthCheckPostgresCommand = "pg_isready -U {User} -d {Database}";

        public static string HealthCheckRedisCommand = "redis-cli ping";

        public static string HealthCheckResponseContentType = "application/json";

        public static string HealthCheckRetries = "5";

        public static string HealthChecksDisabledInConfiguration = "Health checks are disabled in configuration";

        public static string HealthCheckServicesConfiguredSuccessfully = "Health check services configured successfully";

        public static string HealthCheckTimeout = "5s";

        // Health Check messages
        public static string HealthCheckUIEnabledAt = "Health check UI enabled at /{HealthCheckUIEndpoint}";

        public static string HealthCheckUIEndpointName = "SampleProject API";

        public static string HealthCheckUIEndpointUrl = "http://localhost:15553/{HealthCheckEndpoint}";

        public static string HostMasked = "Host=***";

        public static string HttpErrorsTotalDescription = "Total number of HTTP errors";
        public static string HttpErrorsTotalMetric = "http_errors_total";
        public static string HttpRequestDurationDescription = "HTTP request duration in seconds";
        public static string HttpRequestDurationMetric = "http_request_duration_seconds";
        public static string HttpRequestsTotalDescription = "Total number of HTTP requests";
        public static string HttpRequestsTotalMetric = "http_requests_total";
        public static string InputTypeIncorrect = "Incorrect input type";

        public static string InstallingServicesFrom = "Installing services from {InstallerName} (Order: {Order})";

        public static string InsufficientPermissions = "Insufficient permissions to access this resource";
        public static string InvalidCredentialsProvidedForEmail = "Invalid credentials provided for email: {Email}";
        public static string InvalidCurrentPassword = "Invalid current password for user with ID: {UserId}";
        public static string InvalidEmailFormat = "Invalid email format: {Email}";
        public static string InvalidEmailOrPassword = "Invalid email or password";
        public static string AccountNotActive = "Account is not active";
        public static string InvalidUserId = "Invalid user ID provided";
        public static string InvalidUserIdProvided = "Invalid user ID provided: {UserId}";

        public static string InvalidOrExpiredRefreshToken = "Invalid or expired refresh token";
        public static string InvalidPassword = "Invalid password";

        public static string InvalidPasswordForEmail = "Invalid password for email: {Email}";

        public static string InvalidRefreshToken = "Invalid refresh token";

        public static string InvalidRefreshTokenProvided = "Invalid refresh token provided";

        public static string InvalidToken = "Invalid or missing authentication token";
        public static string JWTAudienceNotConfigured = "JWT Audience is not configured";

        public static string JWTAuthenticationChallenge = "JWT authentication challenge: {Error}";

        public static string JWTAuthenticationFailed = "JWT authentication failed: {Error}";

        public static string JWTAuthenticationMiddlewareConfigured = "JWT authentication middleware configured";

        public static string JWTAuthenticationServicesConfiguredSuccessfully = "JWT authentication services configured successfully";

        public static string JWTBearerPrefix = "Bearer ";

        public static string JWTIssuerNotConfigured = "JWT Issuer is not configured";

        // JWT Authentication messages
        public static string JWTSecretKeyNotConfigured = "JWT SecretKey is not configured";

        public static string JWTSecretKeyTooShort = "JWT SecretKey must be at least 32 characters long for security";

        public static string JwtTokenGeneratedForUser = "JWT token generated for user: {Username} with Role: {Role}";
        public static string JwtTokensSetAsHttpOnlyCookies = "JWT tokens set as HTTP-only cookies";
        public static string JWTTokenValidatedSuccessfully = "JWT token validated successfully for user: {User}";
        public static string JwtTokenValidationFailed = "JWT token validation failed";

        // Cache related messages
        public static string KeyNotFoundInCache = "Key not found in cache";

        public static string KeyValue = "Key-Value pair";
        public static string LoggingServicesConfigured = "Logging services configured";

        // Logging messages
        public static string LoggingServicesRegisteredSuccessfully = "Logging services registered successfully";

        public static string LoginFailed = "Login failed";

        // AuthController messages
        public static string LoginSuccessful = "Login successful";

        public static string LoginSuccessfulForUser = "Login successful for user: {UserId}";
        public static string LogoutFailed = "Logout failed";
        public static string LogoutSuccessful = "Logout successful";
        public static string LogsNotFound = "No logs found for the specified date";
        public static string MediatRServicesConfigured = "MediatR services configured";

        // MediatR messages
        public static string MediatRServicesRegisteredSuccessfully = "MediatR services registered successfully";

        public static string MemoryHealthCheckDescription = "Memory check passed";
        public static string MemoryHealthCheckName = "memory";
        public static string MemoryHealthCheckTags = "memory";
        public static string MemoryMetricsCollected = "Memory metrics collected - Process: {ProcessMemory} bytes, GC Total: {SystemMemory} bytes";

        // Method and path information
        public static string MethodPath = "Method and path information";

        public static string MethodTag = "method";
        public static string MetricsActiveUsers = "active_users";
        public static string MetricsActiveUsersDescription = "Number of active users";
        public static string MetricsDatabaseConnections = "database_connections";
        public static string MetricsDatabaseConnectionsDescription = "Number of active database connections";
        public static string MetricsDatabaseQueriesDescription = "Total number of database queries";
        public static string MetricsDatabaseQueriesTotal = "database_queries_total";
        public static string MetricsDatabaseQueryDuration = "database_query_duration_seconds";
        public static string MetricsDatabaseQueryDurationDescription = "Database query duration in seconds";

        // Metrics endpoint paths
        public static string MetricsEndpointPath = "/metrics";

        public static string MetricsHttpRequestDuration = "http_request_duration_seconds";
        public static string MetricsHttpRequestDurationDescription = "HTTP request duration in seconds";

        // Metrics descriptions
        public static string MetricsHttpRequestsDescription = "Total number of HTTP requests";

        // Metrics names
        public static string MetricsHttpRequestsTotal = "http_requests_total";

        public static string MetricsLabelEndpoint = "endpoint";

        // Metrics labels
        public static string MetricsLabelMethod = "method";

        public static string MetricsLabelOperation = "operation";
        public static string MetricsLabelResult = "result";
        public static string MetricsLabelStatusCode = "status_code";
        public static string MetricsLabelTable = "table";
        public static string MetricsLoginAttemptsDescription = "Total number of login attempts";
        public static string MetricsLoginAttemptsTotal = "login_attempts_total";
        public static string MetricsMiddlewareApiPathPrefix = "/api/";
        public static string MetricsMiddlewareNormalizedPath = "/{id}";

        // Metrics middleware
        public static string MetricsMiddlewareRecorded = "Recorded metrics for {0} {1} - {2} in {3}ms";

        public static string MetricsPrometheusServerPath = "/metrics";
        public static string MetricsResultFailure = "failure";

        // Metrics result values
        public static string MetricsResultSuccess = "success";

        public static string MetricsTokenRefreshesDescription = "Total number of token refreshes";
        public static string MetricsTokenRefreshesTotal = "token_refreshes_total";
        public static string MetricsUsersCreatedDescription = "Total number of users created";
        public static string MetricsUsersCreatedTotal = "users_created_total";
        public static string NetworkDriver = "bridge";

        // Network configurations
        public static string NetworkName = "sampleproject-network";

        public static string NoCurrentUserFoundDuringLogout = "No current user found during logout";

        // UsersController messages
        public static string NotFound = "Not Found";

        // ExperimentalController messages
        public static string NothingSpecial = "Nothing special";

        public static string NoTokenProvided = "No token provided";
        public static string NoValidTokenFound = "No valid token found";

        // BaseController messages
        public static string OperationFailed = "Operation Failed";

        public static string PasswordChangedSuccessfully = "Password changed successfully for user with ID: {UserId}";
        public static string PasswordUpdatedForUser = "Password updated for user with ID: {UserId}";
        public static string PathTag = "path";
        public static string PerIpRateLimitExceeded = "Per-IP rate limit exceeded for IP: {ClientIp}";

        // Port configurations
        public static string PortApi = "15553";

        public static string PortGrafana = "3000";
        public static string PortPostgres = "5432";
        public static string PortPrometheus = "9090";
        public static string PortRedis = "6379";
        public static string ProcessCompleted = "Process completed successfully";
        public static string ProcessCpuTimeMetric = "process_cpu_time_ms";

        // Correlation ID messages
        public static string ProcessingRequestWithCorrelationId = "Processing request with correlation ID: {CorrelationId}";

        public static string ProcessingTokenRefreshRequest = "Processing token refresh request";
        public static string ProcessingUserLogout = "Processing user logout";
        public static string ProcessTypeTag = "process";
        public static string PrometheusApiScrapeInterval = "5s";
        public static string PrometheusApiScrapeTimeout = "5s";
        public static string PrometheusDatabaseScrapeInterval = "30s";
        public static string PrometheusEvaluationInterval = "15s";
        public static string PrometheusMetricsMiddlewareConfigured = "Prometheus metrics middleware configured";

        // Prometheus Metrics messages
        public static string PrometheusMetricsServicesConfiguredSuccessfully = "Prometheus metrics services configured successfully";

        public static string PrometheusRedisScrapeInterval = "30s";

        // Prometheus configuration
        public static string PrometheusScrapeInterval = "15s";

        public static string QueryString = "Query string parameters";
        public static string QueryTypeTag = "query_type";

        // Rate Limiting messages
        public static string RateLimitExceededMessage = "Rate limit exceeded. Please try again later.";

        public static string RateLimitRetryAfterSeconds = "60";
        public static string RecordedAuthenticationEvent = "Recorded authentication event: {EventType} {UserId} {Success}";
        public static string RecordedBusinessEvent = "Recorded business event: {EventType} {EntityType} {EntityId}";
        public static string RecordedCpuUsage = "Recorded CPU usage: {CpuPercentage}%";
        public static string RecordedCustomMetric = "Recorded custom metric: {MetricName} {Value}";
        public static string RecordedDatabaseQuery = "Recorded database query: {QueryType} {Duration}s {Success}";
        public static string RecordedMemoryUsage = "Recorded memory usage: {BytesUsed} bytes";
        public static string RecordedRequestMetrics = "Recorded request metrics: {Method} {Path} {StatusCode} {Duration}s";
        public static string RecordNotFound = "Record not found";
        public static string RecordNotFoundInTable = "Record not found in table";

        // Authorization messages
        public static string RefreshingToken = "Refreshing token";

        public static string RefreshTokenRevokedSuccessfully = "Refresh token revoked successfully for user: {UserId}";
        public static string RefreshTokenRevokedSuccessfullyForUser = "Refresh token revoked successfully for user: {UserId}";
        public static string RefreshTokenSavedSuccessfully = "Refresh token saved successfully for user: {UserId}";
        public static string RefreshTokenValidatedSuccessfully = "Refresh token validated successfully for user: {UserId}";

        // HTTP related messages
        public static string RequestBody = "========== REQUEST BODY ==========";

        public static string RequestBodyReadFail = "Failed to read request body content";
        public static string RequestHeaders = "========== REQUEST HEADERS ==========";

        // JWT Policy names
        public static string RequireAuthenticatedUserPolicy = "RequireAuthenticatedUser";

        public static string ResourceLimitMemory128M = "128M";
        public static string ResourceLimitMemory1G = "1G";
        public static string ResourceLimitMemory256M = "256M";

        // Resource limits
        public static string ResourceLimitMemory512M = "512M";

        public static string ResponseBody = "========== RESPONSE BODY ==========";
        public static string RestartPolicyAlways = "always";

        // Restart policies
        public static string RestartPolicyUnlessStopped = "unless-stopped";

        public static string RetrievingUserByEmailFromDatabase = "Retrieving user by email from database: {Email}";
        public static string RetrievingUserByIdFromDatabase = "Retrieving user by ID from database: {UserId}";
        public static string RevokingRefreshTokenForUser = "Revoking refresh token for user: {UserId}";
        public static string SavingRefreshTokenForUser = "Saving refresh token for user: {UserId}";
        public static string SensitiveDataLoggingEnabled = "Sensitive data logging is enabled - this should only be used in development";
        public static string StatusCodeTag = "status_code";
        public static string SuccessTag = "success";
        public static string SwaggerAuthorizationHeader = "Authorization";
        public static string SwaggerBearerDescription = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"";
        public static string SwaggerBearerScheme = "Bearer";

        // SwaggerInstaller messages
        public static string SwaggerDisabledInConfiguration = "Swagger is disabled in configuration";

        public static string SwaggerServicesConfiguredSuccessfully = "Swagger services configured successfully";

        // Swagger messagesabled in configuration";
        public static string SwaggerUIEnabledAt = "Swagger UI enabled at /swagger";

        public static string SwaggerVersioningDescriptionFormat = "{0} {1}";
        public static string SwaggerVersioningEndpointFormat = "/swagger/{0}/swagger.json";

        // Swagger versioning
        public static string SwaggerVersioningTitleFormat = "{0} {1}";

        public static string SystemCpuUsageDescription = "System CPU usage percentage";
        public static string SystemCpuUsageMetric = "system_cpu_usage_percent";
        public static string SystemMemoryTotalMetric = "system_memory_total_bytes";
        public static string SystemMemoryUsageDescription = "System memory usage in bytes";
        public static string SystemMemoryUsageMetric = "system_memory_usage_bytes";
        public static string SystemMonitoringServiceDisposed = "System monitoring service disposed";

        // System Monitoring messages
        public static string SystemMonitoringServiceInitialized = "System monitoring service initialized with {Interval}s collection interval";

        public static string SystemMonitoringServiceStarted = "System monitoring service started";
        public static string SystemMonitoringServiceStopped = "System monitoring service stopped";
        public static string TableIsEmpty = "Table is empty";
        public static string TokenIsInvalidOrExpired = "Token is invalid or expired";
        public static string TokenIsValid = "Token is valid";
        public static string TokenRefreshedSuccessfully = "Token refreshed successfully";
        public static string TokenRefreshedSuccessfullyForUser = "Token refreshed successfully for user: {UserId}";
        public static string TokenRefreshFailed = "Token refresh failed";
        public static string TotalTypeTag = "total";
        public static string TypeNotImplemented = "Type not implemented";

        // Authorization messages
        public static string Unauthorized = "Unauthorized";

        public static string UnexpectedErrorOccurred = "An unexpected error occurred";

        // ExceptionHandlingMiddleware messages
        public static string UnhandledExceptionOccurred = "An unhandled exception occurred";

        public static string UnknownErrorOccurred = "Unknown error occurred";
        public static string UnknownIpAddress = "Unknown";
        public static string UnknownUser = "Unknown";
        public static string UnknownValue = "unknown";
        public static string UpdatingPasswordForUser = "Updating password for user with ID: {UserId}";

        // User Update messages
        public static string UpdatingUser = "Updating user with ID: {UserId}";

        public static string UpdatingUserInDatabase = "Updating user in database with ID: {UserId} and email: {Email}";
        public static string UserAlreadyExists = "User with email {Email} already exists";
        public static string UserCreatedInDatabase = "User created in database with ID: {UserId} and email: {Email}";
        public static string UserDeletedFromDatabase = "User deleted from database with ID: {UserId}";
        public static string UserIdTag = "user_id";
        public static string UserLogoutProcessedSuccessfully = "User logout processed successfully";
        public static string UserNotFound = "User not found";
        public static string UserNotFoundByEmailInDatabase = "User not found by email in database: {Email}";
        public static string UserNotFoundByIdInDatabase = "User not found by ID in database: {UserId}";
        public static string UserNotFoundForDeletion = "User not found for deletion with ID: {UserId}";
        public static string UserNotFoundForEmail = "User not found for email: {Email}";
        public static string UserNotFoundForId = "User not found for ID: {UserId}";
        public static string UserNotFoundForPasswordUpdate = "User not found for password update with ID: {UserId}";
        public static string UserOrAdminPolicy = "UserOrAdmin";
        public static string UserRegisteredSuccessfully = "User registered successfully with ID: {UserId} and email: {Email}";
        public static string UserRetrievedByEmailFromDatabase = "User retrieved by email from database: {Email}";
        public static string UserRetrievedByIdFromDatabase = "User retrieved by ID from database: {UserId}";
        public static string UserRetrievedSuccessfully = "User retrieved successfully with ID: {UserId}";
        public static string UserRetrievedSuccessfullyByEmail = "User retrieved successfully by email: {Email}";
        public static string UserRole = "User";

        public static string UserRoleChangedSuccessfully = "User role changed successfully for user with ID: {UserId} to role: {NewRole}";

        public static string UsersListRetrieved = "Users list retrieved successfully - Count: {Count}, Total: {TotalCount}";

        public static string UsersListRetrievedFromDatabase = "Users list retrieved from database - Count: {Count}, Total: {TotalCount}";

        public static string UserUpdatedInDatabase = "User updated in database with ID: {UserId} and email: {Email}";

        public static string UserUpdatedSuccessfully = "User updated successfully with ID: {UserId}";

        // Database messages
        public static string UsingInMemoryDatabase = "Using In-Memory database: {DatabaseName}";

        public static string UsingPostgreSQLDatabase = "Using PostgreSQL database: {ConnectionString}";

        // Credentials validation messages
        public static string ValidatingCredentialsForEmail = "Validating credentials for email: {Email}";

        public static string ValidatingRefreshToken = "Validating refresh token";
        public static string ValidationError = "Validation error occurred";
        public static string VolumeGrafanaData = "grafana_data";

        // Volume configurations
        public static string VolumePostgresData = "postgres_data";

        public static string VolumePrometheusData = "prometheus_data";
        public static string VolumeRedisData = "redis_data";
        public static string WelcomeMessage = $"Welcome to {ApplicationInfo.Name}\nApplication is starting up...";
        public static string XForwardedForHeader = "X-Forwarded-For";
        public static string XRateLimitLimitEndpointHeader = "X-RateLimit-Limit-Endpoint";
        public static string XRateLimitLimitGlobalHeader = "X-RateLimit-Limit-Global";
        public static string XRateLimitLimitIpHeader = "X-RateLimit-Limit-IP";
        public static string XRateLimitRemainingEndpointHeader = "X-RateLimit-Remaining-Endpoint";
        public static string XRateLimitRemainingGlobalHeader = "X-RateLimit-Remaining-Global";
        public static string XRateLimitRemainingIpHeader = "X-RateLimit-Remaining-IP";
        public static string XRateLimitResetHeader = "X-RateLimit-Reset";
        public static string XRealIpHeader = "X-Real-IP";

        // Configuration
        private static string configFileName = ApplicationInfo.Name;

        /// <summary>
        /// Gets the configuration folder name
        /// </summary>
        public static string ConfigFolderName => configFileName;

        /// <summary>
        /// Creates a message for application information
        /// </summary>
        /// <returns>Formatted application info message</returns>
        public static string ApplicationInfoMessage()
        {
            return $"{ApplicationInfo.Name} v{ApplicationInfo.Version} by {ApplicationInfo.Owner}\n" +
                   $"Release Date: {ApplicationInfo.ReleaseDate:yyyy-MM-dd}\n" +
                   $"Runtime: {ApplicationInfo.RuntimeVersion}\n" +
                   $"Framework: {ApplicationInfo.TargetFramework}";
        }

        /// <summary>
        /// Creates a message for application shutdown
        /// </summary>
        /// <returns>Formatted shutdown message</returns>
        public static string ApplicationShutdownMessage()
        {
            return $"{ApplicationInfo.Name} v{ApplicationInfo.Version} is shutting down gracefully...";
        }

        /// <summary>
        /// Creates a message for application startup
        /// </summary>
        /// <returns>Formatted startup message</returns>
        public static string ApplicationStartupMessage()
        {
            return $"{ApplicationInfo.Name} v{ApplicationInfo.Version} by {ApplicationInfo.Owner} is starting up...";
        }

        /// <summary>
        /// Creates a message for database connection
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        /// <returns>Formatted message</returns>
        public static string DatabaseConnectionMessage(string connectionString)
        {
            return $"====== Database Connection =====\nConnecting to database:\n{connectionString}\n===== Database Connection =====";
        }

        /// <summary>
        /// Creates a message for database connection with application context
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        /// <returns>Formatted message with application context</returns>
        public static string DatabaseConnectionWithAppContextMessage(string connectionString)
        {
            return $"====== {ApplicationInfo.Name} Database Connection =====\n" +
                   $"Application: {ApplicationInfo.Name} v{ApplicationInfo.Version}\n" +
                   $"Connecting to database:\n{connectionString}\n" +
                   $"===== Database Connection =====";
        }

        /// <summary>
        /// Creates a message for database query
        /// </summary>
        /// <param name="query">Database query</param>
        /// <returns>Formatted message</returns>
        public static string DatabaseQueryMessage(string query)
        {
            return $"====== Database Query =====\nPrepared database query:\n{query}\n===== Database Query =====";
        }

        // Dynamic message methods
        /// <summary>
        /// Creates a message for incomplete data
        /// </summary>
        /// <param name="entityName">Name of the entity with incomplete data</param>
        /// <returns>Formatted message</returns>
        public static string DataIncompleteFor(string entityName)
        {
            return $"Incomplete data provided for {entityName}";
        }

        /// <summary>
        /// Creates a message for file not found in directory
        /// </summary>
        /// <param name="fileName">Name of the file not found</param>
        /// <param name="directoryName">Name of the directory</param>
        /// <returns>Formatted message</returns>
        public static string FileNotFoundInDirectoryMessage(string fileName, string directoryName)
        {
            return $"File '{fileName}' not found in directory '{directoryName}'";
        }

        /// <summary>
        /// Creates a message for file not found
        /// </summary>
        /// <param name="fileName">Name of the file not found</param>
        /// <returns>Formatted message</returns>
        public static string FileNotFoundMessage(string fileName)
        {
            return $"File '{fileName}' not found";
        }

        /// <summary>
        /// Creates a message for key-value pair
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>Formatted message</returns>
        public static string KeyValueMessage(string key, object value)
        {
            return $"Key: {key} | Value: {value}";
        }

        /// <summary>
        /// Creates a message for method and path
        /// </summary>
        /// <param name="method">HTTP method</param>
        /// <param name="path">Request path</param>
        /// <returns>Formatted message</returns>
        public static string MethodPathMessage(string method, string path)
        {
            return $"Method: {method} | Path: {path}";
        }

        /// <summary>
        /// Creates a message for process completion
        /// </summary>
        /// <param name="message">Completion message</param>
        /// <returns>Formatted message</returns>
        public static string ProcessCompletedMessage(string message)
        {
            return $"Process completed: {message}";
        }

        /// <summary>
        /// Creates a message for query string
        /// </summary>
        /// <param name="queryString">Query string</param>
        /// <returns>Formatted message</returns>
        public static string QueryStringMessage(object queryString)
        {
            return $"QueryString: {queryString}";
        }

        /// <summary>
        /// Creates a message for record not found in table
        /// </summary>
        /// <param name="tableName">Name of the table</param>
        /// <param name="recordKey">Key of the record not found</param>
        /// <returns>Formatted message</returns>
        public static string RecordNotFoundInTableMessage(string tableName, object recordKey)
        {
            return $"Record with key '{recordKey}' not found in table '{tableName}'";
        }

        /// <summary>
        /// Creates a message for incoming request
        /// </summary>
        /// <param name="date">Date and time of the request</param>
        /// <returns>Formatted message</returns>
        public static string RequestIncomingMessage(DateTime date)
        {
            return $"===== Request received at {date:yyyy-MM-dd HH:mm:ss} =====";
        }

        /// <summary>
        /// Sets the configuration folder name
        /// </summary>
        /// <param name="instanceName">Instance name to append to the configuration folder name</param>
        public static void SetConfigFolderName(string? instanceName)
        {
            configFileName = string.IsNullOrEmpty(instanceName)
                ? ApplicationInfo.Name
                : $"{ApplicationInfo.Name}{instanceName}";
        }

        /// <summary>
        /// Creates a message for user not found
        /// </summary>
        /// <param name="username">Username not found</param>
        /// <returns>Formatted message</returns>
        public static string UserNotFoundMessage(string username)
        {
            return $"User '{username}' not found";
        }
    }
}