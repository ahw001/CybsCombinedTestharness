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
        // FK enforcement in SQLite is per-connection. Microsoft.Data.Sqlite already sets
        // PRAGMA foreign_keys=1 by default (verified empirically against 10.0.10 - raw
        // SQLite defaults it OFF, the provider does not), so this keyword pins that
        // behaviour explicitly rather than being what enables it. What actually gives the
        // constraints teeth is mssql_to_sqlite.py emitting FOREIGN KEY clauses into the
        // schema at all - before that they did not exist to enforce.
        connectionString ??= "Data Source=" + Path.Combine(AppContext.BaseDirectory, "Data", "CybsSampleDb.sqlite")
            + ";Foreign Keys=True";

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
