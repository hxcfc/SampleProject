# Audit Log and Token History - SampleProject

## 📋 Table of Contents
- [Overview](#overview)
- [Audit Log System](#audit-log-system)
- [Database Schema](#database-schema)
- [Automatic Triggers](#automatic-triggers)
- [Token History Tracking](#token-history-tracking)
- [Query Functions](#query-functions)
- [Audit Data Structure](#audit-data-structure)
- [Security Considerations](#security-considerations)
- [Performance Optimization](#performance-optimization)
- [Monitoring & Alerts](#monitoring--alerts)
- [Data Retention](#data-retention)
- [Compliance Features](#compliance-features)
- [API Integration](#api-integration)
- [Troubleshooting](#troubleshooting)
- [Best Practices](#best-practices)

---

## 🎯 Overview

**SampleProject** implements comprehensive audit logging and token history tracking to provide complete visibility into user activities, system changes, and security events. The system automatically tracks all user modifications, authentication events, and token usage patterns.

### Key Features
- ✅ **Automatic Change Tracking**: Database triggers for all user modifications
- ✅ **Comprehensive Field Monitoring**: All user properties tracked
- ✅ **Token Usage History**: Refresh token usage patterns and statistics
- ✅ **JSON-based Storage**: Structured old/new value storage
- ✅ **Performance Optimized**: Indexed for fast queries
- ✅ **Helper Functions**: Built-in query utilities
- ✅ **Security Auditing**: Complete audit trail for compliance
- ✅ **Real-time Monitoring**: Live audit event tracking

---

## 📊 Audit Log System

### Audit Log Entity

```csharp
/// <summary>
/// Audit log entity for tracking user changes history
/// </summary>
[Table("UserAuditLogs")]
public class UserAuditLog
{
    /// <summary>
    /// Unique identifier for the audit log entry
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key to the user being audited
    /// </summary>
    [Required]
    public Guid UserId { get; set; }

    /// <summary>
    /// Type of change (Created, Updated, Deleted, etc.)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Field that was changed (if applicable)
    /// </summary>
    [MaxLength(100)]
    public string? FieldName { get; set; }

    /// <summary>
    /// Old value before change
    /// </summary>
    public string? OldValue { get; set; }

    /// <summary>
    /// New value after change
    /// </summary>
    public string? NewValue { get; set; }

    /// <summary>
    /// User who made the change (can be the same user or admin)
    /// </summary>
    public Guid? ChangedByUserId { get; set; }

    /// <summary>
    /// IP address where the change was made from
    /// </summary>
    [MaxLength(45)]
    public string? IpAddress { get; set; }

    /// <summary>
    /// User agent (browser/client) where the change was made from
    /// </summary>
    [MaxLength(500)]
    public string? UserAgent { get; set; }

    /// <summary>
    /// Timestamp when the change occurred
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Additional notes or description
    /// </summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }
}
```

### Audit Actions

```csharp
public static class AuditActions
{
    public const string Created = "Created";
    public const string Updated = "Updated";
    public const string Deleted = "Deleted";
    public const string Activated = "Activated";
    public const string Deactivated = "Deactivated";
    public const string EmailVerified = "EmailVerified";
    public const string PasswordChanged = "PasswordChanged";
    public const string RoleChanged = "RoleChanged";
    public const string Login = "Login";
    public const string Logout = "Logout";
    public const string TokenRefreshed = "TokenRefreshed";
    public const string TokenRevoked = "TokenRevoked";
}
```

---

## 🗄️ Database Schema

### UserAuditLogs Table

```sql
CREATE TABLE "UserAuditLogs" (
    "Id" UUID PRIMARY KEY NOT NULL,
    "UserId" UUID NOT NULL,
    "Action" VARCHAR(50) NOT NULL,
    "FieldName" VARCHAR(100),
    "OldValue" TEXT,
    "NewValue" TEXT,
    "ChangedByUserId" UUID,
    "IpAddress" VARCHAR(45),
    "UserAgent" VARCHAR(500),
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "Notes" VARCHAR(1000)
);
```

### Indexes for Performance

```sql
-- Primary indexes for fast queries
CREATE INDEX idx_user_audit_logs_user_id ON "UserAuditLogs"("UserId");
CREATE INDEX idx_user_audit_logs_created_at ON "UserAuditLogs"("CreatedAt");
CREATE INDEX idx_user_audit_logs_changed_by_user_id ON "UserAuditLogs"("ChangedByUserId");
CREATE INDEX idx_user_audit_logs_action ON "UserAuditLogs"("Action");

-- Composite indexes for common queries
CREATE INDEX idx_user_audit_logs_user_action ON "UserAuditLogs"("UserId", "Action");
CREATE INDEX idx_user_audit_logs_user_created ON "UserAuditLogs"("UserId", "CreatedAt");
CREATE INDEX idx_user_audit_logs_action_created ON "UserAuditLogs"("Action", "CreatedAt");
```

### Enhanced User Table for Token Tracking

```sql
-- Additional columns for token history tracking
ALTER TABLE "Users" ADD COLUMN "RefreshTokenUseCount" INTEGER NOT NULL DEFAULT 0;
ALTER TABLE "Users" ADD COLUMN "RefreshTokenLastUsedAt" TIMESTAMP;
```

---

## ⚡ Automatic Triggers

### Enhanced Audit Trigger Function

```sql
CREATE OR REPLACE FUNCTION log_user_changes_enhanced()
RETURNS TRIGGER AS $$
DECLARE
    changed_fields TEXT[] := '{}';
    field_name TEXT;
    old_val TEXT;
    new_val TEXT;
    audit_data JSONB;
BEGIN
    IF TG_OP = 'INSERT' THEN
        -- Log user creation with all relevant fields
        audit_data := jsonb_build_object(
            'Email', NEW."Email",
            'FirstName', NEW."FirstName",
            'LastName', NEW."LastName",
            'Role', NEW."Role"::text,
            'IsActive', NEW."IsActive",
            'IsEmailVerified', NEW."IsEmailVerified",
            'CreatedAt', NEW."CreatedAt",
            'UpdatedAt', NEW."UpdatedAt"
        );
        
        INSERT INTO "UserAuditLogs" (
            "Id", "UserId", "Action", "NewValue", "CreatedAt", "Notes"
        ) VALUES (
            gen_random_uuid(), 
            NEW."Id", 
            'Created', 
            audit_data::text,
            NOW(),
            'User account created'
        );
        RETURN NEW;

    ELSIF TG_OP = 'UPDATE' THEN
        -- Check each field for changes and build change list
        IF OLD."Email" IS DISTINCT FROM NEW."Email" THEN
            changed_fields := array_append(changed_fields, 'Email');
        END IF;
        
        IF OLD."FirstName" IS DISTINCT FROM NEW."FirstName" THEN
            changed_fields := array_append(changed_fields, 'FirstName');
        END IF;
        
        IF OLD."LastName" IS DISTINCT FROM NEW."LastName" THEN
            changed_fields := array_append(changed_fields, 'LastName');
        END IF;
        
        IF OLD."Role" IS DISTINCT FROM NEW."Role" THEN
            changed_fields := array_append(changed_fields, 'Role');
        END IF;
        
        IF OLD."IsActive" IS DISTINCT FROM NEW."IsActive" THEN
            changed_fields := array_append(changed_fields, 'IsActive');
        END IF;
        
        IF OLD."IsEmailVerified" IS DISTINCT FROM NEW."IsEmailVerified" THEN
            changed_fields := array_append(changed_fields, 'IsEmailVerified');
        END IF;
        
        IF OLD."RefreshToken" IS DISTINCT FROM NEW."RefreshToken" THEN
            changed_fields := array_append(changed_fields, 'RefreshToken');
        END IF;
        
        IF OLD."RefreshTokenUseCount" IS DISTINCT FROM NEW."RefreshTokenUseCount" THEN
            changed_fields := array_append(changed_fields, 'RefreshTokenUseCount');
        END IF;
        
        IF OLD."RefreshTokenLastUsedAt" IS DISTINCT FROM NEW."RefreshTokenLastUsedAt" THEN
            changed_fields := array_append(changed_fields, 'RefreshTokenLastUsedAt');
        END IF;
        
        -- Only log if there were actual changes
        IF array_length(changed_fields, 1) > 0 THEN
            audit_data := jsonb_build_object(
                'ChangedFields', changed_fields,
                'OldValues', jsonb_build_object(
                    'Email', OLD."Email",
                    'FirstName', OLD."FirstName",
                    'LastName', OLD."LastName",
                    'Role', OLD."Role"::text,
                    'IsActive', OLD."IsActive",
                    'IsEmailVerified', OLD."IsEmailVerified",
                    'RefreshTokenUseCount', OLD."RefreshTokenUseCount",
                    'RefreshTokenLastUsedAt', OLD."RefreshTokenLastUsedAt"
                ),
                'NewValues', jsonb_build_object(
                    'Email', NEW."Email",
                    'FirstName', NEW."FirstName",
                    'LastName', NEW."LastName",
                    'Role', NEW."Role"::text,
                    'IsActive', NEW."IsActive",
                    'IsEmailVerified', NEW."IsEmailVerified",
                    'RefreshTokenUseCount', NEW."RefreshTokenUseCount",
                    'RefreshTokenLastUsedAt', NEW."RefreshTokenLastUsedAt"
                )
            );
            
            INSERT INTO "UserAuditLogs" (
                "Id", "UserId", "Action", "NewValue", "CreatedAt", "Notes"
            ) VALUES (
                gen_random_uuid(), 
                NEW."Id", 
                'Updated', 
                audit_data::text,
                NOW(),
                'User account updated - Fields changed: ' || array_to_string(changed_fields, ', ')
            );
        END IF;
        RETURN NEW;

    ELSIF TG_OP = 'DELETE' THEN
        -- Log user deletion with key information
        audit_data := jsonb_build_object(
            'Email', OLD."Email",
            'FirstName', OLD."FirstName",
            'LastName', OLD."LastName",
            'Role', OLD."Role"::text,
            'IsActive', OLD."IsActive",
            'CreatedAt', OLD."CreatedAt",
            'LastLoginAt', OLD."LastLoginAt"
        );
        
        INSERT INTO "UserAuditLogs" (
            "Id", "UserId", "Action", "OldValue", "CreatedAt", "Notes"
        ) VALUES (
            gen_random_uuid(), 
            OLD."Id", 
            'Deleted', 
            audit_data::text,
            NOW(),
            'User account deleted'
        );
        RETURN OLD;
    END IF;
    
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;
```

### Trigger Creation

```sql
-- Drop existing trigger if it exists
DROP TRIGGER IF EXISTS user_audit_trigger ON "Users";

-- Create the enhanced trigger
CREATE TRIGGER user_audit_trigger_enhanced
AFTER INSERT OR UPDATE OR DELETE ON "Users"
FOR EACH ROW EXECUTE FUNCTION log_user_changes_enhanced();
```

---

## 🔑 Token History Tracking

### Refresh Token Usage Tracking

```csharp
/// <summary>
/// Updates refresh token usage statistics
/// </summary>
/// <param name="userId">User ID</param>
/// <param name="refreshToken">Refresh token</param>
/// <returns>True if successful</returns>
public async Task<bool> UpdateRefreshTokenUsageAsync(Guid userId, string refreshToken)
{
    var user = await _userRepository.GetByIdAsync(userId);
    if (user == null || user.RefreshToken != refreshToken)
        return false;

    // Update usage statistics
    user.RefreshTokenUseCount++;
    user.RefreshTokenLastUsedAt = DateTime.UtcNow;
    
    // The database trigger will automatically log this change
    await _userRepository.UpdateAsync(user);
    
    return true;
}
```

### Token Generation Logging

```csharp
/// <summary>
/// Logs token generation event
/// </summary>
/// <param name="userId">User ID</param>
/// <param name="tokenType">Type of token (Access/Refresh)</param>
/// <param name="ipAddress">Client IP address</param>
/// <param name="userAgent">Client user agent</param>
public async Task LogTokenGenerationAsync(Guid userId, string tokenType, string? ipAddress, string? userAgent)
{
    var auditLog = new UserAuditLog
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        Action = tokenType == "Access" ? "TokenGenerated" : "RefreshTokenGenerated",
        CreatedAt = DateTime.UtcNow,
        IpAddress = ipAddress,
        UserAgent = userAgent,
        Notes = $"{tokenType} token generated"
    };

    await _auditLogRepository.CreateAsync(auditLog);
}
```

### Token Revocation Logging

```csharp
/// <summary>
/// Logs token revocation event
/// </summary>
/// <param name="userId">User ID</param>
/// <param name="reason">Revocation reason</param>
/// <param name="ipAddress">Client IP address</param>
public async Task LogTokenRevocationAsync(Guid userId, string reason, string? ipAddress)
{
    var auditLog = new UserAuditLog
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        Action = "TokenRevoked",
        CreatedAt = DateTime.UtcNow,
        IpAddress = ipAddress,
        Notes = $"Token revoked - Reason: {reason}"
    };

    await _auditLogRepository.CreateAsync(auditLog);
}
```

---

## 🔍 Query Functions

### User Audit History Function

```sql
-- Function to get user audit history
CREATE OR REPLACE FUNCTION get_user_audit_history(user_uuid UUID, limit_count INTEGER DEFAULT 50)
RETURNS TABLE (
    action TEXT,
    changed_fields TEXT,
    old_values JSONB,
    new_values JSONB,
    created_at TIMESTAMP,
    notes TEXT
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        ual."Action"::TEXT,
        COALESCE(
            (ual."NewValue"::JSONB->>'ChangedFields')::TEXT,
            ''
        ) as changed_fields,
        COALESCE(
            (ual."OldValue"::JSONB),
            '{}'::JSONB
        ) as old_values,
        COALESCE(
            (ual."NewValue"::JSONB),
            '{}'::JSONB
        ) as new_values,
        ual."CreatedAt",
        COALESCE(ual."Notes", '')::TEXT
    FROM "UserAuditLogs" ual
    WHERE ual."UserId" = user_uuid
    ORDER BY ual."CreatedAt" DESC
    LIMIT limit_count;
END;
$$ LANGUAGE plpgsql;
```

### Audit Statistics Function

```sql
-- Function to get audit statistics
CREATE OR REPLACE FUNCTION get_audit_statistics()
RETURNS TABLE (
    total_audit_logs BIGINT,
    users_with_changes BIGINT,
    most_common_action TEXT,
    recent_activity_count BIGINT,
    last_activity TIMESTAMP
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        COUNT(*) as total_audit_logs,
        COUNT(DISTINCT "UserId") as users_with_changes,
        (SELECT "Action" 
         FROM "UserAuditLogs" 
         GROUP BY "Action" 
         ORDER BY COUNT(*) DESC 
         LIMIT 1) as most_common_action,
        COUNT(CASE WHEN "CreatedAt" > NOW() - INTERVAL '24 hours' THEN 1 END) as recent_activity_count,
        MAX("CreatedAt") as last_activity
    FROM "UserAuditLogs";
END;
$$ LANGUAGE plpgsql;
```

### Token Usage Statistics Function

```sql
-- Function to get token usage statistics
CREATE OR REPLACE FUNCTION get_token_usage_statistics()
RETURNS TABLE (
    user_id UUID,
    email TEXT,
    total_refresh_count BIGINT,
    last_token_use TIMESTAMP,
    days_since_last_use BIGINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        u."Id" as user_id,
        u."Email" as email,
        u."RefreshTokenUseCount" as total_refresh_count,
        u."RefreshTokenLastUsedAt" as last_token_use,
        EXTRACT(DAYS FROM NOW() - u."RefreshTokenLastUsedAt") as days_since_last_use
    FROM "Users" u
    WHERE u."RefreshToken" IS NOT NULL
    ORDER BY u."RefreshTokenUseCount" DESC;
END;
$$ LANGUAGE plpgsql;
```

---

## 📋 Audit Data Structure

### JSON Audit Data Format

```json
{
  "ChangedFields": ["FirstName", "IsActive", "RefreshTokenUseCount"],
  "OldValues": {
    "Email": "user@example.com",
    "FirstName": "John",
    "LastName": "Doe",
    "Role": "User",
    "IsActive": true,
    "IsEmailVerified": false,
    "RefreshTokenUseCount": 5,
    "RefreshTokenLastUsedAt": "2025-01-26T10:30:00Z"
  },
  "NewValues": {
    "Email": "user@example.com",
    "FirstName": "Jane",
    "LastName": "Doe",
    "Role": "User",
    "IsActive": false,
    "IsEmailVerified": false,
    "RefreshTokenUseCount": 6,
    "RefreshTokenLastUsedAt": "2025-01-27T15:30:00Z"
  }
}
```

### Audit Log Entry Examples

#### **User Creation**
```json
{
  "Id": "550e8400-e29b-41d4-a716-446655440000",
  "UserId": "550e8400-e29b-41d4-a716-446655440001",
  "Action": "Created",
  "NewValue": "{\"Email\":\"newuser@example.com\",\"FirstName\":\"John\",\"LastName\":\"Doe\",\"Role\":\"User\",\"IsActive\":true,\"IsEmailVerified\":false,\"CreatedAt\":\"2025-01-27T14:30:00Z\"}",
  "CreatedAt": "2025-01-27T14:30:00Z",
  "Notes": "User account created"
}
```

#### **User Update**
```json
{
  "Id": "550e8400-e29b-41d4-a716-446655440002",
  "UserId": "550e8400-e29b-41d4-a716-446655440001",
  "Action": "Updated",
  "NewValue": "{\"ChangedFields\":[\"FirstName\",\"IsActive\"],\"OldValues\":{\"FirstName\":\"John\",\"IsActive\":true},\"NewValues\":{\"FirstName\":\"Jane\",\"IsActive\":false}}",
  "CreatedAt": "2025-01-27T15:30:00Z",
  "Notes": "User account updated - Fields changed: FirstName, IsActive"
}
```

#### **Token Usage**
```json
{
  "Id": "550e8400-e29b-41d4-a716-446655440003",
  "UserId": "550e8400-e29b-41d4-a716-446655440001",
  "Action": "Updated",
  "NewValue": "{\"ChangedFields\":[\"RefreshTokenUseCount\",\"RefreshTokenLastUsedAt\"],\"OldValues\":{\"RefreshTokenUseCount\":5,\"RefreshTokenLastUsedAt\":\"2025-01-26T10:30:00Z\"},\"NewValues\":{\"RefreshTokenUseCount\":6,\"RefreshTokenLastUsedAt\":\"2025-01-27T15:30:00Z\"}}",
  "CreatedAt": "2025-01-27T15:30:00Z",
  "Notes": "User account updated - Fields changed: RefreshTokenUseCount, RefreshTokenLastUsedAt"
}
```

---

## 🔒 Security Considerations

### Sensitive Data Protection

```csharp
/// <summary>
/// Sanitizes sensitive data before logging
/// </summary>
/// <param name="value">Value to sanitize</param>
/// <param name="fieldName">Field name</param>
/// <returns>Sanitized value</returns>
private string SanitizeSensitiveData(string value, string fieldName)
{
    var sensitiveFields = new[] { "Password", "PasswordHash", "PasswordSalt", "RefreshToken" };
    
    if (sensitiveFields.Contains(fieldName))
    {
        return "[REDACTED]";
    }
    
    return value;
}
```

### Access Control

```csharp
/// <summary>
/// Checks if user can access audit logs
/// </summary>
/// <param name="userId">User requesting access</param>
/// <param name="targetUserId">User whose logs are being accessed</param>
/// <returns>True if access allowed</returns>
public async Task<bool> CanAccessAuditLogsAsync(Guid userId, Guid targetUserId)
{
    var currentUser = await _userRepository.GetByIdAsync(userId);
    if (currentUser == null) return false;
    
    // Users can only access their own audit logs
    // Admins can access any audit logs
    return userId == targetUserId || currentUser.Role == UserRole.Admin;
}
```

### Audit Log Encryption

```csharp
/// <summary>
/// Encrypts sensitive audit log data
/// </summary>
/// <param name="data">Data to encrypt</param>
/// <returns>Encrypted data</returns>
private string EncryptAuditData(string data)
{
    // Implementation would use AES encryption
    // This is a placeholder for the actual encryption logic
    return Convert.ToBase64String(Encoding.UTF8.GetBytes(data));
}
```

---

## ⚡ Performance Optimization

### Database Indexing Strategy

```sql
-- Primary indexes for fast lookups
CREATE INDEX CONCURRENTLY idx_user_audit_logs_user_id_created 
ON "UserAuditLogs"("UserId", "CreatedAt" DESC);

-- Partial indexes for common queries
CREATE INDEX CONCURRENTLY idx_user_audit_logs_recent_updates 
ON "UserAuditLogs"("UserId", "CreatedAt" DESC) 
WHERE "Action" = 'Updated';

CREATE INDEX CONCURRENTLY idx_user_audit_logs_token_events 
ON "UserAuditLogs"("UserId", "CreatedAt" DESC) 
WHERE "Action" IN ('TokenGenerated', 'TokenRefreshed', 'TokenRevoked');
```

### Query Optimization

```sql
-- Optimized query for recent user activity
EXPLAIN (ANALYZE, BUFFERS) 
SELECT "Action", "CreatedAt", "Notes"
FROM "UserAuditLogs"
WHERE "UserId" = $1
  AND "CreatedAt" > NOW() - INTERVAL '30 days'
ORDER BY "CreatedAt" DESC
LIMIT 100;
```

### Data Archiving

```sql
-- Archive old audit logs (older than 1 year)
CREATE OR REPLACE FUNCTION archive_old_audit_logs()
RETURNS INTEGER AS $$
DECLARE
    archived_count INTEGER;
BEGIN
    -- Move old logs to archive table
    INSERT INTO "UserAuditLogsArchive"
    SELECT * FROM "UserAuditLogs"
    WHERE "CreatedAt" < NOW() - INTERVAL '1 year';
    
    GET DIAGNOSTICS archived_count = ROW_COUNT;
    
    -- Delete archived logs from main table
    DELETE FROM "UserAuditLogs"
    WHERE "CreatedAt" < NOW() - INTERVAL '1 year';
    
    RETURN archived_count;
END;
$$ LANGUAGE plpgsql;
```

---

## 📊 Monitoring & Alerts

### Audit Log Monitoring

```csharp
/// <summary>
/// Monitors audit log for suspicious activity
/// </summary>
public class AuditLogMonitor
{
    public async Task CheckSuspiciousActivityAsync()
    {
        // Check for multiple failed login attempts
        var failedLogins = await _auditLogRepository.GetRecentFailedLoginsAsync(TimeSpan.FromHours(1));
        if (failedLogins.Count > 10)
        {
            await _alertService.SendAlertAsync("Multiple failed login attempts detected");
        }
        
        // Check for unusual token usage patterns
        var tokenUsage = await _auditLogRepository.GetUnusualTokenUsageAsync();
        if (tokenUsage.Any())
        {
            await _alertService.SendAlertAsync("Unusual token usage patterns detected");
        }
    }
}
```

### Real-time Audit Events

```csharp
/// <summary>
/// Publishes audit events for real-time monitoring
/// </summary>
public class AuditEventPublisher
{
    public async Task PublishAuditEventAsync(UserAuditLog auditLog)
    {
        var auditEvent = new AuditEvent
        {
            UserId = auditLog.UserId,
            Action = auditLog.Action,
            Timestamp = auditLog.CreatedAt,
            IpAddress = auditLog.IpAddress,
            UserAgent = auditLog.UserAgent
        };
        
        await _messagePublisher.PublishAsync("audit.events", auditEvent);
    }
}
```

---

## 📅 Data Retention

### Retention Policy Configuration

```csharp
public class AuditLogRetentionPolicy
{
    public TimeSpan StandardRetention { get; set; } = TimeSpan.FromDays(365);
    public TimeSpan SensitiveDataRetention { get; set; } = TimeSpan.FromDays(2555); // 7 years
    public TimeSpan TokenHistoryRetention { get; set; } = TimeSpan.FromDays(90);
}
```

### Automated Cleanup

```sql
-- Cleanup function for expired audit logs
CREATE OR REPLACE FUNCTION cleanup_expired_audit_logs()
RETURNS INTEGER AS $$
DECLARE
    deleted_count INTEGER;
BEGIN
    -- Delete standard audit logs older than retention period
    DELETE FROM "UserAuditLogs"
    WHERE "CreatedAt" < NOW() - INTERVAL '365 days'
      AND "Action" NOT IN ('PasswordChanged', 'RoleChanged', 'TokenRevoked');
    
    GET DIAGNOSTICS deleted_count = ROW_COUNT;
    
    RETURN deleted_count;
END;
$$ LANGUAGE plpgsql;

-- Schedule cleanup job (run daily)
SELECT cron.schedule('audit-cleanup', '0 2 * * *', 'SELECT cleanup_expired_audit_logs();');
```

---

## 📋 Compliance Features

### GDPR Compliance

```csharp
/// <summary>
/// Exports user audit data for GDPR compliance
/// </summary>
/// <param name="userId">User ID</param>
/// <returns>Audit data export</returns>
public async Task<AuditDataExport> ExportUserAuditDataAsync(Guid userId)
{
    var auditLogs = await _auditLogRepository.GetUserAuditHistoryAsync(userId);
    
    return new AuditDataExport
    {
        UserId = userId,
        ExportDate = DateTime.UtcNow,
        AuditLogs = auditLogs.Select(log => new AuditLogExport
        {
            Action = log.Action,
            Timestamp = log.CreatedAt,
            IpAddress = log.IpAddress,
            UserAgent = log.UserAgent,
            Notes = log.Notes
        }).ToList()
    };
}
```

### SOX Compliance

```csharp
/// <summary>
/// Generates SOX compliance report
/// </summary>
/// <param name="startDate">Report start date</param>
/// <param name="endDate">Report end date</param>
/// <returns>SOX compliance report</returns>
public async Task<SoxComplianceReport> GenerateSoxReportAsync(DateTime startDate, DateTime endDate)
{
    var userChanges = await _auditLogRepository.GetUserChangesAsync(startDate, endDate);
    var adminActions = await _auditLogRepository.GetAdminActionsAsync(startDate, endDate);
    var tokenEvents = await _auditLogRepository.GetTokenEventsAsync(startDate, endDate);
    
    return new SoxComplianceReport
    {
        Period = new DateRange(startDate, endDate),
        UserChanges = userChanges.Count,
        AdminActions = adminActions.Count,
        TokenEvents = tokenEvents.Count,
        ComplianceScore = CalculateComplianceScore(userChanges, adminActions, tokenEvents)
    };
}
```

---

## 🔌 API Integration

### Audit Log API Endpoints

```csharp
[ApiController]
[Route("api/v1/audit")]
[Authorize(Roles = "Admin")]
public class AuditLogController : BaseController
{
    /// <summary>
    /// Gets user audit history
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="limit">Number of records to return</param>
    /// <returns>User audit history</returns>
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserAuditHistory(Guid userId, int limit = 50)
    {
        var history = await _auditLogService.GetUserAuditHistoryAsync(userId, limit);
        return Ok(history);
    }
    
    /// <summary>
    /// Gets audit statistics
    /// </summary>
    /// <returns>Audit statistics</returns>
    [HttpGet("statistics")]
    public async Task<IActionResult> GetAuditStatistics()
    {
        var statistics = await _auditLogService.GetAuditStatisticsAsync();
        return Ok(statistics);
    }
    
    /// <summary>
    /// Gets token usage statistics
    /// </summary>
    /// <returns>Token usage statistics</returns>
    [HttpGet("token-usage")]
    public async Task<IActionResult> GetTokenUsageStatistics()
    {
        var statistics = await _auditLogService.GetTokenUsageStatisticsAsync();
        return Ok(statistics);
    }
}
```

### Real-time Audit Events

```csharp
/// <summary>
/// WebSocket endpoint for real-time audit events
/// </summary>
[Authorize]
public class AuditEventHub : Hub
{
    public async Task JoinUserGroup(string userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
    }
    
    public async Task LeaveUserGroup(string userId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
    }
}
```

---

## 🔍 Troubleshooting

### Common Issues

#### **Audit Logs Not Being Created**
- **Cause**: Database trigger not installed
- **Solution**: Run the audit trigger installation script
```sql
\i SampleProject.Persistence/Migrations/audit_log_trigger_enhanced.sql
```

#### **Performance Issues with Audit Queries**
- **Cause**: Missing indexes
- **Solution**: Create appropriate indexes
```sql
CREATE INDEX CONCURRENTLY idx_user_audit_logs_user_created 
ON "UserAuditLogs"("UserId", "CreatedAt" DESC);
```

#### **Large Audit Log Table**
- **Cause**: No data retention policy
- **Solution**: Implement data archiving
```sql
SELECT cleanup_expired_audit_logs();
```

### Debugging Queries

```sql
-- Check if trigger is active
SELECT tgname FROM pg_trigger WHERE tgname = 'user_audit_trigger_enhanced';

-- Check recent audit activity
SELECT "Action", COUNT(*) as count, MAX("CreatedAt") as last_occurrence
FROM "UserAuditLogs"
WHERE "CreatedAt" > NOW() - INTERVAL '24 hours'
GROUP BY "Action"
ORDER BY count DESC;

-- Check token usage patterns
SELECT 
    u."Email",
    u."RefreshTokenUseCount",
    u."RefreshTokenLastUsedAt"
FROM "Users" u
WHERE u."RefreshToken" IS NOT NULL
ORDER BY u."RefreshTokenUseCount" DESC
LIMIT 10;
```

---

## 📚 Best Practices

### Audit Log Design

1. **Comprehensive Tracking**: Log all significant user actions
2. **Structured Data**: Use JSON for old/new values
3. **Performance**: Index frequently queried fields
4. **Retention**: Implement data retention policies
5. **Security**: Protect sensitive audit data

### Token History Management

1. **Usage Tracking**: Monitor refresh token usage patterns
2. **Anomaly Detection**: Alert on unusual token activity
3. **Cleanup**: Regularly clean up expired token data
4. **Privacy**: Respect user privacy in token tracking

### Compliance

1. **GDPR**: Provide data export capabilities
2. **SOX**: Maintain audit trails for financial data
3. **HIPAA**: Protect health-related audit data
4. **PCI DSS**: Secure payment-related audit logs

### Performance

1. **Indexing**: Create appropriate database indexes
2. **Archiving**: Archive old audit data
3. **Partitioning**: Consider table partitioning for large datasets
4. **Monitoring**: Monitor audit log performance

---

**Audit Log and Token History** - Comprehensive tracking and monitoring for security, compliance, and operational insights
