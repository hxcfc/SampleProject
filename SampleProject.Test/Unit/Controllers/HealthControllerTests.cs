using SampleProject.Controllers.Diagnostic;
using Microsoft.AspNetCore.Mvc;
using FluentAssertions;
using Xunit;

namespace SampleProject.Test.Unit.Controllers
{
    /// <summary>
    /// Unit tests for HealthController
    /// </summary>
    public class HealthControllerTests
    {
        private readonly HealthController _controller;

        public HealthControllerTests()
        {
            _controller = new HealthController();
        }

        [Fact]
        public void GetHealth_WithValidRequest_ShouldReturnOk()
        {
            // Act
            var result = _controller.GetHealth();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().NotBeNull();
        }

        [Fact]
        public void GetHealth_WithValidRequest_ShouldReturnHealthStatus()
        {
            // Act
            var result = _controller.GetHealth();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().NotBeNull();
            
            // Use reflection to access properties
            var healthStatusType = okResult.Value.GetType();
            var statusProperty = healthStatusType.GetProperty("status");
            var timestampProperty = healthStatusType.GetProperty("timestamp");
            var versionProperty = healthStatusType.GetProperty("version");
            var environmentProperty = healthStatusType.GetProperty("environment");
            
            statusProperty.Should().NotBeNull();
            timestampProperty.Should().NotBeNull();
            versionProperty.Should().NotBeNull();
            environmentProperty.Should().NotBeNull();
            
            statusProperty!.GetValue(okResult.Value).Should().Be("healthy");
            timestampProperty!.GetValue(okResult.Value).Should().NotBeNull();
            versionProperty!.GetValue(okResult.Value).Should().NotBeNull();
            environmentProperty!.GetValue(okResult.Value).Should().NotBeNull();
        }

        [Fact]
        public void GetHealth_WithValidRequest_ShouldReturnCurrentTime()
        {
            // Arrange
            var beforeCall = DateTime.UtcNow;

            // Act
            var result = _controller.GetHealth();

            // Assert
            var afterCall = DateTime.UtcNow;
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            
            // Use reflection to access timestamp property
            var healthStatusType = okResult!.Value.GetType();
            var timestampProperty = healthStatusType.GetProperty("timestamp");
            timestampProperty.Should().NotBeNull();
            
            var timestamp = (DateTime)timestampProperty!.GetValue(okResult.Value)!;
            timestamp.Should().BeOnOrAfter(beforeCall);
            timestamp.Should().BeOnOrBefore(afterCall);
        }

        [Fact]
        public void GetHealth_WithValidRequest_ShouldReturnValidTypes()
        {
            // Act
            var result = _controller.GetHealth();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().NotBeNull();
            
            // Use reflection to access properties and check types
            var healthStatusType = okResult.Value.GetType();
            var statusProperty = healthStatusType.GetProperty("status");
            var timestampProperty = healthStatusType.GetProperty("timestamp");
            var versionProperty = healthStatusType.GetProperty("version");
            var environmentProperty = healthStatusType.GetProperty("environment");
            
            statusProperty.Should().NotBeNull();
            timestampProperty.Should().NotBeNull();
            versionProperty.Should().NotBeNull();
            environmentProperty.Should().NotBeNull();
            
            statusProperty!.GetValue(okResult.Value).Should().BeOfType<string>();
            timestampProperty!.GetValue(okResult.Value).Should().BeOfType<DateTime>();
            versionProperty!.GetValue(okResult.Value).Should().BeOfType<string>();
            environmentProperty!.GetValue(okResult.Value).Should().BeOfType<string>();
        }

        [Fact]
        public void GetHealth_WithValidRequest_ShouldReturn200StatusCode()
        {
            // Act
            var result = _controller.GetHealth();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.StatusCode.Should().Be(200);
        }

        [Fact]
        public void GetHealth_WithValidRequest_ShouldNotThrow()
        {
            // Act & Assert
            var result = _controller.GetHealth();
            result.Should().NotBeNull();
        }

        [Fact]
        public void GetHealth_WithValidRequest_ShouldReturnConsistentFormat()
        {
            // Act
            var result1 = _controller.GetHealth();
            var result2 = _controller.GetHealth();

            // Assert
            result1.Should().BeOfType<OkObjectResult>();
            result2.Should().BeOfType<OkObjectResult>();
            
            var okResult1 = result1 as OkObjectResult;
            var okResult2 = result2 as OkObjectResult;
            
            // Use reflection to access properties
            var healthStatus1Type = okResult1!.Value.GetType();
            var healthStatus2Type = okResult2!.Value.GetType();
            
            var status1Property = healthStatus1Type.GetProperty("status");
            var version1Property = healthStatus1Type.GetProperty("version");
            var environment1Property = healthStatus1Type.GetProperty("environment");
            
            var status2Property = healthStatus2Type.GetProperty("status");
            var version2Property = healthStatus2Type.GetProperty("version");
            var environment2Property = healthStatus2Type.GetProperty("environment");
            
            status1Property!.GetValue(okResult1.Value).Should().Be(status2Property!.GetValue(okResult2.Value));
            version1Property!.GetValue(okResult1.Value).Should().Be(version2Property!.GetValue(okResult2.Value));
            environment1Property!.GetValue(okResult1.Value).Should().Be(environment2Property!.GetValue(okResult2.Value));
        }

