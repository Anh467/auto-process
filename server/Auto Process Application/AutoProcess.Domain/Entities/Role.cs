using AutoProcess.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoProcess.Domain.Entities
{
    public class Role : IEntity<int>
    {
        public int Id { get; set; }
        public required string Code {get; set;}
        public required string Name { get; set;}
    }
}
