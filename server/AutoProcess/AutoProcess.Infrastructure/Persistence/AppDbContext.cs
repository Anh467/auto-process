// Persistence/AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using AutoProcess.Domain.Entities;
using AutoProcess.Persistence.Interceptors;

namespace AutoProcess.Persistence
{
    public class AppDbContext : DbContext
    {
        private readonly AuditInterceptor _auditInterceptor;

        public AppDbContext(
            DbContextOptions<AppDbContext> options,
            AuditInterceptor auditInterceptor) : base(options)
        {
            _auditInterceptor = auditInterceptor;
        }

        public DbSet<JobStatus> JobStatuses { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<Audio> Audios { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.AddInterceptors(_auditInterceptor);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Tự động load tất cả IEntityTypeConfiguration trong assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}