// Infrastructure/Persistence/Repositories/AudioRepository.cs
using AutoProcess.Application.Common.Interfaces.Repositories;
using AutoProcess.Domain.Entities;
using AutoProcess.Persistence.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace AutoProcess.Persistence.Repositories
{
    public class AudioRepository : BaseRepository<Audio, Guid>, IAudioRepository
    {
        public AudioRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Audio>> GetAllAsync()
            => await _dbSet.ToListAsync();
    }
}