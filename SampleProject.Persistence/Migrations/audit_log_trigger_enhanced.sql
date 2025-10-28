-- Enhanced Audit Log Trigger for User Changes
-- Created: 2025-01-27
-- Description: Comprehensive audit logging with detailed field tracking for UserEntity changes
-- This script enhances the existing audit logging to track all relevant fields from UserEntity

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
            'Roles', NEW."Roles"::text,
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
        
        IF OLD."Roles" IS DISTINCT FROM NEW."Roles" THEN
            changed_fields := array_append(changed_fields, 'Roles');
        END IF;
        
        IF OLD."IsActive" IS DISTINCT FROM NEW."IsActive" THEN
            changed_fields := array_append(changed_fields, 'IsActive');
        END IF;
        
        IF OLD."IsEmailVerified" IS DISTINCT FROM NEW."IsEmailVerified" THEN
            changed_fields := array_append(changed_fields, 'IsEmailVerified');
        END IF;
        
        IF OLD."LastLoginAt" IS DISTINCT FROM NEW."LastLoginAt" THEN
            changed_fields := array_append(changed_fields, 'LastLoginAt');
        END IF;
        
        IF OLD."RefreshToken" IS DISTINCT FROM NEW."RefreshToken" THEN
            changed_fields := array_append(changed_fields, 'RefreshToken');
        END IF;
        
        IF OLD."RefreshTokenExpiryTime" IS DISTINCT FROM NEW."RefreshTokenExpiryTime" THEN
            changed_fields := array_append(changed_fields, 'RefreshTokenExpiryTime');
        END IF;
        
        IF OLD."RefreshTokenUseCount" IS DISTINCT FROM NEW."RefreshTokenUseCount" THEN
            changed_fields := array_append(changed_fields, 'RefreshTokenUseCount');
        END IF;
        
        IF OLD."RefreshTokenLastUsedAt" IS DISTINCT FROM NEW."RefreshTokenLastUsedAt" THEN
            changed_fields := array_append(changed_fields, 'RefreshTokenLastUsedAt');
        END IF;
        
        IF OLD."UpdatedAt" IS DISTINCT FROM NEW."UpdatedAt" THEN
            changed_fields := array_append(changed_fields, 'UpdatedAt');
        END IF;

        -- Only log if there were actual changes
        IF array_length(changed_fields, 1) > 0 THEN
            -- Build comprehensive old and new values
            audit_data := jsonb_build_object(
                'ChangedFields', array_to_json(changed_fields),
                'OldValues', jsonb_build_object(
                    'Email', OLD."Email",
                    'FirstName', OLD."FirstName",
                    'LastName', OLD."LastName",
                    'Roles', OLD."Roles"::text,
                    'IsActive', OLD."IsActive",
                    'IsEmailVerified', OLD."IsEmailVerified",
                    'LastLoginAt', OLD."LastLoginAt",
                    'RefreshTokenExpiryTime', OLD."RefreshTokenExpiryTime",
                    'RefreshTokenUseCount', OLD."RefreshTokenUseCount",
                    'RefreshTokenLastUsedAt', OLD."RefreshTokenLastUsedAt",
                    'UpdatedAt', OLD."UpdatedAt"
                ),
                'NewValues', jsonb_build_object(
                    'Email', NEW."Email",
                    'FirstName', NEW."FirstName",
                    'LastName', NEW."LastName",
                    'Roles', NEW."Roles"::text,
                    'IsActive', NEW."IsActive",
                    'IsEmailVerified', NEW."IsEmailVerified",
                    'LastLoginAt', NEW."LastLoginAt",
                    'RefreshTokenExpiryTime', NEW."RefreshTokenExpiryTime",
                    'RefreshTokenUseCount', NEW."RefreshTokenUseCount",
                    'RefreshTokenLastUsedAt', NEW."RefreshTokenLastUsedAt",
                    'UpdatedAt', NEW."UpdatedAt"
                )
            );

            INSERT INTO "UserAuditLogs" (
                "Id", "UserId", "Action", "OldValue", "NewValue", "CreatedAt", "Notes"
            ) VALUES (
                gen_random_uuid(), 
                NEW."Id", 
                'Updated', 
                audit_data->'OldValues'::text,
                audit_data->'NewValues'::text,
                NOW(),
                'Fields changed: ' || array_to_string(changed_fields, ', ')
            );
        END IF;
        RETURN NEW;

    ELSIF TG_OP = 'DELETE' THEN
        -- Log user deletion with key information
        audit_data := jsonb_build_object(
            'Email', OLD."Email",
            'FirstName', OLD."FirstName",
            'LastName', OLD."LastName",
            'Roles', OLD."Roles"::text,
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
        CASE 
            WHEN ual."NewValue"::JSONB ? 'ChangedFields' 
            THEN (ual."NewValue"::JSONB->'ChangedFields')::TEXT
            ELSE 'N/A'
        END as changed_fields,
        CASE 
            WHEN ual."OldValue"::JSONB ? 'OldValues' 
            THEN ual."OldValue"::JSONB->'OldValues'
            ELSE ual."OldValue"::JSONB
        END as old_values,
        CASE 
            WHEN ual."NewValue"::JSONB ? 'NewValues' 
            THEN ual."NewValue"::JSONB->'NewValues'
            ELSE ual."NewValue"::JSONB
        END as new_values,
        ual."CreatedAt",
        ual."Notes"::TEXT
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
    created_users BIGINT,
    updated_users BIGINT,
    deleted_users BIGINT,
    most_active_user_id UUID,
    most_active_user_email TEXT,
    recent_activity_count BIGINT
) AS $$
BEGIN
    RETURN QUERY
    WITH audit_stats AS (
        SELECT 
            COUNT(*) as total_logs,
            COUNT(*) FILTER (WHERE "Action" = 'Created') as created_count,
            COUNT(*) FILTER (WHERE "Action" = 'Updated') as updated_count,
            COUNT(*) FILTER (WHERE "Action" = 'Deleted') as deleted_count
        FROM "UserAuditLogs"
    ),
    user_activity AS (
        SELECT 
            "UserId",
            COUNT(*) as activity_count
        FROM "UserAuditLogs"
        GROUP BY "UserId"
        ORDER BY activity_count DESC
        LIMIT 1
    ),
    recent_activity AS (
        SELECT COUNT(*) as recent_count
        FROM "UserAuditLogs"
        WHERE "CreatedAt" >= NOW() - INTERVAL '24 hours'
    )
    SELECT 
        ast.total_logs,
        ast.created_count,
        ast.updated_count,
        ast.deleted_count,
        ua."UserId",
        u."Email",
        ra.recent_count
    FROM audit_stats ast
    CROSS JOIN user_activity ua
    LEFT JOIN "Users" u ON u."Id" = ua."UserId"
    CROSS JOIN recent_activity ra;
