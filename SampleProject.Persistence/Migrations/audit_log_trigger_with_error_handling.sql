-- Enhanced Audit Log Trigger with Error Handling
-- Created: 2025-01-28
-- Description: Comprehensive audit logging with detailed field tracking and proper error handling
-- This script enhances the existing audit logging with RAISE NOTICE and exception handling

-- =====================================================
-- 1. Enhanced trigger function with comprehensive error handling
-- =====================================================
CREATE OR REPLACE FUNCTION log_user_changes_with_error_handling()
RETURNS TRIGGER AS $$
DECLARE
    changed_fields TEXT[] := '{}';
    field_name TEXT;
    old_val TEXT;
    new_val TEXT;
    audit_data JSONB;
    error_message TEXT;
    operation_context TEXT;
BEGIN
    -- Set operation context for logging
    operation_context := TG_OP || ' operation on Users table';
    
    -- Log operation start
    RAISE NOTICE 'Starting audit log for % - User ID: %', 
        operation_context, 
        COALESCE(NEW."Id", OLD."Id")::text;

    IF TG_OP = 'INSERT' THEN
        BEGIN
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
            
            RAISE NOTICE 'Successfully logged user creation for User ID: %', NEW."Id"::text;
            RETURN NEW;
            
        EXCEPTION
            WHEN OTHERS THEN
                error_message := 'Failed to log user creation: ' || SQLERRM;
                RAISE NOTICE 'ERROR in audit log (INSERT): %', error_message;
                -- Log to a separate error table if it exists, otherwise just notice
                BEGIN
                    INSERT INTO "AuditLogErrors" (
                        "Id", "UserId", "Operation", "ErrorMessage", "CreatedAt", "OriginalData"
                    ) VALUES (
                        gen_random_uuid(),
                        NEW."Id",
                        'INSERT_AUDIT_LOG_FAILED',
                        error_message,
                        NOW(),
                        audit_data::text
                    );
                EXCEPTION
                    WHEN OTHERS THEN
                        RAISE NOTICE 'Failed to log audit error to error table: %', SQLERRM;
                END;
                -- Don't fail the original operation, just log the error
                RETURN NEW;
        END;

    ELSIF TG_OP = 'UPDATE' THEN
        BEGIN
            -- Check each field for changes and build change log
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
            
            IF OLD."LastLoginAt" IS DISTINCT FROM NEW."LastLoginAt" THEN
                changed_fields := array_append(changed_fields, 'LastLoginAt');
            END IF;
            
            -- Only log if there are actual changes
            IF array_length(changed_fields, 1) > 0 THEN
                -- Build old values JSON
                audit_data := jsonb_build_object(
                    'Email', OLD."Email",
                    'FirstName', OLD."FirstName",
                    'LastName', OLD."LastName",
                    'Role', OLD."Role"::text,
                    'IsActive', OLD."IsActive",
                    'IsEmailVerified', OLD."IsEmailVerified",
                    'LastLoginAt', OLD."LastLoginAt"
                );
                
                -- Build new values JSON
                audit_data := jsonb_build_object(
                    'Email', NEW."Email",
                    'FirstName', NEW."FirstName",
                    'LastName', NEW."LastName",
                    'Role', NEW."Role"::text,
                    'IsActive', NEW."IsActive",
                    'IsEmailVerified', NEW."IsEmailVerified",
                    'LastLoginAt', NEW."LastLoginAt"
                );
                
                INSERT INTO "UserAuditLogs" (
                    "Id", "UserId", "Action", "OldValue", "NewValue", "CreatedAt", "Notes"
                ) VALUES (
                    gen_random_uuid(), 
                    NEW."Id", 
                    'Updated',
                    jsonb_build_object(
                        'Email', OLD."Email",
                        'FirstName', OLD."FirstName",
                        'LastName', OLD."LastName",
                        'Role', OLD."Role"::text,
                        'IsActive', OLD."IsActive",
                        'IsEmailVerified', OLD."IsEmailVerified",
                        'LastLoginAt', OLD."LastLoginAt"
                    )::text,
                    jsonb_build_object(
                        'Email', NEW."Email",
                        'FirstName', NEW."FirstName",
                        'LastName', NEW."LastName",
                        'Role', NEW."Role"::text,
                        'IsActive', NEW."IsActive",
                        'IsEmailVerified', NEW."IsEmailVerified",
                        'LastLoginAt', NEW."LastLoginAt"
                    )::text,
                    NOW(),
                    'Fields changed: ' || array_to_string(changed_fields, ', ')
                );
                
                RAISE NOTICE 'Successfully logged user update for User ID: % - Changed fields: %', 
                    NEW."Id"::text, array_to_string(changed_fields, ', ');
            ELSE
                RAISE NOTICE 'No changes detected for User ID: % - skipping audit log', NEW."Id"::text;
            END IF;
            
            RETURN NEW;
            
        EXCEPTION
            WHEN OTHERS THEN
                error_message := 'Failed to log user update: ' || SQLERRM;
                RAISE NOTICE 'ERROR in audit log (UPDATE): %', error_message;
                -- Log to error table
                BEGIN
                    INSERT INTO "AuditLogErrors" (
                        "Id", "UserId", "Operation", "ErrorMessage", "CreatedAt", "OriginalData"
                    ) VALUES (
                        gen_random_uuid(),
                        NEW."Id",
                        'UPDATE_AUDIT_LOG_FAILED',
                        error_message,
                        NOW(),
                        jsonb_build_object(
                            'OldEmail', OLD."Email",
                            'NewEmail', NEW."Email",
                            'ChangedFields', changed_fields
                        )::text
                    );
                EXCEPTION
                    WHEN OTHERS THEN
                        RAISE NOTICE 'Failed to log audit error to error table: %', SQLERRM;
                END;
                -- Don't fail the original operation
                RETURN NEW;
        END;

    ELSIF TG_OP = 'DELETE' THEN
        BEGIN
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
            
            RAISE NOTICE 'Successfully logged user deletion for User ID: %', OLD."Id"::text;
            RETURN OLD;
            
        EXCEPTION
            WHEN OTHERS THEN
                error_message := 'Failed to log user deletion: ' || SQLERRM;
                RAISE NOTICE 'ERROR in audit log (DELETE): %', error_message;
                -- Log to error table
                BEGIN
                    INSERT INTO "AuditLogErrors" (
                        "Id", "UserId", "Operation", "ErrorMessage", "CreatedAt", "OriginalData"
                    ) VALUES (
                        gen_random_uuid(),
                        OLD."Id",
                        'DELETE_AUDIT_LOG_FAILED',
                        error_message,
                        NOW(),
                        audit_data::text
                    );
                EXCEPTION
                    WHEN OTHERS THEN
                        RAISE NOTICE 'Failed to log audit error to error table: %', SQLERRM;
                END;
                -- Don't fail the original operation
                RETURN OLD;
        END;
    END IF;
    
    RETURN NULL;