        [Fact]
        public void GetHealth_WithValidRequest_ShouldReturnImmediately()
        {
            // Arrange
            var startTime = DateTime.UtcNow;

            // Act
            var result = _controller.GetHealth();

            // Assert
            var endTime = DateTime.UtcNow;
            var duration = endTime - startTime;
            
            result.Should().BeOfType<OkObjectResult>();
            duration.Should().BeLessThan(TimeSpan.FromMilliseconds(100)); // Should be very fast
        }

        [Fact]
        public void GetHealth_WithValidRequest_ShouldNotDependOnExternalServices()
        {
            // Act
            var result = _controller.GetHealth();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            
            // Use reflection to access status property
            var healthStatusType = okResult!.Value.GetType();
            var statusProperty = healthStatusType.GetProperty("status");
            statusProperty.Should().NotBeNull();
            
            statusProperty!.GetValue(okResult.Value).Should().Be("healthy"); // Should always be healthy for basic health check
        }

        [Fact]
        public void GetHealth_WithValidRequest_ShouldBeIdempotent()
        {
            // Act
            var result1 = _controller.GetHealth();
            var result2 = _controller.GetHealth();
            var result3 = _controller.GetHealth();

            // Assert
            result1.Should().BeOfType<OkObjectResult>();
            result2.Should().BeOfType<OkObjectResult>();
            result3.Should().BeOfType<OkObjectResult>();
            
            var okResult1 = result1 as OkObjectResult;
            var okResult2 = result2 as OkObjectResult;
            var okResult3 = result3 as OkObjectResult;
            
            // Use reflection to access status properties
            var healthStatus1Type = okResult1!.Value.GetType();
            var healthStatus2Type = okResult2!.Value.GetType();
            var healthStatus3Type = okResult3!.Value.GetType();
            
            var status1Property = healthStatus1Type.GetProperty("status");
            var status2Property = healthStatus2Type.GetProperty("status");
            var status3Property = healthStatus3Type.GetProperty("status");
            
            status1Property!.GetValue(okResult1.Value).Should().Be(status2Property!.GetValue(okResult2.Value));
            status2Property.GetValue(okResult2.Value).Should().Be(status3Property!.GetValue(okResult3.Value));
        }

        [Fact]
        public void Ping_WithValidRequest_ShouldReturnOk()
        {
            // Act
            var result = _controller.Ping();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().NotBeNull();
        }

        [Fact]
        public void Ping_WithValidRequest_ShouldReturnPongMessage()
        {
            // Act
            var result = _controller.Ping();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().NotBeNull();
            
            // Use reflection to access properties
            var pingResponseType = okResult.Value.GetType();
            var messageProperty = pingResponseType.GetProperty("message");
            var timestampProperty = pingResponseType.GetProperty("timestamp");
            
            messageProperty.Should().NotBeNull();
            timestampProperty.Should().NotBeNull();
            
            messageProperty!.GetValue(okResult.Value).Should().Be("pong");
            timestampProperty!.GetValue(okResult.Value).Should().NotBeNull();
        }

        [Fact]
        public void Ping_WithValidRequest_ShouldReturnCurrentTime()
        {
            // Arrange
            var beforeCall = DateTime.UtcNow;

            // Act
            var result = _controller.Ping();

            // Assert
            var afterCall = DateTime.UtcNow;
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            
            // Use reflection to access timestamp property
            var pingResponseType = okResult!.Value.GetType();
            var timestampProperty = pingResponseType.GetProperty("timestamp");
            timestampProperty.Should().NotBeNull();
            
            var timestamp = (DateTime)timestampProperty!.GetValue(okResult.Value)!;
            timestamp.Should().BeOnOrAfter(beforeCall);
            timestamp.Should().BeOnOrBefore(afterCall);
        }

        [Fact]
        public void GetInfo_WithValidRequest_ShouldReturnOk()
        {
            // Act
            var result = _controller.GetInfo();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().NotBeNull();
        }

        [Fact]
        public void GetInfo_WithValidRequest_ShouldReturnApplicationInfo()
        {
            // Act
            var result = _controller.GetInfo();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().NotBeNull();
            
            // ApplicationInfo.GetDetailedInfo() returns a complex object, so we just verify it's not null
            // and has the expected properties
            var appInfoType = okResult.Value.GetType();
            var nameProperty = appInfoType.GetProperty("Name");
            var versionProperty = appInfoType.GetProperty("Version");
            
            nameProperty.Should().NotBeNull();
            versionProperty.Should().NotBeNull();
        }
    }
}