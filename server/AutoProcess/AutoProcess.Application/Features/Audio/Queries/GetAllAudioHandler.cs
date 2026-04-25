// Application/Features/Audio/Queries/GetAllAudio/GetAllAudioHandler.cs
using AutoProcess.Application.Common.Interfaces.Repositories;
using AutoProcess.Application.Features.CQRS;
using AutoProcess.Domain.Entities;

namespace AutoProcess.Application.Features.Audio.Queries
{
    public class GetAllAudioHandler : IQueryHandler<GetAllAudioQuery, IEnumerable<Domain.Entities.Audio>>
    {
        private readonly IAudioRepository _repository;

        public GetAllAudioHandler(IAudioRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Domain.Entities.Audio>> HandleAsync(GetAllAudioQuery query, CancellationToken cancellationToken)
            => await _repository.GetAllAsync();
    }

    public class GetAllAudioQuery : IQuery { }
}