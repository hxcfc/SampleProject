using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SampleProject.Persistence.Data;
using Xunit;

namespace SampleProject.Test
{
    /// <summary>
    /// Base class for integration tests
    /// </summary>
    public abstract class TestBase : IClassFixture<WebApplicationFactory<Program>>, IDisposable
    {
        protected readonly HttpClient Client;
        protected readonly ApplicationDbContext Context;
        protected readonly WebApplicationFactory<Program> Factory;

        protected TestBase(WebApplicationFactory<Program> factory)
        {
            Factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the real database
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    // Add in-memory database
                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}");
                    });
                });
            });

            Client = Factory.CreateClient();
            Context = Factory.Services.GetRequiredService<ApplicationDbContext>();
        }

        public void Dispose()
        {
            Context.Database.EnsureDeleted();
            Context.Dispose();
            Client.Dispose();
        }
    }
}