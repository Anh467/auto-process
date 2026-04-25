// Persistence/Configurations/JobConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AutoProcess.Domain.Entities;

namespace AutoProcess.Persistence.Configurations
{
    public class JobConfiguration : IEntityTypeConfiguration<Job>
    {
        public void Configure(EntityTypeBuilder<Job> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.TargetId)
                .IsRequired();

            builder.Property(x => x.TargetType)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .IsRequired();

            // Soft delete filter
            builder.HasQueryFilter(x => x.DeletedAt == null);

            // Relationship: Job -> JobStatus (many-to-one)
            builder.HasOne(x => x.JobStatus)
                .WithMany(x => x.Jobs)
                .HasForeignKey(x => x.JobStatusId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}