namespace SampleProject.Domain.Enums
{
    /// <summary>
    /// Enum representing user roles in the system using flags
    /// </summary>
    [Flags]
    public enum UserRole
    {
        /// <summary>
        /// No roles assigned
        /// </summary>
        None = 0,

        /// <summary>
        /// Standard user role
        /// </summary>
        User = 1,

        /// <summary>
        /// Administrator role with full access
        /// </summary>
        Admin = 2
    }

    /// <summary>
    /// Extension methods for UserRole enum
    /// </summary>
    public static class UserRoleExtensions
    {
        /// <summary>
        /// Gets the string representation of the role
        /// </summary>
        /// <param name="role">User role</param>
        /// <returns>Role name as string</returns>
        public static string GetName(this UserRole role)
        {
            return role switch
            {
                UserRole.None => "None",
                UserRole.User => "User",
                UserRole.Admin => "Admin",
                _ => throw new ArgumentOutOfRangeException(nameof(role), role, "Unknown role")
            };
        }

        /// <summary>
        /// Parses string to UserRole enum
        /// </summary>
        /// <param name="roleName">Role name as string</param>
        /// <returns>UserRole enum value</returns>
        public static UserRole ParseRole(string roleName)
        {
            return roleName switch
            {
                "None" => UserRole.None,
                "User" => UserRole.User,
                "Admin" => UserRole.Admin,
                _ => throw new ArgumentException($"Unknown role: {roleName}", nameof(roleName))
            };
        }

        /// <summary>
        /// Gets all available roles as strings
        /// </summary>
        /// <returns>Array of all role names</returns>
        public static string[] GetAllRoleNames()
        {
            return Enum.GetValues<UserRole>()
                .Where(r => r != UserRole.None)
                .Select(r => r.GetName())
                .ToArray();
        }

        /// <summary>
        /// Gets all roles from a flags enum value
        /// </summary>
        /// <param name="roles">Roles flags</param>
        /// <returns>Array of individual role names</returns>
        public static string[] GetRoleNames(this UserRole roles)
        {
            return Enum.GetValues<UserRole>()
                .Where(r => r != UserRole.None && roles.HasFlag(r))
                .Select(r => r.GetName())
                .ToArray();
        }

        /// <summary>
        /// Checks if the user has admin role
        /// </summary>
        /// <param name="roles">User roles</param>
        /// <returns>True if user has admin role</returns>
        public static bool IsAdmin(this UserRole roles)
        {
            return roles.HasFlag(UserRole.Admin);
        }

        /// <summary>
        /// Checks if the user has user role
        /// </summary>
        /// <param name="roles">User roles</param>
        /// <returns>True if user has user role</returns>
        public static bool IsUser(this UserRole roles)
        {
            return roles.HasFlag(UserRole.User);
        }
    }
}
