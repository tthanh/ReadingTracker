using System.Globalization;
using ReadingTracker.Application.Common;
using ReadingTracker.Domain.Repositories;
using ReadingTracker.Domain.Services;
using ReadingTracker.Domain.ValueObjects;

namespace ReadingTracker.Application.UseCases.GetReadingStatistics;

public class GetReadingStatisticsQueryHandler : IQueryHandler<GetReadingStatisticsQuery, ReadingStatisticsDto>
{
    private readonly IUserBookRepository _userBookRepository;
    private readonly ReadingGoalService _readingGoalService;

    public GetReadingStatisticsQueryHandler(
        IUserBookRepository userBookRepository,
        ReadingGoalService readingGoalService)
    {
        _userBookRepository = userBookRepository;
        _readingGoalService = readingGoalService;
    }

    public async Task<ReadingStatisticsDto> Handle(GetReadingStatisticsQuery request, CancellationToken cancellationToken)
    {
        var year = request.Year ?? DateTime.UtcNow.Year;
        var yearStart = new DateTime(year, 1, 1);
        var yearEnd = new DateTime(year, 12, 31, 23, 59, 59);

        // Get all user books
        var allBooks = await _userBookRepository.GetByUserIdAsync(request.UserId);
        var booksInYear = allBooks.Where(b => 
            b.AddedDate >= yearStart && b.AddedDate <= yearEnd ||
            (b.FinishedDate.HasValue && b.FinishedDate >= yearStart && b.FinishedDate <= yearEnd));

        // Basic counts by status
        var totalBooks = allBooks.Count();
        var booksToRead = await _userBookRepository.GetBooksCountByStatusAsync(request.UserId, ReadingStatus.ToRead);
        var booksReading = await _userBookRepository.GetBooksCountByStatusAsync(request.UserId, ReadingStatus.Reading);
        var booksFinished = await _userBookRepository.GetBooksCountByStatusAsync(request.UserId, ReadingStatus.Finished);
        var booksOnHold = await _userBookRepository.GetBooksCountByStatusAsync(request.UserId, ReadingStatus.OnHold);
        var booksDropped = await _userBookRepository.GetBooksCountByStatusAsync(request.UserId, ReadingStatus.Dropped);

        // Reading statistics
        var totalPagesRead = allBooks.Sum(b => b.TotalPagesRead);
        var totalSessions = allBooks.Sum(b => b.TotalReadingSessions);
        var totalReadingTime = TimeSpan.FromTicks(allBooks.Sum(b => b.TotalReadingTime.Ticks));

        // Rating statistics
        var booksWithRating = allBooks.Where(b => b.PersonalRating.HasValue);
        var averageRating = booksWithRating.Any() 
            ? booksWithRating.Average(b => b.PersonalRating!.Value) 
            : 0.0;
        var ratedBooksCount = booksWithRating.Count();

        // Yearly goal progress (assuming a default goal of 12 books per year)
        var yearlyGoal = await CalculateYearlyGoalProgress(request.UserId, year);

        // Monthly progress
        var monthlyProgress = await CalculateMonthlyProgress(request.UserId, year);

        return new ReadingStatisticsDto(
            totalBooks,
            booksToRead,
            booksReading,
            booksFinished,
            booksOnHold,
            booksDropped,
            totalPagesRead,
            totalSessions,
            totalReadingTime,
            averageRating,
            ratedBooksCount,
            yearlyGoal,
            monthlyProgress
        );
    }

    private async Task<ReadingGoalProgressDto?> CalculateYearlyGoalProgress(Guid userId, int year)
    {
        // For now, assume a default goal of 12 books per year
        // In a real implementation, this would come from user preferences
        const int defaultYearlyGoal = 12;
        
        var finishedBooks = await _userBookRepository.FindByDateRangeAsync(
            userId, 
            new DateTime(year, 1, 1), 
            new DateTime(year, 12, 31, 23, 59, 59));
        
        var completedCount = finishedBooks.Count(b => b.Status == ReadingStatus.Finished);
        var progressPercentage = Math.Min(100.0, (double)completedCount / defaultYearlyGoal * 100.0);
        var isAchieved = completedCount >= defaultYearlyGoal;

        return new ReadingGoalProgressDto(
            defaultYearlyGoal,
            completedCount,
            progressPercentage,
            isAchieved
        );
    }

    private async Task<IEnumerable<MonthlyReadingDto>> CalculateMonthlyProgress(Guid userId, int year)
    {
        var monthlyData = new List<MonthlyReadingDto>();

        for (int month = 1; month <= 12; month++)
        {
            var monthStart = new DateTime(year, month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var monthlyBooks = await _userBookRepository.FindByDateRangeAsync(userId, monthStart, monthEnd);
            var finishedInMonth = monthlyBooks.Where(b => 
                b.Status == ReadingStatus.Finished && 
                b.FinishedDate.HasValue && 
                b.FinishedDate >= monthStart && 
                b.FinishedDate <= monthEnd);

            var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);
            var booksFinished = finishedInMonth.Count();
            var pagesRead = finishedInMonth.Sum(b => b.TotalPagesRead);
            var readingTime = TimeSpan.FromTicks(finishedInMonth.Sum(b => b.TotalReadingTime.Ticks));

            monthlyData.Add(new MonthlyReadingDto(
                month,
                monthName,
                booksFinished,
                pagesRead,
                readingTime
            ));
        }

        return monthlyData;
    }
}
