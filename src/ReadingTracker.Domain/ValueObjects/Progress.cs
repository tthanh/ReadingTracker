using System;

namespace ReadingTracker.Domain.ValueObjects;

public record Progress
{
    public int PageNumber { get; }
    public int? TotalPages { get; }
    public decimal? Percentage { get; }

    private Progress(int pageNumber, int? totalPages = null)
    {
        if (pageNumber < 0)
            throw new ArgumentException("Page number cannot be negative", nameof(pageNumber));
        
        if (totalPages.HasValue && totalPages <= 0)
            throw new ArgumentException("Total pages must be positive", nameof(totalPages));
        
        if (totalPages.HasValue && pageNumber > totalPages)
            throw new ArgumentException("Page number cannot exceed total pages", nameof(pageNumber));

        PageNumber = pageNumber;
        TotalPages = totalPages;
        Percentage = totalPages.HasValue && totalPages > 0 
            ? Math.Round((decimal)pageNumber / totalPages.Value * 100, 2) 
            : null;
    }

    public static Progress FromPage(int pageNumber) => new(pageNumber);
    
    public static Progress FromPage(int pageNumber, int totalPages) => new(pageNumber, totalPages);

    public static Progress FromPercentage(decimal percentage, int totalPages)
    {
        if (percentage < 0 || percentage > 100)
            throw new ArgumentException("Percentage must be between 0 and 100", nameof(percentage));
        
        if (totalPages <= 0)
            throw new ArgumentException("Total pages must be positive", nameof(totalPages));

        var pageNumber = (int)Math.Round(percentage / 100 * totalPages);
        return new Progress(pageNumber, totalPages);
    }

    public bool IsComplete => TotalPages.HasValue && PageNumber >= TotalPages.Value;

    public override string ToString()
    {
        if (TotalPages.HasValue)
            return $"Page {PageNumber} of {TotalPages} ({Percentage:F1}%)";
        
        return $"Page {PageNumber}";
    }
}
