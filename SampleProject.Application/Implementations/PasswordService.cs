using System.Security.Cryptography;
using System.Text;
using SampleProject.Application.Interfaces;

namespace SampleProject.Application.Implementations
{
    /// <summary>
    /// Service for password hashing and verification using PBKDF2
    /// </summary>
    public class PasswordService : IPasswordService
    {
        private const int SaltSize = 32; // 256 bits
        private const int HashSize = 32; // 256 bits
        private const int Iterations = 100000; // PBKDF2 iterations

        /// <summary>
        /// Hashes a password with a random salt using PBKDF2
        /// </summary>
        /// <param name="password">Plain text password</param>
        /// <returns>Tuple containing hashed password and salt</returns>
        public (string Hash, string Salt) HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Password cannot be null or empty", nameof(password));
            }

            // Generate random salt
            using var rng = RandomNumberGenerator.Create();
            var saltBytes = new byte[SaltSize];
            rng.GetBytes(saltBytes);

            // Hash password with salt
            using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, Iterations, HashAlgorithmName.SHA256);
            var hashBytes = pbkdf2.GetBytes(HashSize);

            // Convert to base64 strings
            var hash = Convert.ToBase64String(hashBytes);
            var salt = Convert.ToBase64String(saltBytes);

            return (hash, salt);
        }

        /// <summary>
        /// Verifies a password against a hash and salt using PBKDF2
        /// </summary>
        /// <param name="password">Plain text password</param>
        /// <param name="hash">Hashed password</param>
        /// <param name="salt">Salt used for hashing</param>
        /// <returns>True if password matches, false otherwise</returns>
        public bool VerifyPassword(string password, string hash, string salt)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash) || string.IsNullOrWhiteSpace(salt))
            {
                return false;
            }

            try
            {
                // Convert base64 strings back to bytes
                var saltBytes = Convert.FromBase64String(salt);
                var hashBytes = Convert.FromBase64String(hash);

                // Hash the provided password with the stored salt
                using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, Iterations, HashAlgorithmName.SHA256);
                var testHash = pbkdf2.GetBytes(HashSize);

                // Compare hashes using constant-time comparison
                return CryptographicOperations.FixedTimeEquals(hashBytes, testHash);
            }
            catch
            {
                return false;
            }
        }
    }
}
