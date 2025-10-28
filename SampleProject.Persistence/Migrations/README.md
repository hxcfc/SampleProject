# Database Migrations

## UserAuditLog Migration

### File: `add_user_audit_log.sql`

This migration adds:
- ✅ `UserAuditLogs` table for audit history
- ✅ New columns to `Users` table (RefreshTokenUseCount, RefreshTokenLastUsedAt)
- ✅ Automatic trigger for logging user changes
- ✅ Indexes for performance

### How to apply:

#### Option 1: Using psql (PostgreSQL command line)
```bash
psql -U your_username -d your_database -f add_user_audit_log.sql
```

#### Option 2: Using pgAdmin
1. Open pgAdmin
2. Connect to your database
3. Right-click on database → Query Tool
4. Open and run the SQL file

#### Option 3: Using Entity Framework (Recommended)
```bash
# Generate migration
dotnet ef migrations add AddUserAuditLog --project SampleProject.Persistence

# Apply migration
dotnet ef database update --project SampleProject.Persistence
```

### Verification:

After running the migration, verify it worked:

```sql
-- Check if table exists
SELECT COUNT(*) FROM "UserAuditLogs";

-- Check if trigger exists
SELECT tgname FROM pg_trigger WHERE tgname = 'user_audit_trigger';

-- Check new columns
SELECT column_name, data_type 
FROM information_schema.columns 
WHERE table_name = 'Users' 
AND column_name IN ('RefreshTokenUseCount', 'RefreshTokenLastUsedAt');

-- Test trigger (create a test user and check audit log)
INSERT INTO "Users" ("Id", "Email", "FirstName", "LastName", "PasswordHash", "PasswordSalt")
VALUES (gen_random_uuid(), 'test@example.com', 'Test', 'User', 'hash', 'salt');

SELECT * FROM "UserAuditLogs" ORDER BY "CreatedAt" DESC LIMIT 5;
```

### What the trigger does:

1. **On INSERT**: Logs user creation with full details
2. **On UPDATE**: Logs changes with old and new values
3. **On DELETE**: Logs user deletion

### Important Notes:

- Trigger only logs when data actually changes (not on every UPDATE)
- All timestamps are in UTC
- Passwords are NOT logged
- JSON format for old/new values for easy querying

### Rollback (if needed):

```sql
-- Drop trigger
DROP TRIGGER IF EXISTS user_audit_trigger ON "Users";

-- Drop function
DROP FUNCTION IF EXISTS log_user_changes();

-- Drop table (careful - you'll lose all audit history!)
-- DROP TABLE IF EXISTS "UserAuditLogs";

-- Remove columns from Users table
-- ALTER TABLE "Users" DROP COLUMN IF EXISTS "RefreshTokenUseCount";
-- ALTER TABLE "Users" DROP COLUMN IF EXISTS "RefreshTokenLastUsedAt";
```
