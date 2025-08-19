using System;
using System.Threading.Tasks;
using ReadingTracker.Domain.Aggregates;
using ReadingTracker.Domain.Repositories;

namespace ReadingTracker.Domain.Services;

public interface IReadingGoalService
{
    Task<bool> IsReadingGoalMetAsync(Guid userId, int targetBooksPerYear);
    Task<int> CalculateReadingStreakAsync(Guid userId);
    Task<TimeSpan> GetAverageReadingTimePerDayAsync(Guid userId, int days = 30);
}

public class ReadingGoalService : IReadingGoalService
{
    private readonly IUserBookRepository _userBookRepository;

    public ReadingGoalService(IUserBookRepository userBookRepository)
    {
        _userBookRepository = userBookRepository ?? throw new ArgumentNullException(nameof(userBookRepository));
    }

    public async Task<bool> IsReadingGoalMetAsync(Guid userId, int targetBooksPerYear)
    {
        var currentYear = DateTime.UtcNow.Year;
        var startOfYear = new DateTime(currentYear, 1, 1);
        var endOfYear = new DateTime(currentYear, 12, 31, 23, 59, 59);

        var finishedThisYear = await _userBookRepository.FindByDateRangeAsync(userId, startOfYear, endOfYear);
        var booksFinishedCount = finishedThisYear.Count(book => 
            book.Status == Domain.ValueObjects.ReadingStatus.Finished && 
            book.FinishedDate.HasValue);

        return booksFinishedCount >= targetBooksPerYear;
    }

    public async Task<int> CalculateReadingStreakAsync(Guid userId)
    {
        var allBooks = await _userBookRepository.GetByUserIdAsync(userId);
        var finishedBooks = allBooks
            .Where(book => book.Status == Domain.ValueObjects.ReadingStatus.Finished && book.FinishedDate.HasValue)
            .OrderByDescending(book => book.FinishedDate!.Value)
            .ToList();

        if (!finishedBooks.Any())
            return 0;

        var streak = 0;
        var currentDate = DateTime.UtcNow.Date;

        // Start from today and go backwards
        foreach (var book in finishedBooks)
        {
            var finishedDate = book.FinishedDate!.Value.Date;
            
            // If there's a gap of more than 7 days, break the streak
            if ((currentDate - finishedDate).TotalDays > 7)
                break;

            streak++;
            currentDate = finishedDate;
        }

        return streak;
    }

    public async Task<TimeSpan> GetAverageReadingTimePerDayAsync(Guid userId, int days = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        var allBooks = await _userBookRepository.GetByUserIdAsync(userId);
        
        var recentSessions = allBooks
            .SelectMany(book => book.ReadingSessions)
            .Where(session => session.StartDate >= cutoffDate && session.Duration.HasValue)
            .ToList();

        if (!recentSessions.Any())
            return TimeSpan.Zero;

        var totalTime = recentSessions.Sum(session => session.Duration!.Value.Ticks);
        var averageTicks = totalTime / days; // Spread over the entire period, not just active days

        return TimeSpan.FromTicks(averageTicks);
    }
}
