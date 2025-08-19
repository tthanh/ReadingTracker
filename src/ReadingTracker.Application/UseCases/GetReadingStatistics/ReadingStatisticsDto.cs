namespace ReadingTracker.Application.UseCases.GetReadingStatistics;

public record ReadingStatisticsDto(
    int TotalBooks,
    int BooksToRead,
    int BooksCurrentlyReading,
    int BooksFinished,
    int BooksOnHold,
    int BooksDropped,
    int TotalPagesRead,
    int TotalReadingSessions,
    TimeSpan TotalReadingTime,
    double AverageRating,
    int BooksWithRating,
    ReadingGoalProgressDto? YearlyGoal,
    IEnumerable<MonthlyReadingDto> MonthlyProgress
);

public record ReadingGoalProgressDto(
    int TargetBooks,
    int CompletedBooks,
    double ProgressPercentage,
    bool IsAchieved
);

public record MonthlyReadingDto(
    int Month,
    string MonthName,
    int BooksFinished,
    int PagesRead,
    TimeSpan ReadingTime
);
