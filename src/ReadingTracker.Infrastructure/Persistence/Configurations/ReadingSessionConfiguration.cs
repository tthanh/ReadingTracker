using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReadingTracker.Domain.Entities;

namespace ReadingTracker.Infrastructure.Persistence.Configurations;

public class ReadingSessionConfiguration : IEntityTypeConfiguration<ReadingSession>
{
    public void Configure(EntityTypeBuilder<ReadingSession> builder)
    {
        builder.ToTable("ReadingSessions");

        builder.HasKey(rs => rs.SessionId);

        builder.Property(rs => rs.SessionId)
            .ValueGeneratedNever();

        builder.Property(rs => rs.StartDate)
            .IsRequired()
            .HasColumnType("datetime2");

        builder.Property(rs => rs.EndDate)
            .HasColumnType("datetime2");

        builder.Property(rs => rs.Duration)
            .HasConversion(
                v => v.HasValue ? v.Value.Ticks : (long?)null,
                v => v.HasValue ? TimeSpan.FromTicks(v.Value) : null);

        builder.Property(rs => rs.StartPage)
            .IsRequired();

        builder.Property(rs => rs.EndPage)
            .IsRequired();

        builder.Property(rs => rs.Notes)
            .HasMaxLength(1000);

        builder.Property(rs => rs.CreatedAt)
            .IsRequired()
            .HasColumnType("datetime2");

        // Add a foreign key property for UserBook
        builder.Property<Guid>("UserBookId")
            .IsRequired();

        // Indexes for performance
        builder.HasIndex("UserBookId")
            .HasDatabaseName("IX_ReadingSessions_UserBookId");

        builder.HasIndex(rs => rs.StartDate)
            .HasDatabaseName("IX_ReadingSessions_StartDate");

        // Computed properties - ignore these as they are calculated
        builder.Ignore(rs => rs.PagesRead);
        builder.Ignore(rs => rs.IsCompleted);

        // Constraints using modern syntax
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_ReadingSessions_Pages", "[EndPage] >= [StartPage]");
            t.HasCheckConstraint("CK_ReadingSessions_StartPage_NonNegative", "[StartPage] >= 0");
        });
    }
}
