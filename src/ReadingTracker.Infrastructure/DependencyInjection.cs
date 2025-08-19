using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ReadingTracker.Domain.Repositories;
using ReadingTracker.Infrastructure.Configuration;
using ReadingTracker.Infrastructure.Persistence;
using ReadingTracker.Infrastructure.Persistence.Repositories;
using ReadingTracker.Infrastructure.Services;
using ReadingTracker.Infrastructure.ExternalServices;

namespace ReadingTracker.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Add configuration options
        services.Configure<GoogleBooksOptions>(configuration.GetSection(GoogleBooksOptions.SectionName));
        services.Configure<CacheOptions>(configuration.GetSection(CacheOptions.SectionName));
        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));
        // Add Entity Framework
        services.AddDbContext<ReadingTrackerDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            options.UseSqlServer(connectionString);
        });

        // Add caching
        services.AddMemoryCache();
        services.AddScoped<ICacheService, MemoryCacheService>();

        // Add repositories (with caching decorators)
        services.AddScoped<UserBookRepository>();
        services.AddScoped<IUserBookRepository>(provider =>
        {
            var repository = provider.GetRequiredService<UserBookRepository>();
            var cache = provider.GetRequiredService<ICacheService>();
            return new CachedUserBookRepository(repository, cache);
        });
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Add infrastructure services
        services.AddScoped<DataSeedingService>();

        // Add external services (with caching decorators)
        services.AddScoped<GoogleBooksService>();
        services.AddHttpClient<GoogleBooksService>();
        services.AddScoped<IBookSearchService>(provider =>
        {
            var bookSearchService = provider.GetRequiredService<GoogleBooksService>();
            var cache = provider.GetRequiredService<ICacheService>();
            return new CachedBookSearchService(bookSearchService, cache);
        });

        // Add domain event handlers
        services.AddScoped<IDomainEventHandler<Domain.Events.BookAddedToLibraryEvent>, BookAddedToLibraryEventHandler>();
        services.AddScoped<IDomainEventHandler<Domain.Events.BookFinishedEvent>, BookFinishedEventHandler>();
        services.AddScoped<IDomainEventHandler<Domain.Events.ReadingSessionLoggedEvent>, ReadingSessionLoggedEventHandler>();

        // Add background services - these will be registered in the API layer
        // services.AddHostedService<DomainEventDispatcherService>();
        // services.AddHostedService<DatabaseMigrationService>();

        return services;
    }
}
