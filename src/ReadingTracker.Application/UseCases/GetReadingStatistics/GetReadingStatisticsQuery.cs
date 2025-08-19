using ReadingTracker.Application.Common;

namespace ReadingTracker.Application.UseCases.GetReadingStatistics;

public record GetReadingStatisticsQuery(
    Guid UserId,
    int? Year = null
) : IQuery<ReadingStatisticsDto>;
