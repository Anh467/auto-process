using System;
using System.Collections.Generic;
using System.Text;

namespace AutoProcess.Domain.Entities.Base
{
    public interface IAuditedEntity<T> : IEntity<T>, IAudited
    {
    }
}
