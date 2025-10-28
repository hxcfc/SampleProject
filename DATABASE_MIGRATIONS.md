# Database Migrations - SampleProject

## 📋 Table of Contents
- [Overview](#overview)
- [Migration System](#migration-system)
- [Initial Schema](#initial-schema)
- [Audit Log Migration](#audit-log-migration)
- [Enhanced Audit Migration](#enhanced-audit-migration)
- [Migration Management](#migration-management)
- [Database Setup](#database-setup)
- [Verification Queries](#verification-queries)
- [Rollback Procedures](#rollback-procedures)
- [Performance Considerations](#performance-considerations)
- [Troubleshooting](#troubleshooting)
- [Best Practices](#best-practices)

---

## 🎯 Overview

**SampleProject** uses Entity Framework Core with PostgreSQL for database management. The migration system includes both EF Core migrations and custom SQL scripts for advanced features like audit logging and triggers.

### Migration Types
- ✅ **EF Core Migrations**: Standard schema changes
- ✅ **Custom SQL Scripts**: Advanced features and triggers
- ✅ **Audit Log System**: Comprehensive change tracking
- ✅ **Database Functions**: Helper functions for queries
- ✅ **Indexes**: Performance optimization
- ✅ **Triggers**: Automatic audit logging

---

## 🔄 Migration System

### Entity Framework Core Migrations

#### **Migration Commands**
```bash
# Create a new migration
dotnet ef migrations add MigrationName --project SampleProject.Persistence

# Update database with migrations
dotnet ef database update --project SampleProject.Persistence

# Remove last migration (if not applied)
dotnet ef migrations remove --project SampleProject.Persistence

# Generate SQL script
dotnet ef migrations script --project SampleProject.Persistence

# Check migration status
dotnet ef migrations list --project SampleProject.Persistence
```

#### **Migration Configuration**
```csharp
// In ApplicationDbContext
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // User entity configuration
    modelBuilder.Entity<UserEntity>(entity =>
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
        entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
        entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
        entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
        entity.Property(e => e.PasswordSalt).IsRequired().HasMaxLength(255);
        entity.Property(e => e.Role).IsRequired();
        entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
        entity.Property(e => e.IsEmailVerified).IsRequired().HasDefaultValue(false);
        entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("NOW()");
        entity.Property(e => e.RefreshToken).HasMaxLength(500);
        entity.Property(e => e.RefreshTokenUseCount).IsRequired().HasDefaultValue(0);
        
        entity.HasIndex(e => e.Email).IsUnique();
    });

    // UserAuditLog entity configuration
    modelBuilder.Entity<UserAuditLog>(entity =>
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.UserId).IsRequired();
        entity.Property(e => e.Action).IsRequired().HasMaxLength(50);
        entity.Property(e => e.FieldName).HasMaxLength(100);
        entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("NOW()");
        entity.Property(e => e.IpAddress).HasMaxLength(45);
        entity.Property(e => e.UserAgent).HasMaxLength(500);
        entity.Property(e => e.Notes).HasMaxLength(1000);
        
        entity.HasIndex(e => e.UserId);
        entity.HasIndex(e => e.CreatedAt);
        entity.HasIndex(e => e.Action);
    });
}
```

---

## 🏗️ Initial Schema

### Users Table

```sql
-- Initial Users table creation
CREATE TABLE "Users" (
    "Id" UUID PRIMARY KEY NOT NULL,
    "Email" VARCHAR(255) UNIQUE NOT NULL,
    "FirstName" VARCHAR(100) NOT NULL,
    "LastName" VARCHAR(100) NOT NULL,
    "PasswordHash" VARCHAR(255) NOT NULL,
    "PasswordSalt" VARCHAR(255) NOT NULL,
    "Role" INTEGER NOT NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT true,
    "IsEmailVerified" BOOLEAN NOT NULL DEFAULT false,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "LastLoginAt" TIMESTAMP,
    "RefreshToken" VARCHAR(500),
    "RefreshTokenExpiryTime" TIMESTAMP,
    "RefreshTokenUseCount" INTEGER NOT NULL DEFAULT 0,
    "RefreshTokenLastUsedAt" TIMESTAMP
);

-- Indexes for performance
CREATE INDEX idx_users_email ON "Users"("Email");
CREATE INDEX idx_users_refresh_token ON "Users"("RefreshToken") WHERE "RefreshToken" IS NOT NULL;
CREATE INDEX idx_users_active ON "Users"("IsActive");
CREATE INDEX idx_users_role ON "Users"("Role");
```

### UserAuditLogs Table

```sql
-- UserAuditLogs table for audit tracking
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

-- Performance indexes
CREATE INDEX idx_user_audit_logs_user_id ON "UserAuditLogs"("UserId");
CREATE INDEX idx_user_audit_logs_created_at ON "UserAuditLogs"("CreatedAt");
CREATE INDEX idx_user_audit_logs_changed_by_user_id ON "UserAuditLogs"("ChangedByUserId");
CREATE INDEX idx_user_audit_logs_action ON "UserAuditLogs"("Action");

-- Composite indexes for common queries
CREATE INDEX idx_user_audit_logs_user_action ON "UserAuditLogs"("UserId", "Action");
CREATE INDEX idx_user_audit_logs_user_created ON "UserAuditLogs"("UserId", "CreatedAt");
CREATE INDEX idx_user_audit_logs_action_created ON "UserAuditLogs"("Action", "CreatedAt");
```

---

## 📊 Audit Log Migration

### Basic Audit Log Migration Script

**File**: `SampleProject.Persistence/Migrations/add_user_audit_log.sql`

```sql
-- Migration: Add UserAuditLog table and trigger
-- Created: 2025-01-XX
-- Description: Adds audit logging functionality with automatic triggers for user changes

-- =====================================================
-- 1. Create UserAuditLogs table (if not exists)
-- =====================================================
CREATE TABLE IF NOT EXISTS "UserAuditLogs" (
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

-- =====================================================
-- 2. Create indexes for better performance
-- =====================================================
CREATE INDEX IF NOT EXISTS idx_user_audit_logs_user_id ON "UserAuditLogs"("UserId");
CREATE INDEX IF NOT EXISTS idx_user_audit_logs_created_at ON "UserAuditLogs"("CreatedAt");
CREATE INDEX IF NOT EXISTS idx_user_audit_logs_changed_by_user_id ON "UserAuditLogs"("ChangedByUserId");
CREATE INDEX IF NOT EXISTS idx_user_audit_logs_action ON "UserAuditLogs"("Action");

-- =====================================================
-- 3. Add new columns to Users table if they don't exist
-- =====================================================
DO $$ 
BEGIN
    -- Add RefreshTokenUseCount column if it doesn't exist
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'Users' AND column_name = 'RefreshTokenUseCount'
    ) THEN
        ALTER TABLE "Users" ADD COLUMN "RefreshTokenUseCount" INTEGER NOT NULL DEFAULT 0;
    END IF;

    -- Add RefreshTokenLastUsedAt column if it doesn't exist
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'Users' AND column_name = 'RefreshTokenLastUsedAt'
    ) THEN
        ALTER TABLE "Users" ADD COLUMN "RefreshTokenLastUsedAt" TIMESTAMP;
    END IF;
END $$;

-- =====================================================
-- 4. Create trigger function for automatic audit logging
-- =====================================================
CREATE OR REPLACE FUNCTION log_user_changes()
RETURNS TRIGGER AS $$
BEGIN
    IF TG_OP = 'INSERT' THEN
        -- Log user creation
        INSERT INTO "UserAuditLogs" (
            "Id", "UserId", "Action", "NewValue", "CreatedAt"
        ) VALUES (
            gen_random_uuid(), NEW."Id", 'Created', 
            jsonb_build_object(
                'Email', NEW."Email",
                'FirstName', NEW."FirstName",
                'LastName', NEW."LastName",
                'Role', NEW."Role",
                'IsActive', NEW."IsActive",
                'CreatedAt', NEW."CreatedAt"
            )::text, 
            NOW()
        );
        RETURN NEW;

    ELSIF TG_OP = 'UPDATE' THEN
        -- Log only if something actually changed
        IF (OLD."Email" IS DISTINCT FROM NEW."Email" OR
            OLD."FirstName" IS DISTINCT FROM NEW."FirstName" OR
            OLD."LastName" IS DISTINCT FROM NEW."LastName" OR
            OLD."Role" IS DISTINCT FROM NEW."Role" OR
            OLD."IsActive" IS DISTINCT FROM NEW."IsActive" OR
            OLD."IsEmailVerified" IS DISTINCT FROM NEW."IsEmailVerified" OR
            OLD."RefreshToken" IS DISTINCT FROM NEW."RefreshToken") THEN

            INSERT INTO "UserAuditLogs" (
                "Id", "UserId", "Action", "OldValue", "NewValue", "CreatedAt"
            ) VALUES (
                gen_random_uuid(), NEW."Id", 'Updated',
                jsonb_build_object(
                    'Email', OLD."Email",
                    'FirstName', OLD."FirstName",
                    'LastName', OLD."LastName",
                    'Role', OLD."Role",
                    'IsActive', OLD."IsActive",
                    'IsEmailVerified', OLD."IsEmailVerified"
                )::text,
                jsonb_build_object(
                    'Email', NEW."Email",
                    'FirstName', NEW."FirstName",
                    'LastName', NEW."LastName",
                    'Role', NEW."Role",
                    'IsActive', NEW."IsActive",
                    'IsEmailVerified', NEW."IsEmailVerified"
                )::text,
                NOW()
            );
        END IF;
        RETURN NEW;

    ELSIF TG_OP = 'DELETE' THEN
        -- Log user deletion
        INSERT INTO "UserAuditLogs" (
            "Id", "UserId", "Action", "OldValue", "CreatedAt"
        ) VALUES (
            gen_random_uuid(), OLD."Id", 'Deleted',
            jsonb_build_object(
                'Email', OLD."Email",
                'FirstName', OLD."FirstName",
                'LastName', OLD."LastName"
            )::text,
            NOW()
        );
        RETURN OLD;
    END IF;
END;
$$ LANGUAGE plpgsql;

-- =====================================================
-- 5. Create trigger for Users table
-- =====================================================
-- Drop trigger if it exists to avoid conflicts
DROP TRIGGER IF EXISTS user_audit_trigger ON "Users";

-- Create the trigger
CREATE TRIGGER user_audit_trigger
AFTER INSERT OR UPDATE OR DELETE ON "Users"
FOR EACH ROW EXECUTE FUNCTION log_user_changes();
```

---

## 🚀 Enhanced Audit Migration

### Enhanced Audit Log Migration Script

**File**: `SampleProject.Persistence/Migrations/audit_log_trigger_with_error_handling.sql`

```sql
-- Enhanced Audit Log Trigger for User Changes
-- Created: 2025-01-27
-- Description: Comprehensive audit logging with detailed field tracking for UserEntity changes

-- =====================================================
-- 1. Enhanced trigger function with comprehensive field tracking
-- =====================================================
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

-- =====================================================
-- 2. Create enhanced trigger (replace existing one)
-- =====================================================
-- Drop existing trigger if it exists
DROP TRIGGER IF EXISTS user_audit_trigger ON "Users";

-- Create the enhanced trigger
CREATE TRIGGER user_audit_trigger_enhanced
AFTER INSERT OR UPDATE OR DELETE ON "Users"
FOR EACH ROW EXECUTE FUNCTION log_user_changes_enhanced();

-- =====================================================
-- 3. Create helper functions for audit log queries
-- =====================================================

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

## 🛠️ Migration Management

### Migration Installation Guide

#### **Option 1: Using psql (PostgreSQL command line)**
```bash
# Connect to database
psql -U your_username -d your_database

# Run basic audit migration
\i SampleProject.Persistence/Migrations/add_user_audit_log.sql

# Run enhanced audit migration
\i SampleProject.Persistence/Migrations/audit_log_trigger_with_error_handling.sql
```

#### **Option 2: Using pgAdmin**
1. Open pgAdmin
2. Connect to your database
3. Right-click on database → Query Tool
4. Open and run the SQL files in order:
   - `add_user_audit_log.sql`
   - `audit_log_trigger_with_error_handling.sql`

#### **Option 3: Using Entity Framework (Recommended)**
```bash
# Generate migration
dotnet ef migrations add AddUserAuditLog --project SampleProject.Persistence

# Apply migration
dotnet ef database update --project SampleProject.Persistence

# Run custom SQL scripts
psql -U your_username -d your_database -f SampleProject.Persistence/Migrations/add_user_audit_log.sql
psql -U your_username -d your_database -f SampleProject.Persistence/Migrations/audit_log_trigger_with_error_handling.sql
```

### Migration Order
1. **EF Core Migrations**: Standard schema changes
2. **Basic Audit Migration**: `add_user_audit_log.sql`
3. **Enhanced Audit Migration**: `audit_log_trigger_with_error_handling.sql`

---

## 🗄️ Database Setup

### Connection String Configuration

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=SampleProject;Username=your_username;Password=your_password;Port=5432;"
  }
}
```

### Environment-Specific Configuration

#### **Development**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=SampleProject_Dev;Username=dev_user;Password=dev_password;Port=5432;"
  }
}
```

#### **Production**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=prod-db-host;Database=SampleProject_Prod;Username=prod_user;Password=${DB_PASSWORD};Port=5432;"
  }
}
```

### Database Initialization

```csharp
// In Program.cs
public static void Main(string[] args)
{
    var builder = WebApplication.CreateBuilder(args);
    
    // Add services
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
    
    var app = builder.Build();
    
    // Ensure database is created and migrated
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
    }
    
    app.Run();
}
```

---

## ✅ Verification Queries

### Basic Verification

```sql
-- Check if tables exist
SELECT table_name 
FROM information_schema.tables 
WHERE table_schema = 'public' 
AND table_name IN ('Users', 'UserAuditLogs');

-- Check if trigger exists
SELECT tgname 
FROM pg_trigger 
WHERE tgname = 'user_audit_trigger_with_error_handling';

-- Check new columns in Users table
SELECT column_name, data_type, is_nullable, column_default
FROM information_schema.columns 
WHERE table_name = 'Users' 
AND column_name IN ('RefreshTokenUseCount', 'RefreshTokenLastUsedAt');
```

### Audit System Verification

```sql
-- Test trigger by creating a test user
INSERT INTO "Users" ("Id", "Email", "FirstName", "LastName", "PasswordHash", "PasswordSalt", "Role")
VALUES (gen_random_uuid(), 'test@example.com', 'Test', 'User', 'hash', 'salt', 0);

-- Check if audit log was created
SELECT * FROM "UserAuditLogs" ORDER BY "CreatedAt" DESC LIMIT 5;

-- Test update trigger
UPDATE "Users" 
SET "FirstName" = 'Updated', "IsActive" = false 
WHERE "Email" = 'test@example.com';

-- Check if update was logged
SELECT "Action", "Notes", "NewValue"::JSONB 
FROM "UserAuditLogs" 
WHERE "UserId" = (SELECT "Id" FROM "Users" WHERE "Email" = 'test@example.com')
ORDER BY "CreatedAt" DESC;
```

### Function Verification

```sql
-- Test audit history function
SELECT * FROM get_user_audit_history(
    (SELECT "Id" FROM "Users" WHERE "Email" = 'test@example.com'), 
    10
);

-- Test audit statistics function
SELECT * FROM get_audit_statistics();

-- Test token usage statistics function
SELECT * FROM get_token_usage_statistics();
```

### Performance Verification

```sql
-- Check index usage
EXPLAIN (ANALYZE, BUFFERS) 
SELECT * FROM "UserAuditLogs" 
WHERE "UserId" = 'some-uuid' 
ORDER BY "CreatedAt" DESC 
LIMIT 50;

-- Check table sizes
SELECT 
    schemaname,
    tablename,
    pg_size_pretty(pg_total_relation_size(schemaname||'.'||tablename)) as size
FROM pg_tables 
WHERE schemaname = 'public' 
AND tablename IN ('Users', 'UserAuditLogs');
```

---

## 🔄 Rollback Procedures

### Rollback Enhanced Audit Migration

```sql
-- Rollback enhanced audit migration
DROP TRIGGER IF EXISTS user_audit_trigger_with_error_handling ON "Users";
DROP FUNCTION IF EXISTS log_user_changes_with_error_handling();
DROP FUNCTION IF EXISTS get_audit_log_errors(TIMESTAMP, TIMESTAMP);
DROP FUNCTION IF EXISTS cleanup_audit_log_errors(INTEGER);
DROP FUNCTION IF EXISTS get_audit_log_statistics();
DROP TABLE IF EXISTS "AuditLogErrors";
```

### Rollback Basic Audit Migration

```sql
-- Rollback basic audit migration
DROP TRIGGER IF EXISTS user_audit_trigger ON "Users";
DROP FUNCTION IF EXISTS log_user_changes();
DROP TABLE IF EXISTS "UserAuditLogs";

-- Remove added columns (be careful - you'll lose data!)
-- ALTER TABLE "Users" DROP COLUMN IF EXISTS "RefreshTokenUseCount";
-- ALTER TABLE "Users" DROP COLUMN IF EXISTS "RefreshTokenLastUsedAt";
```

### Rollback EF Core Migrations

```bash
# Rollback to specific migration
dotnet ef database update PreviousMigrationName --project SampleProject.Persistence

# Rollback all migrations
dotnet ef database update 0 --project SampleProject.Persistence
```

---

## ⚡ Performance Considerations

### Index Optimization

```sql
-- Create additional indexes for better performance
CREATE INDEX CONCURRENTLY idx_user_audit_logs_user_created 
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
-- Optimize common audit queries
EXPLAIN (ANALYZE, BUFFERS) 
SELECT "Action", COUNT(*) as count
FROM "UserAuditLogs"
WHERE "CreatedAt" > NOW() - INTERVAL '30 days'
GROUP BY "Action"
ORDER BY count DESC;

-- Check for missing indexes
SELECT 
    schemaname,
    tablename,
    attname,
    n_distinct,
    correlation
FROM pg_stats 
WHERE tablename = 'UserAuditLogs'
ORDER BY n_distinct DESC;
```

### Data Archiving

```sql
-- Create archive table for old audit logs
CREATE TABLE "UserAuditLogsArchive" (LIKE "UserAuditLogs");

-- Archive old audit logs (older than 1 year)
INSERT INTO "UserAuditLogsArchive"
SELECT * FROM "UserAuditLogs"
WHERE "CreatedAt" < NOW() - INTERVAL '1 year';

-- Delete archived logs from main table
DELETE FROM "UserAuditLogs"
WHERE "CreatedAt" < NOW() - INTERVAL '1 year';
```

---

## 🔍 Troubleshooting

### Common Issues

#### **Migration Fails with Permission Error**
```sql
-- Grant necessary permissions
GRANT CREATE ON SCHEMA public TO your_username;
GRANT USAGE ON SCHEMA public TO your_username;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO your_username;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO your_username;
```

#### **Trigger Not Firing**
```sql
-- Check if trigger is enabled
SELECT tgname, tgenabled 
FROM pg_trigger 
WHERE tgname = 'user_audit_trigger_with_error_handling';

-- Enable trigger if disabled
ALTER TABLE "Users" ENABLE TRIGGER user_audit_trigger_with_error_handling;
```

#### **Function Compilation Errors**
```sql
-- Check function definition
SELECT prosrc FROM pg_proc WHERE proname = 'log_user_changes_with_error_handling';

-- Recreate function if needed
DROP FUNCTION IF EXISTS log_user_changes_with_error_handling();
-- Then run the function creation script again
```

#### **Performance Issues**
```sql
-- Check for missing indexes
SELECT 
    schemaname,
    tablename,
    indexname,
    indexdef
FROM pg_indexes 
WHERE tablename IN ('Users', 'UserAuditLogs')
ORDER BY tablename, indexname;

-- Analyze table statistics
ANALYZE "Users";
ANALYZE "UserAuditLogs";
```

### Debug Queries

```sql
-- Check recent audit activity
SELECT 
    "Action", 
    COUNT(*) as count, 
    MAX("CreatedAt") as last_occurrence
FROM "UserAuditLogs"
WHERE "CreatedAt" > NOW() - INTERVAL '24 hours'
GROUP BY "Action"
ORDER BY count DESC;

-- Check trigger performance
SELECT 
    schemaname,
    tablename,
    triggername,
    triggerdef
FROM pg_triggers 
WHERE tablename = 'Users';

-- Check function performance
SELECT 
    schemaname,
    functionname,
    definition
FROM pg_functions 
WHERE functionname LIKE '%audit%';
```

---

## 📚 Best Practices

### Migration Best Practices

1. **Always Backup**: Create database backup before running migrations
2. **Test First**: Test migrations on development environment
3. **Incremental Changes**: Make small, incremental changes
4. **Documentation**: Document all custom SQL scripts
5. **Version Control**: Keep all migration scripts in version control

### Performance Best Practices

1. **Index Strategy**: Create indexes for frequently queried fields
2. **Partitioning**: Consider table partitioning for large audit tables
3. **Archiving**: Implement data archiving for old audit logs
4. **Monitoring**: Monitor query performance and index usage
5. **Maintenance**: Regular VACUUM and ANALYZE operations

### Security Best Practices

1. **Permissions**: Use least privilege principle for database users
2. **Sensitive Data**: Never log sensitive information like passwords
3. **Access Control**: Implement proper access control for audit logs
4. **Encryption**: Consider encrypting sensitive audit data
5. **Compliance**: Follow relevant compliance requirements

### Maintenance Best Practices

1. **Regular Cleanup**: Implement automated cleanup of old audit logs
2. **Health Checks**: Monitor database health and performance
3. **Backup Strategy**: Implement comprehensive backup strategy
4. **Monitoring**: Set up alerts for database issues
5. **Documentation**: Keep migration documentation up to date

---

## 📋 Migration Checklist

### Pre-Migration
- [ ] Database backup created
- [ ] Migration scripts tested on development
- [ ] All dependencies identified
- [ ] Rollback plan prepared
- [ ] Maintenance window scheduled (if needed)

### During Migration
- [ ] Run EF Core migrations
- [ ] Execute custom SQL scripts
- [ ] Verify all objects created successfully
- [ ] Test trigger functionality
- [ ] Verify data integrity

### Post-Migration
- [ ] Run verification queries
- [ ] Test application functionality
- [ ] Monitor performance
- [ ] Update documentation
- [ ] Clean up temporary files

---

**Database Migrations** - Comprehensive migration system for SampleProject with audit logging, triggers, and performance optimization