EXCEPTION
    WHEN OTHERS THEN
        error_message := 'Critical error in audit log trigger: ' || SQLERRM;
        RAISE NOTICE 'CRITICAL ERROR in audit log trigger: %', error_message;
        -- Try to log critical error
        BEGIN
            INSERT INTO "AuditLogErrors" (
                "Id", "UserId", "Operation", "ErrorMessage", "CreatedAt", "OriginalData"
            ) VALUES (
                gen_random_uuid(),
                COALESCE(NEW."Id", OLD."Id"),
                'CRITICAL_TRIGGER_ERROR',
                error_message,
                NOW(),
                'Trigger operation: ' || TG_OP
            );
        EXCEPTION
            WHEN OTHERS THEN
                RAISE NOTICE 'Failed to log critical audit error: %', SQLERRM;
        END;
        -- Return appropriate value based on operation
        IF TG_OP = 'DELETE' THEN
            RETURN OLD;
        ELSE
            RETURN NEW;
        END IF;
END;
$$ LANGUAGE plpgsql;

-- =====================================================
-- 2. Create error logging table for audit failures
-- =====================================================
CREATE TABLE IF NOT EXISTS "AuditLogErrors" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" UUID,
    "Operation" VARCHAR(50) NOT NULL,
    "ErrorMessage" TEXT NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT NOW(),
    "OriginalData" TEXT,
    "StackTrace" TEXT
);

