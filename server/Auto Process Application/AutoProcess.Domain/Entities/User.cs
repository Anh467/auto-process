using AutoProcess.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoProcess.Domain.Entities
{
    public class User : IAuditEntity<Guid>
    {
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset UpdatedDate { get; set; }
        public DateTimeOffset? DeletedDate { get; set; }
        public Guid Id { get; set; }
        public required string Name { get; set; 
        }

        public required string HashPassword { get; set; }

        public required string Email { get; set; } 
    }
}
