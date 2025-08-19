using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ReadingTracker.Domain.Aggregates;
using ReadingTracker.Domain.ValueObjects;

namespace ReadingTracker.Domain.Repositories;

public interface IUserBookRepository
{
    // Basic CRUD operations
    Task<UserBook?> GetByIdAsync(Guid userBookId);
    Task<UserBook?> GetByUserAndBookAsync(Guid userId, string bookId);
    Task<IEnumerable<UserBook>> GetByUserIdAsync(Guid userId);
    Task AddAsync(UserBook userBook);
    Task UpdateAsync(UserBook userBook);
    Task DeleteAsync(Guid userBookId);

    // Query methods for specific use cases
    Task<IEnumerable<UserBook>> FindByStatusAsync(Guid userId, ReadingStatus status);
    Task<IEnumerable<UserBook>> FindCurrentlyReadingAsync(Guid userId);
    Task<IEnumerable<UserBook>> FindRecentlyFinishedAsync(Guid userId, int days = 30);
    Task<IEnumerable<UserBook>> FindByRatingAsync(Guid userId, int rating);
    
    // Search and filtering
    Task<IEnumerable<UserBook>> SearchAsync(Guid userId, string searchTerm);
    Task<IEnumerable<UserBook>> FindByAuthorAsync(Guid userId, string author);
    Task<IEnumerable<UserBook>> FindByDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate);
    
    // Statistics queries
    Task<int> GetTotalBooksCountAsync(Guid userId);
    Task<int> GetBooksCountByStatusAsync(Guid userId, ReadingStatus status);
    Task<bool> HasUserReadBookAsync(Guid userId, string bookId);
    
    // Pagination support
    Task<(IEnumerable<UserBook> Books, int TotalCount)> GetPagedAsync(
        Guid userId, 
        int pageNumber, 
        int pageSize,
        ReadingStatus? statusFilter = null,
        string? searchTerm = null);
}
