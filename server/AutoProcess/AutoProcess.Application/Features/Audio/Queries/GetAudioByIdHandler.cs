// Application/Features/Audio/Queries/GetAllAudio/GetAllAudioHandler.cs
using AutoProcess.Application.Common.Interfaces.Repositories;
using AutoProcess.Application.Features.CQRS;
using AutoProcess.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace AutoProcess.Application.Features.Audio.Queries
{
    public class GetAudioByIdHandler : IQueryHandler<GetAudioByIdQuery, Domain.Entities.Audio?>
    {
        private readonly IAudioRepository _repository;

        public GetAudioByIdHandler(IAudioRepository repository)
        {
            _repository = repository;
        }

        public async Task<Domain.Entities.Audio?> HandleAsync(GetAudioByIdQuery query, CancellationToken cancellationToken)
            => await _repository.GetByIdAsync(query.Route.Id);
    }

    public class GetAudioByIdQuery : IQuery
    {
        public RouteParams Route { get; }

        public GetAudioByIdQuery(RouteParams route)
        {
            Route = route;
        }

        public class RouteParams
        {
            [FromRoute]
            public Guid Id { get; set; }
        }
    }
}