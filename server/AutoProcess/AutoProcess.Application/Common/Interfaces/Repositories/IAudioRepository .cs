using AutoProcess.Application.Common.Interfaces.Repositories.Base;
using AutoProcess.Domain.Entities;

namespace AutoProcess.Application.Common.Interfaces.Repositories
{
    public interface IAudioRepository :
        IRepository<Audio, Guid>,
        IGetAllRepository<Audio, Guid>
    {
    }
}