using Microsoft.Extensions.DependencyInjection;
using ReadingTracker.Application.Common.Behaviors;
using ReadingTracker.Domain.Services;

namespace ReadingTracker.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register MediatR
        services.AddMediatR(cfg => 
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            
            // Register behaviors
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(PerformanceBehavior<,>));
        });

        // Register domain services
        services.AddScoped<ReadingGoalService>();

        return services;
    }
}
