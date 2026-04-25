// Application/Features/Audio/Commands/UpdateAudio/UpdateAudioHandler.cs
using AutoProcess.Application.Common.Interfaces.Repositories;
using AutoProcess.Application.Features.CQRS;
using Microsoft.AspNetCore.Mvc;

namespace AutoProcess.Application.Features.Audio.Commands
{
    public class DeleteAudioHandler : IComandHandler<DeleteAudioCommand>
    {
        private readonly IAudioRepository _repository;

        public DeleteAudioHandler(IAudioRepository repository)
        {
            _repository = repository;
        }

        public async Task HandleAsync(DeleteAudioCommand command, CancellationToken cancellationToken)
        {
            var audio = await _repository.GetByIdAsync(command.Route.Id)
         ?? throw new Exception($"Audio {command.Route.Id} not found");

            audio.DeletedAt = DateTimeOffset.UtcNow;
            await _repository.SaveAsync(audio);
        }
    }

    public class DeleteAudioCommand : IComand
    {
        public RouteParams Route { get; }

        public DeleteAudioCommand(RouteParams route)
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
