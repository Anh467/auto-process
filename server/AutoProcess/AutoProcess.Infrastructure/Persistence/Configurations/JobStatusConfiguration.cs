// Persistence/Configurations/JobStatusConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AutoProcess.Domain.Entities;

namespace AutoProcess.Persistence.Configurations
{
    public class JobStatusConfiguration : IEntityTypeConfiguration<JobStatus>
    {
        public void Configure(EntityTypeBuilder<JobStatus> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Code)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            // Seed data
            builder.HasData(
                new JobStatus { Id = 1, Code = "PENDING", Name = "Pending" },
                new JobStatus { Id = 2, Code = "PROCESSING", Name = "Processing" },
                new JobStatus { Id = 3, Code = "SUCCEEDED", Name = "Succeeded" },
                new JobStatus { Id = 4, Code = "FAILED", Name = "Failed" },
                new JobStatus { Id = 5, Code = "CANCELLED", Name = "Cancelled" }
            );
        }
    }
}