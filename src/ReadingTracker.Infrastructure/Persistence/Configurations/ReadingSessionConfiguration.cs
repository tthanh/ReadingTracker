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

        builder.Property(rs => rs.EndTime)
            .HasColumnType("datetime2");

        builder.Property(rs => rs.StartPage)
            .IsRequired();

        builder.Property(rs => rs.EndPage)
            .IsRequired();

        builder.Property(rs => rs.SessionNotes)
            .HasMaxLength(1000);

        // Add a foreign key property for UserBook
        builder.Property<Guid>("UserBookId")
            .IsRequired();

        // Indexes for performance
        builder.HasIndex("UserBookId")
            .HasDatabaseName("IX_ReadingSessions_UserBookId");

        builder.HasIndex(rs => rs.StartDate)
            .HasDatabaseName("IX_ReadingSessions_StartDate");

        // Computed properties
        builder.Ignore(rs => rs.PagesRead);
        builder.Ignore(rs => rs.Duration);

        // Constraints
        builder.HasCheckConstraint("CK_ReadingSessions_Pages", "[EndPage] >= [StartPage]");
    }
}
