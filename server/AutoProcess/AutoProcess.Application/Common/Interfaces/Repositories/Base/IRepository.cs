using AutoProcess.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoProcess.Application.Common.Interfaces.Repositories.Base
{
    public interface IRepository<TEntity, in TPrimaryKey> where TEntity : IEntity<TPrimaryKey>
    {
        Task<TEntity?> GetByIdAsync(TPrimaryKey id);
        Task SaveAsync(TEntity entity);
    }
}
