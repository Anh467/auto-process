// Infrastructure/Persistence/Repositories/Base/BaseRepository.cs
using AutoProcess.Application.Common.Interfaces.Repositories.Base;
using AutoProcess.Domain.Entities.Base;
using Microsoft.EntityFrameworkCore;

namespace AutoProcess.Persistence.Repositories.Base
{
    public class BaseRepository<TEntity, TPrimaryKey> : IRepository<TEntity, TPrimaryKey>
        where TEntity : class, IEntity<TPrimaryKey>
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<TEntity> _dbSet;

        public BaseRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<TEntity>();
        }

        public async Task<TEntity?> GetByIdAsync(TPrimaryKey id)
            => await _dbSet.FindAsync(id);

        public async Task SaveAsync(TEntity entity)
        {
            var exists = await _dbSet.FindAsync(entity.Id);
            if (exists is null)
                await _dbSet.AddAsync(entity);
            else
                _dbSet.Update(entity);

            await _context.SaveChangesAsync();
        }
    }
}