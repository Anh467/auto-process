using AutoProcess.Domain.Entities;
using AutoProcess.Domain.Entities.Base;

public class Job : IAuditedEntity<Guid>
{
    public Guid Id { get; set; }
    public Guid TargetId { get; set; }
    public required string TargetType { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public int JobStatusId { get; set; }
    public required JobStatus JobStatus { get; set; }  
}