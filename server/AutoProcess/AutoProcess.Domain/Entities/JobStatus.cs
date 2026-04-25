

using AutoProcess.Core.Common.Constants;
using AutoProcess.Domain.Entities.Base;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;

namespace AutoProcess.Domain.Entities
{
    [Table(nameof(JobStatus), Schema = DatabaseConstant.Schema.DBO)]
    public class JobStatus : IEntity<int>
    {
        public int Id { get; set; }
        public required string Code { get; set; }
        public required string Name { get; set; }

        public ICollection<Job> Jobs { get; set; } = [];
    }
}
