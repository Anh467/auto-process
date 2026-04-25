// Persistence/Interceptors/AuditInterceptor.cs
using AutoProcess.Domain.Entities.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AutoProcess.Persistence.Interceptors
{
    public class AuditInterceptor : SaveChangesInterceptor
    {
        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            ApplyAudit(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            ApplyAudit(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private static void ApplyAudit(DbContext? context)
        {
            if (context is null) return;

            var now = DateTimeOffset.UtcNow;

            foreach (var entry in context.ChangeTracker.Entries())
            {
                if (entry.Entity is IAudited auditedEntity)
                {
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditedEntity.CreatedAt = now;
                            auditedEntity.UpdatedAt = now;
                            break;

                        case EntityState.Modified:
                            auditedEntity.UpdatedAt = now;
                            break;

                        case EntityState.Deleted:
                            // Soft delete
                            entry.State = EntityState.Modified;
                            auditedEntity.DeletedAt = now;
                            break;
                    }
                }
            }
        }
    }
}