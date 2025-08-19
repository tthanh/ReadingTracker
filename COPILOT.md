🤖 COPILOT.md: Personal Reading Tracker
This document outlines the plan, architecture, and development steps for creating a personal application to catalog books and track reading progress, following the principles of Domain-Driven Design (DDD).

🎯 1. Project Goal & Core Domain
The primary goal is to build an application that allows a user to manage their personal library, track their reading progress for each book, and view their reading history.

Our design will be guided by the Ubiquitous Language for this personal tracking domain.

Key Terms in our Ubiquitous Language:
Book: A specific publication identified by an ISBN or other unique key. This is a global concept.

UserBook: A Book as it exists in a user's personal library. This is the central concept, tracking the user's relationship with a Book.

ReadingStatus: The current state of a UserBook (e.g., ToRead, Reading, Finished, OnHold, Dropped).

Progress: The user's current position in a book, which can be a page number or percentage.

ReadingSession: An event representing a period of reading. It captures the date, duration, and progress made (e.g., "Read from page 50 to 75 on Tuesday").

Bookshelf: A user-defined collection for organizing UserBooks (e.g., "Favorites," "Sci-Fi," "2025 Reading Challenge").

🗺️ 2. Strategic Design: Bounded Contexts
For this application, we can start with a single, well-defined Bounded Context that covers the core functionality.

Core Bounded Context:
Tracking Context:

Responsibility: Manages a user's personal library, their reading status, progress, and history for each book.

Core Logic: Adding a book to the user's library, starting to read a book, logging a ReadingSession, updating Progress, changing ReadingStatus, and organizing books onto Bookshelves.

Future Contexts (Potential for Growth):
Social Context: Could manage sharing progress, book recommendations, and reading groups.

Analytics Context: Could generate stats and visualizations about a user's reading habits (pages per day, favorite genres, etc.).

For now, we will focus exclusively on the Tracking Context.

🛠️ 3. Tactical Design: Modeling the Tracking Context
Let's model the core of our application using tactical DDD patterns.

Aggregate: UserBook
The Aggregate is our consistency and transactional boundary. All operations related to a user's interaction with a single book will go through this aggregate.

Aggregate Root: UserBook (Entity)

UserBookId: Unique identifier for this specific instance in the user's library.

BookId: A reference to the global Book entity (could just be an ISBN string initially).

UserId: The owner of this library entry.

ReadingStatus: The current status (ToRead, Reading, etc.).

CurrentProgress: The user's last logged position (a Value Object, e.g., PageNumber).

ReadingSessions: A list of ReadingSession entities within the aggregate.

AddedDate: When the book was added to the user's library.

FinishedDate: When the book was finished (nullable).

Entities vs. Value Objects:
Entities:

UserBook: The aggregate root, with a distinct identity and lifecycle.

ReadingSession: Has an identity (SessionId) and records a specific event in time.

Value Objects (VOs):

Progress: Defined by its value (e.g., new Page(120)). It is immutable. Two Progress objects representing page 120 are identical.

BookInfo: A VO that could hold immutable details like title, author, and page count, fetched from an external service or a catalog.

Repositories:
We'll define an interface in the Domain layer to persist our UserBook aggregate.

// Example in C#
public interface IUserBookRepository
{
    Task<UserBook> GetByIdAsync(UserBookId id);
    Task<List<UserBook>> FindByStatusAsync(UserId userId, ReadingStatus status);
    Task AddAsync(UserBook userBook);
    Task UpdateAsync(UserBook userBook);
}

🏗️ 4. Architecture & Implementation Plan
We will use a Clean Architecture (or Hexagonal Architecture) to ensure our domain logic remains independent of frameworks, databases, and UIs.

Layers:
Domain Layer:

Content: UserBook aggregate, ReadingSession entity, Progress VO, Domain Events (e.g., BookFinishedEvent), and IUserBookRepository interface.

Rules: No dependencies on other layers.

Application Layer:

Content: Use Cases like LogReadingSessionCommand or AddBookToLibraryCommand. Orchestrates fetching the aggregate, calling domain methods, and saving changes.

Rules: Depends only on the Domain layer.

Infrastructure Layer:

Content: Implementation of IUserBookRepository using a database technology like Entity Framework Core. Could also include clients for external book data APIs (like Google Books).

Rules: Depends on the Application and Domain layers.

Presentation Layer:

Content: A REST API, a mobile app UI (React Native, Flutter), or a web front-end (React, Blazor).

Rules: Depends on the Application layer.

Recommended Directory Structure:
src/
├── ReadingTracker.Domain/
│   ├── Aggregates/
│   │   └── UserBook.cs
│   ├── Entities/
│   │   └── ReadingSession.cs
│   ├── ValueObjects/
│   │   └── Progress.cs
│   └── Repositories/
│       └── IUserBookRepository.cs
├── ReadingTracker.Application/
│   └── UseCases/
│       └── LogReadingSession/
│           ├── LogReadingSessionCommand.cs
│           └── LogReadingSessionCommandHandler.cs
├── ReadingTracker.Infrastructure/
│   └── Persistence/
│       ├── Repositories/
│       │   └── UserBookRepository.cs
│       └── AppDbContext.cs
└── ReadingTracker.Api/
    └── Controllers/
        └── BooksController.cs

🚀 5. Development Roadmap
Step 1: Project Setup.

Create the solution and projects based on the directory structure.

Install necessary dependencies (e.g., EF Core, MediatR).

Step 2: Model the Domain Layer.

Implement the UserBook aggregate root with its core logic (e.g., a method like LogProgress(newPage, date)).

Define the ReadingSession entity and Progress value object.

Define the IUserBookRepository interface.

Step 3: Implement Application Layer Use Cases.

Create the first command: AddBookToLibraryCommand.

Create a core command: LogReadingSessionCommand. The handler will load the UserBook, call the domain method, and use the repository to save it.

Step 4: Build the Infrastructure Layer.

Set up the EF Core DbContext with configurations for the UserBook aggregate.

Implement the UserBookRepository.

Step 5: Expose via Presentation Layer.

Create an API controller with endpoints for adding a book and logging progress.

POST /api/userbooks

POST /api/userbooks/{id}/sessions

Step 6: Testing.

Write unit tests for the UserBook aggregate's business logic.

Write integration tests to verify that the application use cases work correctly with an in-memory or test database.

---

🧑‍💻 6. Local Development Setup

To set up local development using Docker Compose:

1. Create a `docker-compose.yml` file in the project root.
2. Define services for the database, API, and any other dependencies.
3. Example minimal setup:

```yaml
version: '3.8'
services:
  db:
    image: postgres:14
    environment:
      POSTGRES_USER: user
      POSTGRES_PASSWORD: password
      POSTGRES_DB: reading_tracker
    ports:
      - "5432:5432"
  api:
    build: ./src/ReadingTracker.Api
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
    ports:
      - "5000:80"
    depends_on:
      - db
```

4. Run `docker-compose up --build` to start all services.
5. Update connection strings in your app to use the Docker service names (e.g., `Host=db`).
6. Add more services as needed for your stack.

---