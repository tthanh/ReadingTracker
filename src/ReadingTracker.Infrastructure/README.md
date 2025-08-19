# ReadingTracker.Infrastructure

This layer contains the infrastructure concerns for the ReadingTracker application, implementing external dependencies and providing concrete implementations for domain repositories and services.

## Overview

The Infrastructure layer follows Clean Architecture principles and implements:

- **Data Persistence**: Entity Framework Core with SQL Server
- **External Services**: Google Books API integration for book search
- **Caching**: Memory-based caching with decorator pattern
- **Domain Events**: Background service for domain event processing
- **Configuration**: Strongly-typed configuration options

## Key Components

### Persistence

#### DbContext
- `ReadingTrackerDbContext`: Main Entity Framework DbContext
- Configured with proper relationships and constraints
- Supports database migrations

#### Entity Configurations
- `UserBookConfiguration`: EF Core configuration for UserBook aggregate
- `ReadingSessionConfiguration`: EF Core configuration for ReadingSession entity
- Proper mapping of value objects (BookInfo, Progress)
- Database indexes for performance

#### Repositories
- `UserBookRepository`: Implementation of IUserBookRepository
- `CachedUserBookRepository`: Decorator with caching functionality
- `UnitOfWork`: Implementation of Unit of Work pattern

### External Services

#### Book Search
- `IBookSearchService`: Interface for external book search
- `GoogleBooksService`: Google Books API integration
- `CachedBookSearchService`: Cached decorator for book search
- Proper error handling and resilience

### Infrastructure Services

#### Caching
- `ICacheService`: Abstraction for caching operations
- `MemoryCacheService`: In-memory cache implementation
- Cache key strategies and expiration policies

#### Background Services
- `DomainEventDispatcherService`: Processes domain events asynchronously
- `DatabaseMigrationService`: Handles automatic database migrations
- `DataSeedingService`: Seeds initial data for development

#### Domain Event Handlers
- `BookAddedToLibraryEventHandler`: Handles book addition events
- `BookFinishedEventHandler`: Handles book completion events
- `ReadingSessionLoggedEventHandler`: Handles reading session events

### Configuration

#### Options Classes
- `GoogleBooksOptions`: Configuration for Google Books API
- `CacheOptions`: Cache behavior configuration
- `DatabaseOptions`: Database-related settings

## Features

### Performance Optimizations
- **Caching Strategy**: Multi-level caching with appropriate expiration
- **Database Indexes**: Optimized for common query patterns
- **Lazy Loading**: Efficient data loading strategies

### Resilience
- **Error Handling**: Graceful degradation for external services
- **Retry Logic**: Built-in resilience for HTTP requests
- **Fallback Mechanisms**: Cached data when external services fail

### Observability
- **Structured Logging**: Comprehensive logging throughout
- **Performance Monitoring**: Cache hit/miss tracking
- **Error Tracking**: Detailed error logging and context

## Usage

### Dependency Injection

```csharp
// In Program.cs or Startup.cs
services.AddInfrastructure(configuration);
```

### Configuration

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ReadingTracker;Trusted_Connection=true;"
  },
  "GoogleBooks": {
    "ApiKey": "your-api-key-here",
    "BaseUrl": "https://www.googleapis.com/books/v1/volumes",
    "DefaultMaxResults": 10,
    "MaxResultsLimit": 40,
    "RequestTimeout": "00:00:30"
  },
  "Cache": {
    "DefaultExpiration": "00:15:00",
    "BookSearchExpiration": "02:00:00",
    "BookByIsbnExpiration": "24:00:00",
    "UserBooksExpiration": "00:30:00",
    "SlidingExpiration": "00:05:00"
  },
  "Database": {
    "AutoMigrate": true,
    "SeedData": false,
    "CommandTimeout": 30
  }
}
```

## Database Schema

The infrastructure layer creates the following tables:

### UserBooks
- Primary aggregate root table
- Contains book information as value objects
- Foreign key relationships to reading sessions

### ReadingSessions
- Reading session tracking
- Linked to UserBooks via foreign key
- Optimized for date-range queries

## Development

### Migrations

```bash
# Add new migration
dotnet ef migrations add MigrationName --project src/ReadingTracker.Infrastructure

# Update database
dotnet ef database update --project src/ReadingTracker.Infrastructure
```

### Testing External Services

The Google Books service can be tested independently:

```csharp
var service = serviceProvider.GetRequiredService<IBookSearchService>();
var books = await service.SearchBooksAsync("The Hobbit");
```

## Dependencies

- Microsoft.EntityFrameworkCore
- Microsoft.EntityFrameworkCore.SqlServer
- Microsoft.EntityFrameworkCore.Tools
- Microsoft.Extensions.Hosting.Abstractions
- Microsoft.Extensions.Http
- System.Text.Json

## Architecture Patterns

- **Repository Pattern**: Data access abstraction
- **Unit of Work**: Transaction management
- **Decorator Pattern**: Caching and cross-cutting concerns
- **Options Pattern**: Configuration management
- **Background Services**: Asynchronous processing
