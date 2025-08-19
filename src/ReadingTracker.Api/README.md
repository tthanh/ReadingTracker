# Reading Tracker API

A comprehensive RESTful API for managing personal reading progress and book libraries.

## Overview

The Reading Tracker API provides endpoints to:
- Manage personal book libraries
- Track reading progress and sessions
- Search for books from external sources
- Generate reading statistics and analytics
- Rate and review books

## Base URL

```
https://localhost:7000/api
```

## API Documentation

Interactive API documentation is available at:
- **Swagger UI**: `https://localhost:7000/` (Development)
- **OpenAPI Spec**: `https://localhost:7000/swagger/v1/swagger.json`

## Authentication

Currently, the API uses a mock user system for demonstration purposes. In a production environment, this would be replaced with proper JWT authentication.

## Endpoints

### UserBooks Controller
Manages the user's personal book library.

#### Get All Books
```http
GET /api/userbooks
```

#### Get Specific Book
```http
GET /api/userbooks/{userBookId}
```

#### Add Book to Library
```http
POST /api/userbooks
Content-Type: application/json

{
  "bookId": "978-0544003415",
  "bookInfo": {
    "title": "The Hobbit",
    "author": "J.R.R. Tolkien",
    "isbn": "978-0544003415",
    "publisher": "Houghton Mifflin Harcourt",
    "publicationYear": 1937,
    "totalPages": 310,
    "genre": "Fantasy",
    "description": "A reluctant hobbit goes on an adventure...",
    "coverImageUrl": "https://example.com/cover.jpg"
  },
  "personalNotes": "Recommended by a friend"
}
```

#### Update Book Status
```http
PUT /api/userbooks/{userBookId}/status
Content-Type: application/json

{
  "status": "reading",
  "date": "2024-01-15T10:00:00Z"
}
```

Valid statuses: `toread`, `reading`, `finished`, `onhold`, `dropped`

#### Log Reading Session
```http
POST /api/userbooks/{userBookId}/sessions
Content-Type: application/json

{
  "sessionDate": "2024-01-15T19:00:00Z",
  "startPage": 1,
  "endPage": 25,
  "endTime": "2024-01-15T20:30:00Z",
  "sessionNotes": "Great opening chapter"
}
```

#### Update Reading Progress
```http
PUT /api/userbooks/{userBookId}/progress
Content-Type: application/json

{
  "pageNumber": 150
}
```

#### Update Personal Notes
```http
PUT /api/userbooks/{userBookId}/notes
Content-Type: application/json

{
  "personalNotes": "Updated thoughts on the book..."
}
```

#### Rate Book
```http
PUT /api/userbooks/{userBookId}/rating
Content-Type: application/json

{
  "rating": 5
}
```

#### Remove Rating
```http
DELETE /api/userbooks/{userBookId}/rating
```

#### Delete Book
```http
DELETE /api/userbooks/{userBookId}
```

#### Get Books by Status
```http
GET /api/userbooks/by-status/{status}
```

#### Search User Books
```http
GET /api/userbooks/search?query=tolkien
```

### BookSearch Controller
Search for books from external sources (Google Books API).

#### Search Books
```http
GET /api/booksearch/search?query=the hobbit&maxResults=10
```

#### Search by ISBN
```http
GET /api/booksearch/isbn/9780544003415
```

#### Detailed Search
```http
POST /api/booksearch/search
Content-Type: application/json

{
  "query": "fantasy books tolkien",
  "maxResults": 20
}
```

### Statistics Controller
Provides reading statistics and analytics.

#### Get Reading Statistics
```http
GET /api/statistics
```

Returns comprehensive reading statistics including:
- Total books and counts by status
- Total pages read and reading time
- Average rating
- Recently finished books
- Currently reading books

#### Get Books by Status
```http
GET /api/statistics/by-status/{status}
```

#### Get Recently Finished Books
```http
GET /api/statistics/recently-finished?days=30
```

#### Get Books by Rating
```http
GET /api/statistics/by-rating/{rating}
```

#### Get Books by Author
```http
GET /api/statistics/by-author?author=tolkien
```

#### Get Statistics Counts
```http
GET /api/statistics/counts
```

## Response Format

All API responses follow a consistent format:

### Success Response
```json
{
  "success": true,
  "data": { ... },
  "message": "Operation completed successfully",
  "errors": [],
  "timestamp": "2024-01-15T10:00:00Z"
}
```

### Error Response
```json
{
  "success": false,
  "data": null,
  "message": null,
  "errors": ["Error message here"],
  "timestamp": "2024-01-15T10:00:00Z"
}
```

### Validation Error Response
```json
{
  "title": "Validation failed",
  "status": 400,
  "errors": [
    {
      "field": "Rating",
      "message": "Rating must be between 1 and 5"
    }
  ],
  "timestamp": "2024-01-15T10:00:00Z"
}
```

## Status Codes

- `200 OK` - Request successful
- `201 Created` - Resource created successfully
- `400 Bad Request` - Invalid request data
- `404 Not Found` - Resource not found
- `409 Conflict` - Resource already exists
- `500 Internal Server Error` - Server error

## Data Models

### BookInfo
```json
{
  "title": "string",
  "author": "string",
  "isbn": "string",
  "publisher": "string",
  "publicationYear": 2024,
  "totalPages": 300,
  "genre": "string",
  "description": "string",
  "coverImageUrl": "string"
}
```

### UserBook
```json
{
  "userBookId": "guid",
  "bookId": "string",
  "userId": "guid",
  "bookInfo": { ... },
  "status": "string",
  "currentProgress": {
    "pageNumber": 150,
    "totalPages": 300,
    "percentageComplete": 50.0,
    "isComplete": false
  },
  "addedDate": "2024-01-15T10:00:00Z",
  "startedDate": "2024-01-15T10:00:00Z",
  "finishedDate": null,
  "personalNotes": "string",
  "personalRating": 5,
  "readingSessions": [...],
  "totalPagesRead": 150,
  "totalReadingTime": "02:30:00",
  "totalReadingSessions": 5,
  "lastReadingSessionDate": "2024-01-15T19:00:00Z"
}
```

## Error Handling

The API includes comprehensive error handling:
- Global exception middleware catches unhandled exceptions
- Domain-specific exceptions are mapped to appropriate HTTP status codes
- Validation errors provide detailed field-level feedback
- Request logging includes request IDs for debugging

## Rate Limiting

Currently, no rate limiting is implemented. In production, consider implementing rate limiting based on your usage requirements.

## CORS

CORS is configured to allow all origins in development. Configure appropriately for production environments.

## Health Check

A health check endpoint is available:

```http
GET /health
```

Returns:
```json
{
  "status": "Healthy",
  "timestamp": "2024-01-15T10:00:00Z",
  "environment": "Development"
}
```

## Development

### Running the API
```bash
cd src/ReadingTracker.Api
dotnet run
```

### Building the API
```bash
dotnet build
```

### Running Tests
```bash
dotnet test
```

## Configuration

Key configuration settings in `appsettings.json`:

- **ConnectionStrings**: Database connection
- **GoogleBooks**: External API configuration
- **Cache**: Caching behavior settings
- **Database**: Migration and seeding options
