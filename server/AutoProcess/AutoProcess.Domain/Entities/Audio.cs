using AutoProcess.Core.Common.Constants;
using AutoProcess.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AutoProcess.Domain.Entities
{
    [Table(nameof(Audio), Schema = DatabaseConstant.Schema.DBO)]
    public class Audio : IAuditedEntity<Guid>, ILink
    {
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Path { get; set; }
        public required string Url { get; set; }
    }
}
