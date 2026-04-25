// Persistence/Configurations/AudioConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AutoProcess.Domain.Entities;

namespace AutoProcess.Persistence.Configurations
{
    public class AudioConfiguration : IEntityTypeConfiguration<Audio>
    {
        public void Configure(EntityTypeBuilder<Audio> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .HasMaxLength(255);

            builder.Property(x => x.Path)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.Url)
                .HasMaxLength(500);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .IsRequired();

            // Soft delete filter
            builder.HasQueryFilter(x => x.DeletedAt == null);
        }
    }
}