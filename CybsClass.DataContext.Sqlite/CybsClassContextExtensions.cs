using Microsoft.EntityFrameworkCore; // To use the UseSqlite method.
using Microsoft.Extensions.DependencyInjection; // To use IServiceCollection.

namespace CybsClass.EntityModels;

public static class CybsClassContextExtensions
{
    /// <summary>
    /// Adds CybsDbContext to the specified IServiceCollection. Uses the SQLite database provider.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">Set to override the default.</param>
    /// <returns>An IServiceCollection that can be used to add more services.</returns>
    public static IServiceCollection AddCybsDbContext(
      this IServiceCollection services, // The type to extend.
      string? connectionString = null)
    {
        connectionString ??= "Data Source=" + Path.Combine(AppContext.BaseDirectory, "Data", "CybsSampleDb.sqlite");

        services.AddDbContext<CybsDbContext>(options =>
        {
            options.UseSqlite(connectionString);
        },

        // Register with a transient lifetime to avoid concurrency
        // issues with Blazor Server projects.
        contextLifetime: ServiceLifetime.Transient,
        optionsLifetime: ServiceLifetime.Transient);

        return services;
    }
}