END;
$$ LANGUAGE plpgsql;

-- =====================================================
-- 4. Create indexes for better performance
-- =====================================================
CREATE INDEX IF NOT EXISTS idx_user_audit_logs_user_id_action ON "UserAuditLogs"("UserId", "Action");
CREATE INDEX IF NOT EXISTS idx_user_audit_logs_created_at_desc ON "UserAuditLogs"("CreatedAt" DESC);
CREATE INDEX IF NOT EXISTS idx_user_audit_logs_action_created_at ON "UserAuditLogs"("Action", "CreatedAt" DESC);

-- =====================================================
-- 5. Verification and testing queries
-- =====================================================
-- Uncomment these queries to test the enhanced audit logging:

-- Test user creation audit
-- INSERT INTO "Users" ("Id", "Email", "FirstName", "LastName", "PasswordHash", "PasswordSalt", "Roles", "IsActive", "IsEmailVerified", "CreatedAt")
-- VALUES (gen_random_uuid(), 'test@example.com', 'Test', 'User', 'hash', 'salt', 1, true, false, NOW());

-- Test user update audit
-- UPDATE "Users" SET "FirstName" = 'Updated', "IsActive" = false WHERE "Email" = 'test@example.com';

-- Test user deletion audit
-- DELETE FROM "Users" WHERE "Email" = 'test@example.com';

-- View audit history for a specific user
-- SELECT * FROM get_user_audit_history((SELECT "Id" FROM "Users" LIMIT 1), 10);

-- View audit statistics
-- SELECT * FROM get_audit_statistics();

-- =====================================================
-- 6. Migration completion message
-- =====================================================
DO $$
BEGIN
    RAISE NOTICE 'Enhanced audit logging trigger installed successfully!';
    RAISE NOTICE 'Features:';
    RAISE NOTICE '- Comprehensive field tracking for all UserEntity properties';
    RAISE NOTICE '- JSON-based old/new value storage with change detection';
    RAISE NOTICE '- Helper functions for querying audit history';
    RAISE NOTICE '- Performance optimized with proper indexes';
    RAISE NOTICE '- Detailed change notes and field tracking';
END $$;