-- Create index for performance
CREATE INDEX IF NOT EXISTS "IX_AuditLogErrors_UserId" ON "AuditLogErrors" ("UserId");
CREATE INDEX IF NOT EXISTS "IX_AuditLogErrors_CreatedAt" ON "AuditLogErrors" ("CreatedAt");
CREATE INDEX IF NOT EXISTS "IX_AuditLogErrors_Operation" ON "AuditLogErrors" ("Operation");

-- =====================================================
-- 3. Create enhanced trigger (replace existing one)
-- =====================================================
-- Drop existing triggers if they exist
DROP TRIGGER IF EXISTS user_audit_trigger ON "Users";
DROP TRIGGER IF EXISTS user_audit_trigger_enhanced ON "Users";

-- Create the enhanced trigger with error handling
CREATE TRIGGER user_audit_trigger_with_error_handling
AFTER INSERT OR UPDATE OR DELETE ON "Users"
FOR EACH ROW EXECUTE FUNCTION log_user_changes_with_error_handling();

-- =====================================================
-- 4. Create helper functions for monitoring and maintenance
-- =====================================================

-- Function to get audit log errors
CREATE OR REPLACE FUNCTION get_audit_log_errors(
    start_date TIMESTAMP DEFAULT NOW() - INTERVAL '7 days',
    end_date TIMESTAMP DEFAULT NOW()
)
RETURNS TABLE (
    error_id UUID,
    user_id UUID,
    operation VARCHAR(50),
    error_message TEXT,
    created_at TIMESTAMP,
    original_data TEXT
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        ale."Id",
        ale."UserId",
        ale."Operation",
        ale."ErrorMessage",
        ale."CreatedAt",
        ale."OriginalData"
    FROM "AuditLogErrors" ale
    WHERE ale."CreatedAt" BETWEEN start_date AND end_date
    ORDER BY ale."CreatedAt" DESC;
END;
$$ LANGUAGE plpgsql;

-- Function to clean up old audit log errors (older than specified days)
CREATE OR REPLACE FUNCTION cleanup_audit_log_errors(days_to_keep INTEGER DEFAULT 30)
RETURNS INTEGER AS $$
DECLARE
    deleted_count INTEGER;
BEGIN
    DELETE FROM "AuditLogErrors" 
    WHERE "CreatedAt" < NOW() - (days_to_keep || ' days')::INTERVAL;
    
    GET DIAGNOSTICS deleted_count = ROW_COUNT;
    
    RAISE NOTICE 'Cleaned up % audit log error records older than % days', deleted_count, days_to_keep;
    
    RETURN deleted_count;
END;
$$ LANGUAGE plpgsql;

-- Function to get audit log statistics
CREATE OR REPLACE FUNCTION get_audit_log_statistics()
RETURNS TABLE (
    total_audit_logs BIGINT,
    total_errors BIGINT,
    error_rate NUMERIC,
    last_audit_log TIMESTAMP,
    last_error TIMESTAMP
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        (SELECT COUNT(*) FROM "UserAuditLogs") as total_audit_logs,
        (SELECT COUNT(*) FROM "AuditLogErrors") as total_errors,
        CASE 
            WHEN (SELECT COUNT(*) FROM "UserAuditLogs") > 0 THEN
                ROUND(
                    (SELECT COUNT(*) FROM "AuditLogErrors")::NUMERIC / 
                    (SELECT COUNT(*) FROM "UserAuditLogs")::NUMERIC * 100, 
                    2
                )
            ELSE 0
        END as error_rate,
        (SELECT MAX("CreatedAt") FROM "UserAuditLogs") as last_audit_log,
        (SELECT MAX("CreatedAt") FROM "AuditLogErrors") as last_error;
END;
$$ LANGUAGE plpgsql;

-- =====================================================
-- 5. Verification and testing
-- =====================================================

-- Test the trigger with a sample operation
DO $$
DECLARE
    test_user_id UUID;
    audit_count_before INTEGER;
    audit_count_after INTEGER;
    error_count_before INTEGER;
    error_count_after INTEGER;
BEGIN
    -- Get initial counts
    SELECT COUNT(*) INTO audit_count_before FROM "UserAuditLogs";
    SELECT COUNT(*) INTO error_count_before FROM "AuditLogErrors";
    
    RAISE NOTICE 'Starting trigger test - Audit logs before: %, Errors before: %', 
        audit_count_before, error_count_before;
    
    -- Test INSERT
    test_user_id := gen_random_uuid();
    INSERT INTO "Users" ("Id", "Email", "FirstName", "LastName", "PasswordHash", "PasswordSalt")
    VALUES (test_user_id, 'test@example.com', 'Test', 'User', 'hash', 'salt');
    
    -- Test UPDATE
    UPDATE "Users" SET "FirstName" = 'UpdatedTest' WHERE "Id" = test_user_id;
    
    -- Test DELETE
    DELETE FROM "Users" WHERE "Id" = test_user_id;
    
    -- Get final counts
    SELECT COUNT(*) INTO audit_count_after FROM "UserAuditLogs";
    SELECT COUNT(*) INTO error_count_after FROM "AuditLogErrors";
    
    RAISE NOTICE 'Trigger test completed - Audit logs after: %, Errors after: %', 
        audit_count_after, error_count_after;
    RAISE NOTICE 'New audit logs created: %, New errors: %', 
        audit_count_after - audit_count_before, error_count_after - error_count_before;
END;
$$;

-- =====================================================
-- 6. Create monitoring view for audit log health
-- =====================================================
CREATE OR REPLACE VIEW "AuditLogHealth" AS
SELECT 
    'Audit Log Statistics' as metric_type,
    (SELECT COUNT(*) FROM "UserAuditLogs") as total_records,
    (SELECT COUNT(*) FROM "AuditLogErrors") as error_records,
    CASE 
        WHEN (SELECT COUNT(*) FROM "UserAuditLogs") > 0 THEN
            ROUND(
                (SELECT COUNT(*) FROM "AuditLogErrors")::NUMERIC / 
                (SELECT COUNT(*) FROM "UserAuditLogs")::NUMERIC * 100, 
                2
            )
        ELSE 0
    END as error_percentage,
    (SELECT MAX("CreatedAt") FROM "UserAuditLogs") as last_audit_log,
    (SELECT MAX("CreatedAt") FROM "AuditLogErrors") as last_error,
    NOW() as check_time;

-- Grant appropriate permissions
GRANT SELECT ON "AuditLogHealth" TO PUBLIC;
GRANT SELECT ON "AuditLogErrors" TO PUBLIC;
GRANT EXECUTE ON FUNCTION get_audit_log_errors TO PUBLIC;
GRANT EXECUTE ON FUNCTION cleanup_audit_log_errors TO PUBLIC;
GRANT EXECUTE ON FUNCTION get_audit_log_statistics TO PUBLIC;

RAISE NOTICE 'Enhanced audit log trigger with error handling has been successfully installed!';
RAISE NOTICE 'Use SELECT * FROM "AuditLogHealth" to monitor audit log health';
RAISE NOTICE 'Use SELECT * FROM get_audit_log_errors() to view recent errors';
RAISE NOTICE 'Use SELECT cleanup_audit_log_errors(30) to clean up old error records';
