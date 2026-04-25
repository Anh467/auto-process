using System;
using System.Collections.Generic;
using System.Text;

namespace AutoProcess.Domain.Entities.Base
{
    public interface IEntity<T>
    {
        T Id { get; set; }
    }
}
