namespace ReadingTracker.Infrastructure.Configuration;

public class GoogleBooksOptions
{
    public const string SectionName = "GoogleBooks";
    
    public string? ApiKey { get; set; }
    public string BaseUrl { get; set; } = "https://www.googleapis.com/books/v1/volumes";
    public int DefaultMaxResults { get; set; } = 10;
    public int MaxResultsLimit { get; set; } = 40;
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(30);
}

public class CacheOptions
{
    public const string SectionName = "Cache";
    
    public TimeSpan DefaultExpiration { get; set; } = TimeSpan.FromMinutes(15);
    public TimeSpan BookSearchExpiration { get; set; } = TimeSpan.FromHours(2);
    public TimeSpan BookByIsbnExpiration { get; set; } = TimeSpan.FromHours(24);
    public TimeSpan UserBooksExpiration { get; set; } = TimeSpan.FromMinutes(30);
    public TimeSpan SlidingExpiration { get; set; } = TimeSpan.FromMinutes(5);
}

public class DatabaseOptions
{
    public const string SectionName = "Database";
    
    public bool AutoMigrate { get; set; } = true;
    public bool SeedData { get; set; } = false;
    public int CommandTimeout { get; set; } = 30;
}
