using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ReadingTracker.Domain.Aggregates;
using ReadingTracker.Domain.Repositories;
using ReadingTracker.Domain.ValueObjects;

namespace ReadingTracker.Infrastructure.Persistence.Repositories;

public class UserBookRepository : IUserBookRepository
{
    private readonly ReadingTrackerDbContext _context;

    public UserBookRepository(ReadingTrackerDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<UserBook?> GetByIdAsync(Guid userBookId)
    {
        return await _context.UserBooks
            .Include(ub => ub.ReadingSessions)
            .FirstOrDefaultAsync(ub => ub.UserBookId == userBookId);
    }

    public async Task<UserBook?> GetByUserAndBookAsync(Guid userId, string bookId)
    {
        return await _context.UserBooks
            .Include(ub => ub.ReadingSessions)
            .FirstOrDefaultAsync(ub => ub.UserId == userId && ub.BookId == bookId);
    }

    public async Task<IEnumerable<UserBook>> GetByUserIdAsync(Guid userId)
    {
        return await _context.UserBooks
            .Include(ub => ub.ReadingSessions)
            .Where(ub => ub.UserId == userId)
            .OrderByDescending(ub => ub.AddedDate)
            .ToListAsync();
    }

    public async Task AddAsync(UserBook userBook)
    {
        if (userBook == null)
            throw new ArgumentNullException(nameof(userBook));

        _context.UserBooks.Add(userBook);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(UserBook userBook)
    {
        if (userBook == null)
            throw new ArgumentNullException(nameof(userBook));

        _context.UserBooks.Update(userBook);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid userBookId)
    {
        var userBook = await _context.UserBooks
            .FirstOrDefaultAsync(ub => ub.UserBookId == userBookId);
        
        if (userBook != null)
        {
            _context.UserBooks.Remove(userBook);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<UserBook>> FindByStatusAsync(Guid userId, ReadingStatus status)
    {
        return await _context.UserBooks
            .Include(ub => ub.ReadingSessions)
            .Where(ub => ub.UserId == userId && ub.Status == status)
            .OrderByDescending(ub => ub.AddedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserBook>> FindCurrentlyReadingAsync(Guid userId)
    {
        return await FindByStatusAsync(userId, ReadingStatus.Reading);
    }

    public async Task<IEnumerable<UserBook>> FindRecentlyFinishedAsync(Guid userId, int days = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        
        return await _context.UserBooks
            .Include(ub => ub.ReadingSessions)
            .Where(ub => ub.UserId == userId 
                && ub.Status == ReadingStatus.Finished 
                && ub.FinishedDate >= cutoffDate)
            .OrderByDescending(ub => ub.FinishedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserBook>> FindByRatingAsync(Guid userId, int rating)
    {
        return await _context.UserBooks
            .Include(ub => ub.ReadingSessions)
            .Where(ub => ub.UserId == userId && ub.PersonalRating == rating)
            .OrderByDescending(ub => ub.AddedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserBook>> SearchAsync(Guid userId, string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetByUserIdAsync(userId);

        var normalizedSearchTerm = searchTerm.Trim().ToLower();

        return await _context.UserBooks
            .Include(ub => ub.ReadingSessions)
            .Where(ub => ub.UserId == userId && (
                ub.BookInfo.Title.ToLower().Contains(normalizedSearchTerm) ||
                ub.BookInfo.Author.ToLower().Contains(normalizedSearchTerm) ||
                (ub.BookInfo.Genre != null && ub.BookInfo.Genre.ToLower().Contains(normalizedSearchTerm)) ||
                (ub.PersonalNotes != null && ub.PersonalNotes.ToLower().Contains(normalizedSearchTerm))
            ))
            .OrderByDescending(ub => ub.AddedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserBook>> FindByAuthorAsync(Guid userId, string author)
    {
        if (string.IsNullOrWhiteSpace(author))
            return new List<UserBook>();

        var normalizedAuthor = author.Trim().ToLower();

        return await _context.UserBooks
            .Include(ub => ub.ReadingSessions)
            .Where(ub => ub.UserId == userId && ub.BookInfo.Author.ToLower().Contains(normalizedAuthor))
            .OrderByDescending(ub => ub.AddedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserBook>> FindByDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate)
    {
        return await _context.UserBooks
            .Include(ub => ub.ReadingSessions)
            .Where(ub => ub.UserId == userId && ub.AddedDate >= startDate && ub.AddedDate <= endDate)
            .OrderByDescending(ub => ub.AddedDate)
            .ToListAsync();
    }

    public async Task<int> GetTotalBooksCountAsync(Guid userId)
    {
        return await _context.UserBooks
            .CountAsync(ub => ub.UserId == userId);
    }

    public async Task<int> GetBooksCountByStatusAsync(Guid userId, ReadingStatus status)
    {
        return await _context.UserBooks
            .CountAsync(ub => ub.UserId == userId && ub.Status == status);
    }

    public async Task<bool> HasUserReadBookAsync(Guid userId, string bookId)
    {
        return await _context.UserBooks
            .AnyAsync(ub => ub.UserId == userId && ub.BookId == bookId);
    }

    public async Task<(IEnumerable<UserBook> Books, int TotalCount)> GetPagedAsync(
        Guid userId, 
        int pageNumber, 
        int pageSize,
        ReadingStatus? statusFilter = null,
        string? searchTerm = null)
    {
        var query = _context.UserBooks
            .Include(ub => ub.ReadingSessions)
            .Where(ub => ub.UserId == userId);

        // Apply status filter
        if (statusFilter.HasValue)
        {
            query = query.Where(ub => ub.Status == statusFilter.Value);
        }

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var normalizedSearchTerm = searchTerm.Trim().ToLower();
            query = query.Where(ub => 
                ub.BookInfo.Title.ToLower().Contains(normalizedSearchTerm) ||
                ub.BookInfo.Author.ToLower().Contains(normalizedSearchTerm) ||
                (ub.BookInfo.Genre != null && ub.BookInfo.Genre.ToLower().Contains(normalizedSearchTerm)) ||
                (ub.PersonalNotes != null && ub.PersonalNotes.ToLower().Contains(normalizedSearchTerm))
            );
        }

        var totalCount = await query.CountAsync();

        var books = await query
            .OrderByDescending(ub => ub.AddedDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (books, totalCount);
    }
}
