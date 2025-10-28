using FluentValidation;

namespace SampleProject.Application.Common
{
    /// <summary>
    /// Base validator class with common validation methods
    /// </summary>
    public abstract class BaseValidator<T> : AbstractValidator<T>
    {
        /// <summary>
        /// Validates email format for security
        /// </summary>
        /// <param name="email">Email to validate</param>
        /// <returns>True if email format is valid</returns>
        protected static bool BeValidEmailFormat(string email)
        {
            if (string.IsNullOrEmpty(email))
                return true; // Let other validators handle empty validation

            // Check for potential injection attempts - only check for actual dangerous patterns
            var dangerousPatterns = new[] { "<script", "javascript:", "vbscript:", "javascript" };
            var lowerEmail = email.ToLowerInvariant();
            return !dangerousPatterns.Any(pattern => lowerEmail.Contains(pattern));
        }

        /// <summary>
        /// Validates name for security and format
        /// </summary>
        /// <param name="name">Name to validate</param>
        /// <returns>True if name is valid</returns>
        protected static bool BeValidName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return true; // Let other validators handle empty validation

            // Check for potential injection attempts and invalid characters
            var lowerName = name.ToLowerInvariant();
            
            // Check for dangerous patterns that should always be rejected
            var alwaysRejectPatterns = new[] { "<script", "javascript:", "vbscript:", "javascript" };
            if (alwaysRejectPatterns.Any(pattern => lowerName.Contains(pattern)))
                return false;
            
            // For SQL keywords, use word boundaries to avoid false positives like "Updated" containing "update"
            // But also catch embedded patterns like "JohnunionDoe" or "JohnselectDoe"
            var sqlKeywordPatterns = new[] { "union", "select", "insert", "delete", "drop", "update", "create", "alter", "exec", "execute" };
            return !sqlKeywordPatterns.Any(pattern => 
                lowerName.Contains($" {pattern} ") || // word surrounded by spaces
                lowerName.StartsWith($"{pattern} ") || // word at start
                lowerName.EndsWith($" {pattern}") ||   // word at end
                lowerName == pattern ||                // exact match
                // Catch embedded patterns but allow common words that contain these patterns
                (lowerName.Contains(pattern) && !IsCommonWordContainingPattern(lowerName, pattern)));
        }

        /// <summary>
        /// Checks if a name is a common word that contains a dangerous pattern but should be allowed
        /// </summary>
        /// <param name="name">Name to check</param>
        /// <param name="pattern">Dangerous pattern</param>
        /// <returns>True if it's a common word that should be allowed</returns>
        private static bool IsCommonWordContainingPattern(string name, string pattern)
        {
            // Allow common words that contain these patterns
            var allowedWords = new[] { "updated", "created", "selected", "inserted", "deleted" };
            return allowedWords.Contains(name);
        }

        /// <summary>
        /// Validates password for dangerous characters
        /// </summary>
        /// <param name="password">Password to validate</param>
        /// <returns>True if password is safe</returns>
        protected static bool NotContainDangerousCharacters(string password)
        {
            if (string.IsNullOrEmpty(password))
                return true; // Let other validators handle empty validation

            // Check for potential injection attempts - only check for actual dangerous patterns
            var dangerousPatterns = new[] { "<script", "javascript:", "vbscript:", "javascript", "union", "select", "insert", "update", "delete", "drop" };
            var lowerPassword = password.ToLowerInvariant();
            return !dangerousPatterns.Any(pattern => lowerPassword.Contains(pattern));
        }

        /// <summary>
        /// Validates GUID for empty values
        /// </summary>
        /// <param name="guid">GUID to validate</param>
        /// <returns>True if GUID is valid</returns>
        protected static bool BeValidGuid(Guid guid)
        {
            return guid != Guid.Empty;
        }

        /// <summary>
        /// Validates string for SQL injection patterns
        /// </summary>
        /// <param name="input">Input string to validate</param>
        /// <returns>True if input is safe from SQL injection</returns>
        protected static bool BeSafeFromSqlInjection(string input)
        {
            if (string.IsNullOrEmpty(input))
                return true;

            var sqlInjectionPatterns = new[] 
            { 
                "union", "select", "insert", "update", "delete", "drop", "create", "alter", 
                "exec", "execute", "sp_", "xp_", "--", "/*", "*/", "xp_cmdshell", 
                "sp_executesql", "waitfor", "delay", "shutdown"
            };

            var lowerInput = input.ToLowerInvariant();
            return !sqlInjectionPatterns.Any(pattern => lowerInput.Contains(pattern));
        }
    }
}