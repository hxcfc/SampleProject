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
DECLARE
    error_message TEXT;
BEGIN
    IF TG_OP = 'INSERT' THEN
        BEGIN
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
            RAISE NOTICE 'Successfully logged user creation for User ID: %', NEW."Id"::text;
            RETURN NEW;
        EXCEPTION
            WHEN OTHERS THEN
                error_message := 'Failed to log user creation: ' || SQLERRM;
                RAISE NOTICE 'ERROR in audit log (INSERT): %', error_message;
                -- Don't fail the original operation, just log the error
                RETURN NEW;
        END;

    ELSIF TG_OP = 'UPDATE' THEN
        BEGIN
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
                RAISE NOTICE 'Successfully logged user update for User ID: %', NEW."Id"::text;
            ELSE
                RAISE NOTICE 'No changes detected for User ID: % - skipping audit log', NEW."Id"::text;
            END IF;
            RETURN NEW;
        EXCEPTION
            WHEN OTHERS THEN
                error_message := 'Failed to log user update: ' || SQLERRM;
                RAISE NOTICE 'ERROR in audit log (UPDATE): %', error_message;
                -- Don't fail the original operation
                RETURN NEW;
        END;

    ELSIF TG_OP = 'DELETE' THEN
        BEGIN
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
            RAISE NOTICE 'Successfully logged user deletion for User ID: %', OLD."Id"::text;
            RETURN OLD;
        EXCEPTION
            WHEN OTHERS THEN
                error_message := 'Failed to log user deletion: ' || SQLERRM;
                RAISE NOTICE 'ERROR in audit log (DELETE): %', error_message;
                -- Don't fail the original operation
                RETURN OLD;
        END;
    END IF;
    
    RETURN NULL;
EXCEPTION
    WHEN OTHERS THEN
        error_message := 'Critical error in audit log trigger: ' || SQLERRM;
        RAISE NOTICE 'CRITICAL ERROR in audit log trigger: %', error_message;
        -- Return appropriate value based on operation
        IF TG_OP = 'DELETE' THEN
            RETURN OLD;
        ELSE
            RETURN NEW;
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

-- =====================================================
-- 6. Verification queries (optional, comment out if not needed)
-- =====================================================
-- SELECT 'Migration completed successfully!' AS status;
-- SELECT COUNT(*) AS user_audit_logs_count FROM "UserAuditLogs";
-- SELECT COUNT(*) AS users_with_refresh_token_usage FROM "Users" WHERE "RefreshTokenUseCount" > 0;
