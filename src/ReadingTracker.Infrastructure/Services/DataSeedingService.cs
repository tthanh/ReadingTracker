using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ReadingTracker.Domain.Aggregates;
using ReadingTracker.Domain.ValueObjects;
using ReadingTracker.Infrastructure.Persistence;

namespace ReadingTracker.Infrastructure.Services;

public class DataSeedingService
{
    private readonly ReadingTrackerDbContext _context;
    private readonly ILogger<DataSeedingService> _logger;

    public DataSeedingService(
        ReadingTrackerDbContext context,
        ILogger<DataSeedingService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SeedAsync()
    {
        try
        {
            _logger.LogInformation("Starting data seeding...");

            // Check if we already have data
            var existingCount = await _context.UserBooks.CountAsync();
            if (existingCount > 0)
            {
                _logger.LogInformation("Database already contains {Count} user books. Skipping seeding.", existingCount);
                return;
            }

            await SeedSampleDataAsync();
            
            _logger.LogInformation("Data seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding data");
            throw;
        }
    }

    private async Task SeedSampleDataAsync()
    {
        // Sample user ID for demo purposes
        var sampleUserId = Guid.Parse("12345678-1234-1234-1234-123456789012");

        var sampleBooks = new List<UserBook>
        {
            CreateSampleBook(
                sampleUserId,
                "978-0-544-00341-5",
                "The Hobbit",
                "J.R.R. Tolkien",
                "Houghton Mifflin Harcourt",
                1937,
                310,
                "Fantasy",
                "A reluctant hobbit, Bilbo Baggins, sets out to the Lonely Mountain with a spirited group of dwarves to reclaim their mountain home and the gold within it from the dragon Smaug."
            ),
            CreateSampleBook(
                sampleUserId,
                "978-0-7432-7356-5",
                "The Da Vinci Code",
                "Dan Brown",
                "Doubleday",
                2003,
                454,
                "Mystery/Thriller",
                "A murder in the Louvre Museum and clues in Da Vinci paintings lead to the discovery of a religious mystery protected by a secret society for two thousand years."
            ),
            CreateSampleBook(
                sampleUserId,
                "978-0-618-00222-1",
                "The Lord of the Rings: The Fellowship of the Ring",
                "J.R.R. Tolkien",
                "Houghton Mifflin",
                1954,
                423,
                "Fantasy",
                "Frodo Baggins finds himself faced with an immense task, as his elderly cousin Bilbo entrusts the Ring to his care."
            )
        };

        foreach (var book in sampleBooks)
        {
            _context.UserBooks.Add(book);
        }

        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Seeded {Count} sample books", sampleBooks.Count);
    }

    private static UserBook CreateSampleBook(
        Guid userId,
        string bookId,
        string title,
        string author,
        string publisher,
        int publicationYear,
        int totalPages,
        string genre,
        string description)
    {
        var bookInfo = new BookInfo(
            title: title,
            author: author,
            isbn: bookId,
            publisher: publisher,
            publicationYear: publicationYear,
            totalPages: totalPages,
            genre: genre,
            description: description
        );

        return new UserBook(bookId, userId, bookInfo);
    }
}
