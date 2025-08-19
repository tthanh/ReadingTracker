using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReadingTracker.Domain.Events;
using ReadingTracker.Infrastructure.Persistence;

namespace ReadingTracker.Infrastructure.Services;

public class DomainEventDispatcherService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DomainEventDispatcherService> _logger;

    public DomainEventDispatcherService(
        IServiceProvider serviceProvider,
        ILogger<DomainEventDispatcherService> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Domain Event Dispatcher Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessDomainEventsAsync();
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing domain events");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        _logger.LogInformation("Domain Event Dispatcher Service stopped");
    }

    private async Task ProcessDomainEventsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ReadingTrackerDbContext>();

        // In a real implementation, you would:
        // 1. Query for aggregates with pending domain events
        // 2. Process each event (send notifications, update read models, etc.)
        // 3. Clear the events after processing

        // For now, this is a placeholder for domain event processing
        await Task.CompletedTask;
    }
}

public interface IDomainEventHandler<in TEvent> where TEvent : DomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default);
}

// Example domain event handlers
public class BookAddedToLibraryEventHandler : IDomainEventHandler<BookAddedToLibraryEvent>
{
    private readonly ILogger<BookAddedToLibraryEventHandler> _logger;

    public BookAddedToLibraryEventHandler(ILogger<BookAddedToLibraryEventHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task HandleAsync(BookAddedToLibraryEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Book added to library: {BookTitle} by {Author} for user {UserId}",
            domainEvent.BookInfo.Title,
            domainEvent.BookInfo.Author,
            domainEvent.UserId);

        // Here you could:
        // - Send a notification to the user
        // - Update reading statistics
        // - Log analytics events
        // - Update recommendation engine

        await Task.CompletedTask;
    }
}

public class BookFinishedEventHandler : IDomainEventHandler<BookFinishedEvent>
{
    private readonly ILogger<BookFinishedEventHandler> _logger;

    public BookFinishedEventHandler(ILogger<BookFinishedEventHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task HandleAsync(BookFinishedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Book finished: {BookTitle} by {Author} for user {UserId}. Total pages: {TotalPages}, Reading time: {ReadingTime}",
            domainEvent.BookInfo.Title,
            domainEvent.BookInfo.Author,
            domainEvent.UserId,
            domainEvent.TotalPagesRead,
            domainEvent.TotalReadingTime);

        // Here you could:
        // - Send a congratulations notification
        // - Update reading goals
        // - Award achievements/badges
        // - Update reading statistics
        // - Suggest similar books

        await Task.CompletedTask;
    }
}

public class ReadingSessionLoggedEventHandler : IDomainEventHandler<ReadingSessionLoggedEvent>
{
    private readonly ILogger<ReadingSessionLoggedEventHandler> _logger;

    public ReadingSessionLoggedEventHandler(ILogger<ReadingSessionLoggedEventHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task HandleAsync(ReadingSessionLoggedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Reading session logged for user {UserId}: {PagesRead} pages read",
            domainEvent.UserId,
            domainEvent.PagesRead);

        // Here you could:
        // - Update daily/weekly reading streaks
        // - Check reading goals progress
        // - Send motivational notifications
        // - Update reading analytics

        await Task.CompletedTask;
    }
}
