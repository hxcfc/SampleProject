using SampleProject.Infrastructure.Implementations;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using FluentAssertions;
using Moq;
using Xunit;

namespace SampleProject.Test.Unit.Infrastructure
{
    /// <summary>
    /// Unit tests for CacheService
    /// </summary>
    public class CacheServiceTests
    {
        private readonly Mock<IMemoryCache> _memoryCacheMock;
        private readonly Mock<IDistributedCache> _distributedCacheMock;
        private readonly Mock<ILogger<CacheService>> _loggerMock;
        private readonly CacheService _cacheService;

        public CacheServiceTests()
        {
            _memoryCacheMock = new Mock<IMemoryCache>();
            _distributedCacheMock = new Mock<IDistributedCache>();
            _loggerMock = new Mock<ILogger<CacheService>>();
            _cacheService = new CacheService(_memoryCacheMock.Object, _distributedCacheMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetAsync_WithValueInMemoryCache_ShouldReturnValue()
        {
            // Arrange
            var key = "test_key";
            var expectedValue = "test_value";

            _memoryCacheMock
                .Setup(x => x.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny))
                .Callback((object k, out object v) => 
                {
                    v = expectedValue;
                })
                .Returns(true);

            // Act
            var result = await _cacheService.GetAsync<string>(key);

            // Assert
            result.Should().Be(expectedValue);
            _distributedCacheMock.Verify(x => x.GetAsync(key, It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task GetAsync_WithValueInDistributedCache_ShouldReturnValueAndSetMemoryCache()
        {
            // Arrange
            var key = "test_key";
            var expectedValue = "test_value";
            var jsonValue = "\"test_value\"";
            object? outValue = null;

            _memoryCacheMock
                .Setup(x => x.TryGetValue(key, out outValue))
                .Returns(false);

            _distributedCacheMock
                .Setup(x => x.GetAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(System.Text.Encoding.UTF8.GetBytes(jsonValue));

            _memoryCacheMock
                .Setup(x => x.CreateEntry(It.IsAny<object>()))
                .Returns(Mock.Of<ICacheEntry>());

            // Act
            var result = await _cacheService.GetAsync<string>(key);

            // Assert
            result.Should().Be(expectedValue);
            _distributedCacheMock.Verify(x => x.GetAsync(key, It.IsAny<CancellationToken>()), Times.Once);
            _memoryCacheMock.Verify(x => x.CreateEntry(key), Times.Once);
        }

        [Fact]
        public async Task GetAsync_WithNoValueInCache_ShouldReturnDefault()
        {
            // Arrange
            var key = "test_key";

            _memoryCacheMock
                .Setup(x => x.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny))
                .Returns(false);

            _distributedCacheMock
                .Setup(x => x.GetAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync((byte[]?)null);

            // Act
            var result = await _cacheService.GetAsync<string>(key);

            // Assert
            result.Should().BeNull();
            _distributedCacheMock.Verify(x => x.GetAsync(key, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task SetAsync_WithValidData_ShouldSetBothCaches()
        {
            // Arrange
            var key = "test_key";
            var value = "test_value";
            var expiration = TimeSpan.FromMinutes(30);

            _memoryCacheMock
                .Setup(x => x.CreateEntry(It.IsAny<object>()))
                .Returns(Mock.Of<ICacheEntry>());

            _distributedCacheMock
                .Setup(x => x.SetAsync(key, It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _cacheService.SetAsync(key, value, expiration);

            // Assert
            _memoryCacheMock.Verify(x => x.CreateEntry(key), Times.Once);
            _distributedCacheMock.Verify(x => x.SetAsync(key, It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task SetAsync_WithDefaultExpiration_ShouldUseDefaultExpiration()
        {
            // Arrange
            var key = "test_key";
            var value = "test_value";

            _memoryCacheMock
                .Setup(x => x.CreateEntry(It.IsAny<object>()))
                .Returns(Mock.Of<ICacheEntry>());

            _distributedCacheMock
                .Setup(x => x.SetAsync(key, It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _cacheService.SetAsync(key, value);

            // Assert
            _memoryCacheMock.Verify(x => x.CreateEntry(key), Times.Once);
            _distributedCacheMock.Verify(x => x.SetAsync(key, It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task RemoveAsync_WithValidKey_ShouldRemoveFromBothCaches()
        {
            // Arrange
            var key = "test_key";

            _memoryCacheMock
                .Setup(x => x.Remove(key))
                .Verifiable();

            _distributedCacheMock
                .Setup(x => x.RemoveAsync(key, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _cacheService.RemoveAsync(key);

            // Assert
            _memoryCacheMock.Verify(x => x.Remove(key), Times.Once);
            _distributedCacheMock.Verify(x => x.RemoveAsync(key, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task RemoveByPatternAsync_ShouldLogWarning()
        {
            // Arrange
            var pattern = "test_pattern*";

            // Act
            await _cacheService.RemoveByPatternAsync(pattern);

            // Assert
            // Verify that warning was logged
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("RemoveByPatternAsync is not fully implemented")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetAsync_WithException_ShouldLogErrorAndReturnDefault()
        {
            // Arrange
            var key = "test_key";
            var exception = new Exception("Cache error");

            _memoryCacheMock
                .Setup(x => x.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny))
                .Throws(exception);

            // Act
            var result = await _cacheService.GetAsync<string>(key);

            // Assert
            result.Should().BeNull();
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error retrieving cache value")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task SetAsync_WithException_ShouldLogError()
        {
            // Arrange
            var key = "test_key";
            var value = "test_value";
            var exception = new Exception("Cache error");

            _memoryCacheMock
                .Setup(x => x.CreateEntry(It.IsAny<object>()))
                .Throws(exception);

            // Act
            await _cacheService.SetAsync(key, value);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error setting cache value")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
