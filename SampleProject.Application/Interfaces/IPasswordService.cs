namespace SampleProject.Application.Interfaces
{
    /// <summary>
    /// Interface for password hashing and verification
    /// </summary>
    public interface IPasswordService
    {
        /// <summary>
        /// Hashes a password with a random salt
        /// </summary>
        /// <param name="password">Plain text password</param>
        /// <returns>Tuple containing hashed password and salt</returns>
        (string Hash, string Salt) HashPassword(string password);

        /// <summary>
        /// Verifies a password against a hash and salt
        /// </summary>
        /// <param name="password">Plain text password</param>
        /// <param name="hash">Hashed password</param>
        /// <param name="salt">Salt used for hashing</param>
        /// <returns>True if password matches, false otherwise</returns>
        bool VerifyPassword(string password, string hash, string salt);
    }
}
