using FluentAssertions;
using SampleProject.Application.Implementations;
using Xunit;

namespace SampleProject.Test.Unit.Infrastructure
{
    /// <summary>
    /// Unit tests for PasswordService
    /// </summary>
    public class PasswordServiceTests
    {
        private readonly PasswordService _passwordService;

        public PasswordServiceTests()
        {
            _passwordService = new PasswordService();
        }

        [Fact]
        public void HashPassword_ShouldGenerateConsistentHashLength()
        {
            // Arrange
            var password = "password123";
            var hashLengths = new List<int>();

            // Act
            for (int i = 0; i < 10; i++)
            {
                var (hash, salt) = _passwordService.HashPassword(password);
                hashLengths.Add(hash.Length);
            }

            // Assert
            hashLengths.Should().AllSatisfy(length => length.Should().BeGreaterThan(0));
            hashLengths.Should().AllSatisfy(length => length.Should().Be(hashLengths[0])); // All should be same length
        }

        [Fact]
        public void HashPassword_ShouldGenerateConsistentSaltLength()
        {
            // Arrange
            var password = "password123";
            var saltLengths = new List<int>();

            // Act
            for (int i = 0; i < 10; i++)
            {
                var (hash, salt) = _passwordService.HashPassword(password);
                saltLengths.Add(salt.Length);
            }

            // Assert
            saltLengths.Should().AllSatisfy(length => length.Should().BeGreaterThan(0));
            saltLengths.Should().AllSatisfy(length => length.Should().Be(saltLengths[0])); // All should be same length
        }

        [Fact]
        public void HashPassword_WithDifferentPasswords_ShouldReturnDifferentHashes()
        {
            // Arrange
            var password1 = "password123";
            var password2 = "password456";

            // Act
            var (hash1, salt1) = _passwordService.HashPassword(password1);
            var (hash2, salt2) = _passwordService.HashPassword(password2);

            // Assert
            hash1.Should().NotBe(hash2);
            salt1.Should().NotBe(salt2);
        }

        [Fact]
        public void HashPassword_WithEmptyPassword_ShouldThrowArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _passwordService.HashPassword(""));
        }

        [Fact]
        public void HashPassword_WithNullPassword_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _passwordService.HashPassword(null!));
        }

        [Fact]
        public void HashPassword_WithSamePassword_ShouldReturnDifferentHashes()
        {
            // Arrange
            var password = "password123";

            // Act
            var (hash1, salt1) = _passwordService.HashPassword(password);
            var (hash2, salt2) = _passwordService.HashPassword(password);

            // Assert
            hash1.Should().NotBe(hash2);
            salt1.Should().NotBe(salt2);
        }

        [Fact]
        public void HashPassword_WithValidPassword_ShouldReturnHashAndSalt()
        {
            // Arrange
            var password = "password123";

            // Act
            var (hash, salt) = _passwordService.HashPassword(password);

            // Assert
            hash.Should().NotBeNull();
            hash.Should().NotBeEmpty();
            salt.Should().NotBeNull();
            salt.Should().NotBeEmpty();
            hash.Should().NotBe(password);
            salt.Should().NotBe(password);
        }

        [Theory]
        [InlineData("password123")]
        [InlineData("P@ssw0rd!")]
        [InlineData("VeryLongPasswordWithSpecialCharacters!@#$%^&*()")]
        [InlineData("12345678")]
        [InlineData("abcdefgh")]
        public void HashPassword_WithVariousPasswords_ShouldWork(string password)
        {
            // Act
            var (hash, salt) = _passwordService.HashPassword(password);

            // Assert
            hash.Should().NotBeNull();
            hash.Should().NotBeEmpty();
            salt.Should().NotBeNull();
            salt.Should().NotBeEmpty();
        }

        [Fact]
        public void HashPassword_WithWhitespacePassword_ShouldThrowArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _passwordService.HashPassword("   "));
        }

        [Fact]
        public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
        {
            // Arrange
            var password = "password123";
            var (hash, salt) = _passwordService.HashPassword(password);

            // Act
            var isValid = _passwordService.VerifyPassword(password, hash, salt);

            // Assert
            isValid.Should().BeTrue();
        }

        [Fact]
        public void VerifyPassword_WithEmptyHash_ShouldThrowArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _passwordService.VerifyPassword("password123", "", "salt"));
        }

        [Fact]
        public void VerifyPassword_WithEmptyPassword_ShouldThrowArgumentException()
        {
            // Arrange
            var (hash, salt) = _passwordService.HashPassword("password123");

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _passwordService.VerifyPassword("", hash, salt));
        }

        [Fact]
        public void VerifyPassword_WithEmptySalt_ShouldThrowArgumentException()
        {
            // Arrange
            var (hash, salt) = _passwordService.HashPassword("password123");

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _passwordService.VerifyPassword("password123", hash, ""));
        }

        [Fact]
        public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
        {
            // Arrange
            var password = "password123";
            var wrongPassword = "wrongpassword";
            var (hash, salt) = _passwordService.HashPassword(password);

            // Act
            var isValid = _passwordService.VerifyPassword(wrongPassword, hash, salt);

            // Assert
            isValid.Should().BeFalse();
        }

        [Fact]
        public void VerifyPassword_WithNullHash_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _passwordService.VerifyPassword("password123", null!, "salt"));
        }

        [Fact]
        public void VerifyPassword_WithNullPassword_ShouldThrowArgumentNullException()
        {
            // Arrange
            var (hash, salt) = _passwordService.HashPassword("password123");

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _passwordService.VerifyPassword(null!, hash, salt));
        }

        [Fact]
        public void VerifyPassword_WithNullSalt_ShouldThrowArgumentNullException()
        {
            // Arrange
            var (hash, salt) = _passwordService.HashPassword("password123");

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _passwordService.VerifyPassword("password123", hash, null!));
        }

        [Theory]
        [InlineData("password123")]
        [InlineData("P@ssw0rd!")]
        [InlineData("VeryLongPasswordWithSpecialCharacters!@#$%^&*()")]
        [InlineData("12345678")]
        [InlineData("abcdefgh")]
        public void VerifyPassword_WithVariousPasswords_ShouldWork(string password)
        {
            // Arrange
            var (hash, salt) = _passwordService.HashPassword(password);

            // Act
            var isValid = _passwordService.VerifyPassword(password, hash, salt);

            // Assert
            isValid.Should().BeTrue();
        }
    }
}