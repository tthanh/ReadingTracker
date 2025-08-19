using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReadingTracker.Domain.Aggregates;
using ReadingTracker.Domain.ValueObjects;

namespace ReadingTracker.Infrastructure.Persistence.Configurations;

public class UserBookConfiguration : IEntityTypeConfiguration<UserBook>
{
    public void Configure(EntityTypeBuilder<UserBook> builder)
    {
        builder.ToTable("UserBooks");

        builder.HasKey(ub => ub.UserBookId);

        builder.Property(ub => ub.UserBookId)
            .ValueGeneratedNever();

        builder.Property(ub => ub.BookId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(ub => ub.UserId)
            .IsRequired();

        // Configure BookInfo value object
        builder.OwnsOne(ub => ub.BookInfo, bookInfo =>
        {
            bookInfo.Property(bi => bi.Title)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnName("Title");

            bookInfo.Property(bi => bi.Author)
                .IsRequired()
                .HasMaxLength(300)
                .HasColumnName("Author");

            bookInfo.Property(bi => bi.ISBN)
                .HasMaxLength(20)
                .HasColumnName("ISBN");

            bookInfo.Property(bi => bi.Publisher)
                .HasMaxLength(200)
                .HasColumnName("Publisher");

            bookInfo.Property(bi => bi.PublicationYear)
                .HasColumnName("PublicationYear");

            bookInfo.Property(bi => bi.TotalPages)
                .HasColumnName("TotalPages");

            bookInfo.Property(bi => bi.Genre)
                .HasMaxLength(100)
                .HasColumnName("Genre");

            bookInfo.Property(bi => bi.Description)
                .HasMaxLength(2000)
                .HasColumnName("Description");

            bookInfo.Property(bi => bi.CoverImageUrl)
                .HasMaxLength(500)
                .HasColumnName("CoverImageUrl");
        });

        // Configure Progress value object
        builder.OwnsOne(ub => ub.CurrentProgress, progress =>
        {
            progress.Property(p => p.PageNumber)
                .IsRequired()
                .HasColumnName("CurrentPage");

            progress.Property(p => p.TotalPages)
                .HasColumnName("TotalPagesInProgress");

            progress.Property(p => p.PercentageComplete)
                .HasColumnName("PercentageComplete")
                .HasPrecision(5, 2);

            progress.Property(p => p.IsComplete)
                .HasColumnName("IsComplete");
        });

        // Configure ReadingStatus enum
        builder.Property(ub => ub.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(ub => ub.AddedDate)
            .IsRequired()
            .HasColumnType("datetime2");

        builder.Property(ub => ub.StartedDate)
            .HasColumnType("datetime2");

        builder.Property(ub => ub.FinishedDate)
            .HasColumnType("datetime2");

        builder.Property(ub => ub.PersonalNotes)
            .HasMaxLength(2000);

        builder.Property(ub => ub.PersonalRating)
            .HasCheckConstraint("CK_UserBooks_PersonalRating", "[PersonalRating] >= 1 AND [PersonalRating] <= 5");

        // Configure relationships
        builder.HasMany(ub => ub.ReadingSessions)
            .WithOne()
            .HasForeignKey("UserBookId")
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes for performance
        builder.HasIndex(ub => ub.UserId)
            .HasDatabaseName("IX_UserBooks_UserId");

        builder.HasIndex(ub => new { ub.UserId, ub.BookId })
            .IsUnique()
            .HasDatabaseName("IX_UserBooks_UserId_BookId");

        builder.HasIndex(ub => new { ub.UserId, ub.Status })
            .HasDatabaseName("IX_UserBooks_UserId_Status");

        builder.HasIndex(ub => ub.AddedDate)
            .HasDatabaseName("IX_UserBooks_AddedDate");

        builder.HasIndex(ub => ub.FinishedDate)
            .HasDatabaseName("IX_UserBooks_FinishedDate");

        // Ignore domain events - they should not be persisted
        builder.Ignore(ub => ub.DomainEvents);

        // Computed properties
        builder.Ignore(ub => ub.TotalPagesRead);
        builder.Ignore(ub => ub.TotalReadingTime);
        builder.Ignore(ub => ub.TotalReadingSessions);
        builder.Ignore(ub => ub.LastReadingSessionDate);
        builder.Ignore(ub => ub.TimeSinceLastSession);
    }
}
