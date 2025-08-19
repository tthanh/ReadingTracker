using Microsoft.EntityFrameworkCore;
using ReadingTracker.Domain.Aggregates;
using ReadingTracker.Domain.Entities;
using ReadingTracker.Infrastructure.Persistence.Configurations;

namespace ReadingTracker.Infrastructure.Persistence;

public class ReadingTrackerDbContext : DbContext
{
    public ReadingTrackerDbContext(DbContextOptions<ReadingTrackerDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserBook> UserBooks { get; set; } = null!;
    public DbSet<ReadingSession> ReadingSessions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserBookConfiguration());
        modelBuilder.ApplyConfiguration(new ReadingSessionConfiguration());

        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer();
        }

        base.OnConfiguring(optionsBuilder);
    }
}
